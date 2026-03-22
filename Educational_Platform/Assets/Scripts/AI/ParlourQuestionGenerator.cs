using System;
using System.Collections.Generic;
using System.Linq;
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
The character may subtly reference the player's journey if it fits naturally — never force it.

=== SCENE ===
{c.Scene}

=== SKILL BEING PRACTICED ===
{c.SkillFocus}
Step description: {step.Description}
Difficulty note: {c.DifficultyNote}

=== YOUR TASK ===

Step 1 — Write SHORT dialogue from {character.Name} (2–3 sentences) that:
  - Creates a SITUATION or MOMENT that REQUIRES the player to apply ""{c.SkillFocus}""
  - The situation must NOT have an obvious single answer — the player needs to think
  - Do NOT make it an open invitation like ""What do you think?"" or ""Tell me about yourself""
  - The character should say or do something that CHALLENGES the player to use the skill

Step 2 — Write the response question as: ""What do you say?""

Step 3 — Write exactly 4 options. CRITICAL RULES FOR OPTIONS:
  ✅ Each option MUST be actual words the player would speak aloud — like real dialogue
  ✅ Each option must DEMONSTRATE (or fail to demonstrate) ""{c.SkillFocus}"" specifically
  ✅ The correct option should clearly apply the skill in context
  ✅ Wrong options should fail in different, specific ways (too blunt, misses the subtext, wrong register, deflects)
  ❌ NEVER write meta-descriptions like ""Respond warmly"", ""Use a formal tone"", ""Agree politely""
  ❌ NEVER write options that are just agreement/disagreement (""Yes I agree"", ""I think so too"")
  ❌ Options must NOT be generic — they must be specific to THIS scene and this character's dialogue
  Keep each option under 15 words.

Step 4 — Write a 1-sentence explanation of why the correct answer best applies ""{c.SkillFocus}"".

Return ONLY valid JSON with no markdown, no extra text:

{{
  ""dialogue"": ""<character's situational dialogue>"",
  ""question"": ""What do you say?"",
  ""options"": [""<spoken response A>"", ""<spoken response B>"", ""<spoken response C>"", ""<spoken response D>""],
  ""correct"": ""<exact text of the correct spoken response>"",
  ""explanation"": ""<one sentence: why this response best applies {c.SkillFocus}>"",
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
                Difficulty        = 0.5f
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

        // Reject meta-description options — options must be actual spoken dialogue
        string[] metaPhrases = { "respond warmly", "respond formally", "respond with", "use a", "give a ",
                                  "ignore the", "say nothing", "agree politely", "agree and", "disagree",
                                  "be formal", "be casual", "be polite", "match the tone", "show empathy",
                                  "acknowledge", "use formal", "use casual", "act professionally" };
        foreach (var opt in q.Options)
        {
            string lower = opt.ToLower();
            foreach (var phrase in metaPhrases)
                if (lower.StartsWith(phrase) || lower.Contains(" " + phrase))
                    { reason = $"Option is a meta-description, not spoken dialogue: \"{opt}\""; return false; }
        }

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

    private IQuestion GetFallbackQuestion(Step step, Character character = null, ParlourConstraints? constraints = null)
    {
        string charName = character?.Name ?? "Maya";
        string skill    = constraints?.SkillFocus ?? step?.Description ?? "verbal communication";

        // Fallback questions are skill-specific where possible, always use actual spoken dialogue as options
        return GetFallbackForSkill(skill, charName, character?.Id ?? CharacterManager.CHAR_MAYA_ID);
    }

    private ConversationQuestion GetFallbackForSkill(string skill, string charName, string charId)
    {
        // Pick a situation + real spoken-dialogue options that match the skill focus
        string skillLower = skill.ToLower();

        if (skillLower.Contains("active listening") || skillLower.Contains("listen"))
            return MakeFallback(charId, charName, skill,
                $"{charName} finishes explaining something personal, then pauses and looks at you expectantly.",
                new[] {
                    "That must have been really hard. How did you feel afterward?",
                    "Interesting. Anyway, let me tell you about my day.",
                    "Got it. So what do you need from me?",
                    "Yeah, people always say that kind of stuff."
                }, 0,
                "Asking a follow-up about their feelings shows you were truly listening and invite them to continue.");

        if (skillLower.Contains("tone") || skillLower.Contains("register") || skillLower.Contains("formal"))
            return MakeFallback(charId, charName, skill,
                $"{charName} is your new manager and greets you for the first time at the office.",
                new[] {
                    "Great to meet you! I'm looking forward to working with you.",
                    "Hey, nice one! This is gonna be fun, right?",
                    "Hello. I am present and ready for work.",
                    "So like… are you strict or what?"
                }, 0,
                "Warm but professional language matches the first-impression context without being overly casual or robotic.");

        if (skillLower.Contains("empathy") || skillLower.Contains("support"))
            return MakeFallback(charId, charName, skill,
                $"{charName} tells you their project got cancelled after months of work, looking deflated.",
                new[] {
                    "That's really disappointing — all that effort you put in.",
                    "Well, at least now you have time for other things.",
                    "Yeah, that happens. So, what's next on your list?",
                    "You should have seen it coming honestly."
                }, 0,
                "Acknowledging the effort and emotional impact first shows genuine empathy before moving forward.");

        if (skillLower.Contains("small talk") || skillLower.Contains("casual"))
            return MakeFallback(charId, charName, skill,
                $"{charName} is in the elevator with you and says: \"Rough week, huh?\"",
                new[] {
                    "Ha, just a bit! Yours going okay?",
                    "I prefer not to discuss personal matters in public.",
                    "It was fine.",
                    "Don't even get me started. Last Tuesday..."
                }, 0,
                "A light, reciprocal reply keeps the small talk natural without oversharing or shutting it down.");

        if (skillLower.Contains("assertive") || skillLower.Contains("boundary") || skillLower.Contains("disagree"))
            return MakeFallback(charId, charName, skill,
                $"{charName} keeps interrupting you mid-sentence during a group discussion.",
                new[] {
                    "Could I finish my thought? I want to make sure my point lands.",
                    "Sorry, go ahead, it doesn't matter.",
                    "Stop interrupting me, that's so rude!",
                    "..."
                }, 0,
                "Calmly asserting your turn without aggression or giving up is the hallmark of assertive communication.");

        // Generic fallback — still uses actual dialogue, not meta-descriptions
        return MakeFallback(charId, charName, skill,
            $"{charName} asks: \"I've heard a lot about you — what makes you tick?\"",
            new[] {
                "I'm really driven by learning new things and solving tricky problems.",
                "I don't know, I'm just normal I guess.",
                "That's a broad question. Can you be more specific?",
                "I work hard and I am a team player."
            }, 0,
            "A genuine, specific answer demonstrates self-awareness and engages the conversation meaningfully.");
    }

    private ConversationQuestion MakeFallback(string charId, string charName, string skill,
        string dialogue, string[] options, int correctIndex, string explanation)
    {
        return new ConversationQuestion
        {
            CharacterId       = charId,
            CharacterName     = charName,
            CharacterDialogue = dialogue,
            QuestionText      = "What do you say?",
            Options           = new List<string>(options),
            CorrectAnswer     = options[correctIndex],
            Explanation       = explanation,
            ToneContext       = skill,
            SkillFocus        = skill,
            Difficulty        = 0.3f
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
