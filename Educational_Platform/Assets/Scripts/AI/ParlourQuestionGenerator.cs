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
        string prompt = BuildPrompt(step, character, constraints, playerContext);
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

        var weakAreas = new List<string>();
        if (player.MasteryByStep != null)
            foreach (var kv in player.MasteryByStep)
                if (kv.Value < 0.6f)
                {
                    var s = ChallengeDataManager.Instance.GetStepById(kv.Key);
                    if (s != null) weakAreas.Add(s.Description);
                }

        string weakText = weakAreas.Count > 0
            ? $"Areas needing improvement: {string.Join(", ", weakAreas.Take(3))}"
            : "No significant weak areas identified yet";

        string historyBlock    = BuildHistoryBlock(player, debugLog);
        string memorablesBlock = ExtractMemorables(player, debugLog);

        string context = $"- Player: {player.Name}\n" +
                         $"- Progress: {expTier}\n" +
                         $"- Steps completed overall: {completedCount}\n" +
                         $"- {weakText}\n" +
                         $"- Current streak in this step: {player.StreakInCurrentStep}\n" +
                         memorablesBlock + "\n" +
                         historyBlock;

        debugLog.AppendLine($"Player context: {expTier}, {completedCount} steps done, {player.QuestionHistory?.Count ?? 0} history entries");
        return context;
    }

    /// <summary>
    /// Pulls 2–3 specific memorable events from history so the AI can reference them
    /// as concrete "memories" in the character's dialogue.
    /// </summary>
    private string ExtractMemorables(Player player, System.Text.StringBuilder debugLog)
    {
        var history = player.QuestionHistory;
        if (history == null || history.Count < 3)
            return "- Memorable moments: not enough history yet";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("- Memorable moments (specific events the character could reference):");

        // Last failure — what they got wrong and what they chose
        var lastFailure = history.LastOrDefault(r => !r.IsCorrect);
        if (lastFailure != null)
        {
            string subj  = !string.IsNullOrEmpty(lastFailure.SubjectName) ? $" in {lastFailure.SubjectName}" : "";
            string step  = !string.IsNullOrEmpty(lastFailure.StepDescription) ? $" ({lastFailure.StepDescription})" : "";
            string chose = lastFailure.StudentAnswer?.Length > 40 ? lastFailure.StudentAnswer.Substring(0, 37) + "…" : lastFailure.StudentAnswer;
            string right = lastFailure.CorrectAnswer?.Length > 40 ? lastFailure.CorrectAnswer.Substring(0, 37) + "…" : lastFailure.CorrectAnswer;
            sb.AppendLine($"  • Last misstep{subj}{step}: chose \"{chose}\" (correct was \"{right}\")");
        }

        // Clutch moment — a correct answer after 2+ consecutive wrong answers
        for (int i = 2; i < history.Count; i++)
        {
            if (history[i].IsCorrect && !history[i - 1].IsCorrect && !history[i - 2].IsCorrect)
            {
                string subj = !string.IsNullOrEmpty(history[i].SubjectName) ? $" in {history[i].SubjectName}" : "";
                string step = !string.IsNullOrEmpty(history[i].StepDescription) ? $" ({history[i].StepDescription})" : "";
                string ans  = history[i].StudentAnswer?.Length > 40 ? history[i].StudentAnswer.Substring(0, 37) + "…" : history[i].StudentAnswer;
                sb.AppendLine($"  • Clutch recovery{subj}{step}: finally got it with \"{ans}\" after two misses");
                break;
            }
        }

        // Best subject (most accurate, minimum 4 answers)
        var bySubject = history
            .Where(r => !string.IsNullOrEmpty(r.SubjectName))
            .GroupBy(r => r.SubjectName)
            .Where(g => g.Count() >= 4)
            .OrderByDescending(g => g.Count(r => r.IsCorrect) / (float)g.Count())
            .FirstOrDefault();
        if (bySubject != null)
        {
            float acc = bySubject.Count(r => r.IsCorrect) / (float)bySubject.Count() * 100f;
            sb.AppendLine($"  • Strongest subject: {bySubject.Key} ({acc:F0}% accuracy over {bySubject.Count()} questions)");
        }

        // Fastest wrong — a suspicious speed-run failure (answered in under 3 seconds and got it wrong)
        var fastMiss = history.Where(r => !r.IsCorrect && r.TimeTakenSeconds > 0 && r.TimeTakenSeconds < 3f).LastOrDefault();
        if (fastMiss != null)
        {
            string subj = !string.IsNullOrEmpty(fastMiss.SubjectName) ? $" in {fastMiss.SubjectName}" : "";
            sb.AppendLine($"  • Rushed mistake{subj}: wrong answer in {fastMiss.TimeTakenSeconds:F1}s — classic guessing");
        }

        debugLog.AppendLine($"Memorables extracted from {history.Count} entries");
        return sb.ToString().TrimEnd();
    }

    private string BuildHistoryBlock(Player player, System.Text.StringBuilder debugLog)
    {
        var history = player.QuestionHistory;
        if (history == null || history.Count == 0)
        {
            debugLog.AppendLine("History block: empty");
            return "- Answer history: none yet (first session)";
        }

        // Send last 20 entries — enough for both short and long-term signals
        int take = Math.Min(20, history.Count);
        var entries = history.Skip(history.Count - take).ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"- Answer history ({entries.Count} most recent, oldest → newest):");

        foreach (var r in entries)
        {
            string mark    = r.IsCorrect ? "✓" : "✗";
            string subject = !string.IsNullOrEmpty(r.SubjectName) ? $"[{r.SubjectName}]" : "[?]";
            string step    = !string.IsNullOrEmpty(r.StepDescription) ? $" · {r.StepDescription}" : "";
            string timing  = r.TimeTakenSeconds > 0 ? $" {r.TimeTakenSeconds:F0}s" : "";
            string error   = (!r.IsCorrect && !string.IsNullOrEmpty(r.ErrorType)) ? $" [{r.ErrorType}]" : "";
            string q       = r.QuestionText?.Length > 55 ? r.QuestionText.Substring(0, 52) + "…" : (r.QuestionText ?? "?");
            string ans     = r.StudentAnswer?.Length > 40 ? r.StudentAnswer.Substring(0, 37) + "…" : (r.StudentAnswer ?? "?");

            sb.AppendLine($"  {mark} {subject}{step} | Q: \"{q}\" | A: \"{ans}\"{timing}{error}");
        }

        debugLog.AppendLine($"History block: {entries.Count} entries from {history.Select(r => r.SubjectName).Where(s => s != null).Distinct().Count()} subjects");
        return sb.ToString().TrimEnd();
    }

    private string BuildPrompt(Step step, Character character, ParlourConstraints c, string playerContext)
    {
        return $@"You are generating a verbal communication exercise for an educational game.

=== CHARACTER BLOCK ===
Name: {character.Name}
Personality: {character.PersonalityDescription}
Speaking Style: {character.SpeakingStyle}

=== PLAYER CONTEXT ===
{playerContext}

=== HOW TO USE THIS HISTORY ===
Before writing, silently analyze the data above. You are looking for:
  - Short-term mood: is the player on a roll, slumping, rushing, or grinding through?
  - Long-term identity: what kind of learner are they — careful, impulsive, strong in one area, stuck in another?
  - Specific memorable moments: the missteps, clutch recoveries, and fastest guesses listed above are REAL events you witnessed.

Now express these insights through {character.Name}'s personality in the opening dialogue.
You are NOT reading a report — you are a character who has been watching and remembers.
Pick AT MOST ONE specific moment or pattern to reference. Make it feel natural, not like a data dump.

How {character.Name} would express this (stay true to their voice):
  - If warm/celebratory (e.g. Maya): bring up a win or near-miss with genuine excitement — ""I remember when you finally nailed that one after two tries!""
  - If formal/precise (e.g. Victor): cite the pattern as a matter of professional record — ""Your recent performance in X revealed a tendency toward..."" 
  - If playful/sarcastic (e.g. Zoe, Alex): tease the misstep with wit — ""Sooo... that one answer last time was... a choice."" / ""Bold move picking that. Bold.""
  - If analytical/curious (e.g. Dr. Chen): observe the pattern with fascination — ""Interesting — I noticed you tend to rush when the stakes feel lower...""

The goal is to make the player feel seen — surprised by how personal this feels.

=== SCENE ===
{c.Scene}

=== SKILL BEING PRACTICED ===
{c.SkillFocus}
Step description: {step.Description}
Difficulty note: {c.DifficultyNote}

=== YOUR TASK ===

Step 1 — Write SHORT dialogue from {character.Name} (2–3 sentences) that:
  - Opens with OR naturally includes a brief personal reference from the history above (in their voice)
  - Then creates a SITUATION that REQUIRES the player to apply ""{c.SkillFocus}""
  - The situation must NOT have an obvious single answer — the player needs to think
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
  ""dialogue"": ""<character's dialogue, personal + situational>"",
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
