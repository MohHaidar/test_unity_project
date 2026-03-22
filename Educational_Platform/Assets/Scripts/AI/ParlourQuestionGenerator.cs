using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates ConversationQuestions for parlour challenges using Ollama.
/// Builds a three-part prompt:
///   1. CHARACTER BLOCK — personality + speaking style
///   2. PLAYER CONTEXT — progress, achievements, weaknesses
///   3. STEP CONSTRAINTS — skill focus, scene, difficulty from prompt_constraints JSON
///
/// Usage: one instance per challenge session (same pattern as OllamaQuestionGenerator).
/// </summary>
public class ParlourQuestionGenerator
{
    private OllamaAPI _ollamaAPI;

    public ParlourQuestionGenerator(string model = "gpt-oss:20b-cloud")
    {
        _ollamaAPI = new OllamaAPI(model);
    }

    /// <summary>Full debug log from the most recent GenerateQuestion() call.</summary>
    public string LastGenerationDebugLog { get; private set; } = "";

    /// <summary>
    /// Generates a ConversationQuestion for the given step and player.
    /// Returns a fallback question if all Ollama attempts fail.
    /// </summary>
    public IQuestion GenerateQuestion(Player player, Step step)
    {
        var debugLog = new System.Text.StringBuilder();
        debugLog.AppendLine($"=== ParlourQuestionGenerator: {step?.Challenge} Step {step?.Number} ({step?.Description}) ===");

        if (player == null || step == null)
        {
            Debug.LogError("[ParlourGenerator] Player or Step is null");
            LastGenerationDebugLog = debugLog.ToString();
            return GetFallbackQuestion(step);
        }

        // Parse step's prompt_constraints JSON for character_id, skill_focus, scene
        ParlourConstraints constraints = ParseConstraints(step.PromptConstraints, debugLog);

        // Resolve character
        Character character = CharacterManager.Instance.GetCharacterById(constraints.CharacterId);
        if (character == null)
        {
            debugLog.AppendLine($"WARNING: character_id '{constraints.CharacterId}' not found — using Maya");
            character = CharacterManager.Instance.GetCharacterById(CharacterManager.CHAR_MAYA_ID)
                        ?? new Character(CharacterManager.CHAR_MAYA_ID, "Maya", "Warm and encouraging.", "Casual and friendly.", "maya_placeholder");
        }
        debugLog.AppendLine($"Character: {character.Name}  |  Skill: {constraints.SkillFocus}");

        // Build player context
        string playerContext = BuildPlayerContext(player, debugLog);

        // Build prompt
        string prompt = BuildPrompt(player, step, character, constraints, playerContext);
        debugLog.AppendLine($"Prompt length: {prompt.Length} chars");

        float[] temps = { 0.4f, 0.6f, 0.8f, 0.9f };
        int maxAttempts = 4;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float temp = temps[Mathf.Clamp(attempt, 0, temps.Length - 1)];
            debugLog.AppendLine($"\n--- Attempt {attempt + 1} (temp={temp}) ---");

            string response = _ollamaAPI.GenerateSync(prompt, temperature: temp);

            if (string.IsNullOrEmpty(response))
            {
                debugLog.AppendLine("REJECTED: Empty response from Ollama");
                continue;
            }

            string preview = response.Length > 300 ? response.Substring(0, 300) + "…" : response;
            debugLog.AppendLine($"Raw response ({response.Length} chars):\n{preview}");

            ConversationQuestion q = ParseResponse(response, character, debugLog);
            if (q == null)
            {
                debugLog.AppendLine("REJECTED: Parse failed");
                continue;
            }

            if (!ValidateQuestion(q, out string reason))
            {
                debugLog.AppendLine($"REJECTED: Validation — {reason}");
                Debug.LogWarning($"[ParlourGenerator] Attempt {attempt + 1}: validation failed — {reason}");
                continue;
            }

            if (IsRecentlyAsked(player, q))
            {
                debugLog.AppendLine($"REJECTED: Duplicate — \"{q.QuestionText}\"");
                continue;
            }

            debugLog.AppendLine($"\n✓ ACCEPTED on attempt {attempt + 1}");
            Debug.Log($"[ParlourGenerator] Generated on attempt {attempt + 1}: {q}");
            LastGenerationDebugLog = debugLog.ToString();
            return q;
        }

        debugLog.AppendLine($"\n✗ All {maxAttempts} attempts failed — using fallback");
        Debug.LogWarning("[ParlourGenerator] All attempts failed, returning fallback question");
        LastGenerationDebugLog = debugLog.ToString();
        return GetFallbackQuestion(step, character, constraints);
    }

    // ── Prompt building ───────────────────────────────────────────────────────

    private string BuildPlayerContext(Player player, System.Text.StringBuilder debugLog)
    {
        int completedCount = player.CompletedSteps?.Count ?? 0;
        string expTier = player.TotalExp switch
        {
            < 50  => "just starting out (fewer than 50 EXP)",
            < 200 => "making solid early progress (50–200 EXP)",
            < 500 => "intermediate learner (200–500 EXP)",
            _     => "experienced learner (500+ EXP)"
        };

        // Find any mastery gaps (steps below 0.6 mastery)
        var weakAreas = new List<string>();
        if (player.MasteryByStep != null)
            foreach (var kv in player.MasteryByStep)
                if (kv.Value < 0.6f)
                {
                    var step = ChallengeDataManager.Instance.GetStepById(kv.Key);
                    if (step != null) weakAreas.Add(step.Description);
                }

        string weakText = weakAreas.Count > 0
            ? $"Areas needing improvement: {string.Join(", ", weakAreas.Take(3))}"
            : "No significant weak areas identified yet";

        string context = $"- Player: {player.Name}\n" +
                         $"- Progress: {expTier}\n" +
                         $"- Steps completed overall: {completedCount}\n" +
                         $"- {weakText}\n" +
                         $"- Current streak in this step: {player.StreakInCurrentStep}";

        debugLog.AppendLine($"Player context: {expTier}, {completedCount} steps done");
        return context;
    }

    private string BuildPrompt(Player player, Step step, Character character, ParlourConstraints c, string playerContext)
    {
        return $@"You are generating a verbal communication exercise for an educational game.

=== CHARACTER BLOCK ===
Name: {character.Name}
Personality: {character.PersonalityDescription}
Speaking Style: {character.SpeakingStyle}

=== PLAYER CONTEXT ===
{playerContext}
The character is aware of this context and may subtly reference the player's progress, encourage them, or challenge them based on it.

=== SCENE ===
{c.Scene}

=== STEP REQUIREMENTS ===
Skill being practiced: {c.SkillFocus}
Step description: {step.Description}
Difficulty note: {c.DifficultyNote}

=== INSTRUCTIONS ===
1. Write a SHORT piece of dialogue from {character.Name} (2–3 sentences max) that:
   - Fits the scene and the character's personality/style EXACTLY
   - Naturally leads into a verbal communication question
   - May subtly reference the player's progress if natural (don't force it)
2. Write a short question prompt (""How do you respond?"" or similar) that follows the dialogue
3. Write exactly 3 response options. Only ONE is the ideal response given the skill being practiced.
   The others should be plausible but wrong in specific ways (too formal, too casual, misreads tone, etc.)
   Keep each option under 15 words — they are selectable buttons, not essays.
4. Provide a 1-sentence explanation of why the correct answer is right.

Return ONLY valid JSON with no markdown, no extra text:

{{
  ""dialogue"": ""<character's opening speech>"",
  ""question"": ""<short prompt asking the player how to respond>"",
  ""options"": [""<option A>"", ""<option B>"", ""<option C>""],
  ""correct"": ""<exact text of the correct option>"",
  ""explanation"": ""<why this response is correct>"",
  ""toneContext"": ""{c.SkillFocus}"",
  ""skillFocus"": ""{c.SkillFocus}""
}}";
    }

    // ── Response parsing ──────────────────────────────────────────────────────

    private ConversationQuestion ParseResponse(string response, Character character, System.Text.StringBuilder debugLog)
    {
        try
        {
            int jsonStart = response.IndexOf('{');
            int jsonEnd   = response.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                debugLog.AppendLine("PARSE ERROR: No JSON object found");
                return null;
            }
            string json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);

            ConversationJSON data = JsonUtility.FromJson<ConversationJSON>(json);
            if (data == null)
            {
                debugLog.AppendLine("PARSE ERROR: JsonUtility returned null");
                return null;
            }

            var options = new List<string>();
            if (data.options != null) options.AddRange(data.options);

            return new ConversationQuestion
            {
                CharacterId       = character.Id,
                CharacterName     = character.Name,
                CharacterDialogue = data.dialogue,
                QuestionText      = data.question,
                Options           = options,
                CorrectAnswer     = data.correct,
                Explanation       = data.explanation,
                ToneContext       = data.toneContext,
                SkillFocus        = data.skillFocus,
                Difficulty        = 0.5f,
                AllowFreeResponse = true
            };
        }
        catch (Exception e)
        {
            debugLog.AppendLine($"PARSE ERROR: {e.Message}");
            Debug.LogError($"[ParlourGenerator] Parse error: {e.Message}");
            return null;
        }
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private bool ValidateQuestion(ConversationQuestion q, out string reason)
    {
        reason = null;
        if (string.IsNullOrWhiteSpace(q.CharacterDialogue))
            { reason = "Empty character dialogue"; return false; }
        if (string.IsNullOrWhiteSpace(q.QuestionText))
            { reason = "Empty question text"; return false; }
        if (q.Options == null || q.Options.Count < 2)
            { reason = $"Too few options ({q.Options?.Count ?? 0} — need at least 2)"; return false; }
        if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
            { reason = "Empty correct answer"; return false; }
        if (!q.Options.Contains(q.CorrectAnswer))
            { reason = $"Correct answer not in options: \"{q.CorrectAnswer}\""; return false; }
        return true;
    }

    private bool IsRecentlyAsked(Player player, ConversationQuestion q)
    {
        int recentCount = Math.Min(10, player.QuestionHistory.Count);
        int startIdx    = Math.Max(0, player.QuestionHistory.Count - recentCount);
        for (int i = startIdx; i < player.QuestionHistory.Count; i++)
            if (player.QuestionHistory[i].QuestionText == q.CharacterDialogue)
                return true;
        return false;
    }

    // ── Fallback question ─────────────────────────────────────────────────────

    private IQuestion GetFallbackQuestion(Step step, Character character = null, ParlourConstraints constraints = null)
    {
        string charName = character?.Name ?? "Maya";
        string skill    = constraints?.SkillFocus ?? step?.Description ?? "verbal communication";

        return new ConversationQuestion
        {
            CharacterId       = character?.Id ?? CharacterManager.CHAR_MAYA_ID,
            CharacterName     = charName,
            CharacterDialogue = $"Hey! Let's work on {skill} together. I have a quick question for you.",
            QuestionText      = "Which response best fits this situation?",
            Options           = new List<string>
            {
                "Respond warmly and match the conversational tone",
                "Give a very formal and distant response",
                "Respond with a one-word answer"
            },
            CorrectAnswer     = "Respond warmly and match the conversational tone",
            Explanation       = "Matching tone and showing engagement is the hallmark of effective informal communication.",
            ToneContext       = skill,
            SkillFocus        = skill,
            Difficulty        = 0.3f,
            AllowFreeResponse = true
        };
    }

    // ── Constraint parsing ────────────────────────────────────────────────────

    /// <summary>
    /// Parses prompt_constraints JSON string into a ParlourConstraints struct.
    /// Expected format: {"character_id":"...","skill_focus":"...","scene":"...","difficulty_note":"..."}
    /// </summary>
    private ParlourConstraints ParseConstraints(string json, System.Text.StringBuilder debugLog)
    {
        var result = new ParlourConstraints
        {
            CharacterId     = CharacterManager.CHAR_MAYA_ID,
            SkillFocus      = "verbal communication",
            Scene           = "a casual conversation",
            DifficultyNote  = ""
        };

        if (string.IsNullOrWhiteSpace(json)) return result;

        try
        {
            var data = JsonUtility.FromJson<ConstraintsJSON>(json);
            if (data == null) return result;
            if (!string.IsNullOrEmpty(data.character_id))    result.CharacterId    = data.character_id;
            if (!string.IsNullOrEmpty(data.skill_focus))     result.SkillFocus     = data.skill_focus;
            if (!string.IsNullOrEmpty(data.scene))           result.Scene          = data.scene;
            if (!string.IsNullOrEmpty(data.difficulty_note)) result.DifficultyNote = data.difficulty_note;
        }
        catch (Exception e)
        {
            debugLog.AppendLine($"WARNING: Failed to parse prompt_constraints — {e.Message}");
        }

        return result;
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    [Serializable]
    private class ConversationJSON
    {
        public string   dialogue;
        public string   question;
        public string[] options;
        public string   correct;
        public string   explanation;
        public string   toneContext;
        public string   skillFocus;
    }

    [Serializable]
    private class ConstraintsJSON
    {
        public string character_id;
        public string skill_focus;
        public string scene;
        public string difficulty_note;
    }

    private struct ParlourConstraints
    {
        public string CharacterId;
        public string SkillFocus;
        public string Scene;
        public string DifficultyNote;
    }
}
