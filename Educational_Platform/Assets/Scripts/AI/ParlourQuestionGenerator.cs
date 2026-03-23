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

        string personalityBlock = BuildPersonalityBlock(player);
        string historyBlock     = BuildHistoryBlock(player, debugLog);
        string memorablesBlock  = ExtractMemorables(player, debugLog);

        string context = $"- Player: {player.Name}\n" +
                         $"- Progress: {expTier}\n" +
                         $"- Steps completed overall: {completedCount}\n" +
                         $"- {weakText}\n" +
                         $"- Current streak in this step: {player.StreakInCurrentStep}\n" +
                         personalityBlock + "\n" +
                         memorablesBlock + "\n" +
                         historyBlock;

        debugLog.AppendLine($"Player context: {expTier}, {completedCount} steps done, {player.QuestionHistory?.Count ?? 0} history entries, {player.PersonalityProfile?.Count ?? 0} personality traits");
        return context;
    }

    private string BuildPersonalityBlock(Player player)
    {
        var profile = player.PersonalityProfile;
        if (profile == null || profile.Count == 0)
            return "- Communication personality: not enough data yet";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("- Communication personality (derived from answer patterns):");
        foreach (var kv in profile.OrderByDescending(p => p.Value))
        {
            string tier = kv.Value >= 0.80f ? "very strong"
                        : kv.Value >= 0.65f ? "solid"
                        : kv.Value >= 0.45f ? "developing"
                        : "struggling";
            sb.AppendLine($"  • {kv.Key}: {kv.Value * 100f:F0}/100 ({tier})");
        }
        return sb.ToString().TrimEnd();
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
            string ans     = r.StudentAnswer?.Length > 45 ? r.StudentAnswer.Substring(0, 42) + "…" : (r.StudentAnswer ?? "?");

            if (r.AnswerScore >= 0 && !string.IsNullOrEmpty(r.SkillFocus))
            {
                // Parlour entry — show the communication act, not the surface content
                string scoreLabel = r.AnswerScore >= 85 ? "strong" : r.AnswerScore >= 70 ? "decent" : r.AnswerScore >= 40 ? "weak" : "poor";
                string bestNote   = (!r.IsCorrect && !string.IsNullOrEmpty(r.BestAnswer))
                    ? $" | best was: \"{(r.BestAnswer.Length > 40 ? r.BestAnswer.Substring(0, 37) + "…" : r.BestAnswer)}\""
                    : "";
                sb.AppendLine($"  {mark} {subject}{step} | Skill: {r.SkillFocus} | Score: {r.AnswerScore}/100 ({scoreLabel}) | Chose: \"{ans}\"{bestNote}{timing}");
            }
            else
            {
                // Non-parlour entry — standard format
                string q     = r.QuestionText?.Length > 50 ? r.QuestionText.Substring(0, 47) + "…" : (r.QuestionText ?? "?");
                string error = (!r.IsCorrect && !string.IsNullOrEmpty(r.ErrorType)) ? $" [{r.ErrorType}]" : "";
                sb.AppendLine($"  {mark} {subject}{step} | Q: \"{q}\" | A: \"{ans}\"{timing}{error}");
            }
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
You are NOT reading a report — you are a character living in the same world as the player, who naturally remembers things that happened.

CRITICAL — relationship and tone:
  The relationship between {character.Name} and the player is already established. Do NOT open with greetings, introductions, or ""Hey there!"" type openers. Jump straight into the scene as if mid-conversation or resuming from where you left off.

CRITICAL — how to reference the past:
  NEVER quote the player's previous answer verbatim: e.g. ""I remember you saying 'yeah, a cappuccino sounds good'""
  NEVER frame it as a memory recall: ""last time you said..."", ""I remember you answering...""
  NEVER reference the surface content of an answer (the topic, the drink, the object) as if it were a personal preference — the player was choosing a COMMUNICATION STYLE, not expressing a preference. If the history shows ""Chose: 'I'd go with the latte' | Score: 40/100 | Skill: Tone Matching"", the relevant insight is that they gave a flat, unengaged answer on tone-matching — NOT that they like lattes.
  DO reference the communication pattern: ""You tend to go direct when things feel uncertain."" / ""Last time you kept it surface-level when things got personal."" / ""You came back strong after that one.""
  DO treat past scores and skills as what they are — evidence of how the player communicates, not what they ordered or said.
  The character has absorbed these patterns into their understanding of who the player IS — not what they literally said.

Pick AT MOST ONE specific moment or pattern to reference. Weave it in naturally, don't announce it.

How {character.Name} would express this (stay true to their voice):
  - If warm/celebratory (e.g. Maya): bring up a win like it's shared history — ""You totally came back on that one — I was rooting for you!""
  - If formal/precise (e.g. Victor): note it as an observable pattern, not a memory — ""Your instinct in X situations tends to be [pattern]. Let's test that.""
  - If playful/sarcastic (e.g. Zoe, Alex): tease the fact, not the quote — ""So. Cappuccino. Bold."" / ""You really went for that one, huh.""
  - If analytical/curious (e.g. Dr. Chen): absorb it as data about the person — ""Interesting — you tend to go direct when under pressure.""

The goal is for the player to feel seen — surprised by how personal this feels, not reminded they're in a quiz.

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

Step 3 — Write exactly 4 options. Each option is a SPOKEN RESPONSE with a quality SCORE (0–100).
  SCORE TIERS:
    85–100: ideal — clearly demonstrates ""{c.SkillFocus}"" in this specific context
    55–75 : decent — shows some awareness but misses a nuance or reads tone slightly off
    25–50 : weak — a recognisable mistake: too blunt, deflects, wrong social register
    0–20  : poor — tone-deaf, inappropriate, or completely ignores the situation

  RULES:
  - Scores do NOT have to be in order — mix them up across the 4 options
  - There can be TWO options with scores >= 70 (the player is not punished for either)
  - NO option can be a meta-description like ""Respond warmly"" — each must be actual spoken words
  - NO options that are mere agreement/disagreement (""Yes I agree"", ""I think so too"")
  - All 4 options must be specific to THIS scene, not generic
  - Keep each option under 15 words

Step 4 — Write a 1-sentence explanation of what makes the highest-scored answer the strongest.

Return ONLY valid JSON with no markdown, no extra text:

{{
  ""dialogue"": ""<character's dialogue, personal + situational>"",
  ""question"": ""What do you say?"",
  ""options"": [
    {{""text"": ""<spoken response>"", ""score"": <0-100>}},
    {{""text"": ""<spoken response>"", ""score"": <0-100>}},
    {{""text"": ""<spoken response>"", ""score"": <0-100>}},
    {{""text"": ""<spoken response>"", ""score"": <0-100>}}
  ],
  ""explanation"": ""<one sentence: what makes the top-scored answer best for {c.SkillFocus}>"",
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

            var options = new List<ConversationOption>();
            if (data.options != null)
                foreach (var o in data.options)
                    if (o != null && !string.IsNullOrWhiteSpace(o.text))
                        options.Add(new ConversationOption { Text = o.text, Score = Mathf.Clamp(o.score, 0, 100) });

            return new ConversationQuestion
            {
                CharacterId       = character.Id,
                CharacterName     = character.Name,
                CharacterDialogue = data.dialogue,
                QuestionText      = data.question,
                Options           = options,
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
        if (q.CorrectAnswer == null)
            { reason = "No scoreable option (all options have score 0?)"; return false; }

        // At least one option must score >= 70
        if (!q.Options.Any(o => o.Score >= 70))
            { reason = "No option scores >= 70 — question has no good answer"; return false; }

        // Reject meta-description options — options must be actual spoken dialogue
        string[] metaPhrases = { "respond warmly", "respond formally", "respond with", "use a", "give a ",
                                  "ignore the", "say nothing", "agree politely", "agree and", "disagree",
                                  "be formal", "be casual", "be polite", "match the tone", "show empathy",
                                  "acknowledge", "use formal", "use casual", "act professionally" };
        foreach (var opt in q.Options)
        {
            string lower = opt.Text?.ToLower() ?? "";
            foreach (var phrase in metaPhrases)
                if (lower.StartsWith(phrase) || lower.Contains(" " + phrase))
                    { reason = $"Option is a meta-description: \"{opt.Text}\""; return false; }
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
        string skillLower = skill.ToLower();

        if (skillLower.Contains("active listening") || skillLower.Contains("listen"))
            return MakeFallback(charId, charName, skill,
                $"{charName} finishes explaining something personal, then pauses and looks at you expectantly.",
                new[] {
                    "That must have been really hard. How did you feel afterward?",
                    "Interesting. Anyway, let me tell you about my day.",
                    "Got it. So what do you need from me?",
                    "Yeah, people always say that kind of stuff."
                },
                new[] { 95, 15, 45, 20 },
                "Asking a follow-up about their feelings shows you were truly listening and invites them to continue.");

        if (skillLower.Contains("tone") || skillLower.Contains("register") || skillLower.Contains("formal"))
            return MakeFallback(charId, charName, skill,
                $"{charName} is your new manager and greets you for the first time at the office.",
                new[] {
                    "Great to meet you! I'm looking forward to working with you.",
                    "Hey, nice one! This is gonna be fun, right?",
                    "Hello. I am present and ready for work.",
                    "So like… are you strict or what?"
                },
                new[] { 90, 40, 30, 10 },
                "Warm but professional language matches the first-impression context without being overly casual or robotic.");

        if (skillLower.Contains("empathy") || skillLower.Contains("support"))
            return MakeFallback(charId, charName, skill,
                $"{charName} tells you their project got cancelled after months of work, looking deflated.",
                new[] {
                    "That's really disappointing — all that effort you put in.",
                    "Well, at least now you have time for other things.",
                    "Yeah, that happens. So, what's next on your list?",
                    "You should have seen it coming honestly."
                },
                new[] { 92, 35, 50, 5 },
                "Acknowledging the effort and emotional impact first shows genuine empathy before moving forward.");

        if (skillLower.Contains("small talk") || skillLower.Contains("casual"))
            return MakeFallback(charId, charName, skill,
                $"{charName} is in the elevator with you and says: \"Rough week, huh?\"",
                new[] {
                    "Ha, just a bit! Yours going okay?",
                    "I prefer not to discuss personal matters in public.",
                    "It was fine.",
                    "Don't even get me started. Last Tuesday..."
                },
                new[] { 88, 15, 40, 30 },
                "A light, reciprocal reply keeps the small talk natural without oversharing or shutting it down.");

        if (skillLower.Contains("assertive") || skillLower.Contains("boundary") || skillLower.Contains("disagree"))
            return MakeFallback(charId, charName, skill,
                $"{charName} keeps interrupting you mid-sentence during a group discussion.",
                new[] {
                    "Could I finish my thought? I want to make sure my point lands.",
                    "Sorry, go ahead, it doesn't matter.",
                    "Stop interrupting me, that's so rude!",
                    "..."
                },
                new[] { 93, 20, 15, 5 },
                "Calmly asserting your turn without aggression or giving up is the hallmark of assertive communication.");

        // Generic fallback
        return MakeFallback(charId, charName, skill,
            $"{charName} asks: \"I've heard a lot about you — what makes you tick?\"",
            new[] {
                "I'm really driven by learning new things and solving tricky problems.",
                "I don't know, I'm just normal I guess.",
                "That's a broad question. Can you be more specific?",
                "I work hard and I am a team player."
            },
            new[] { 90, 25, 60, 35 },
            "A genuine, specific answer demonstrates self-awareness and engages the conversation meaningfully.");
    }

    private ConversationQuestion MakeFallback(string charId, string charName, string skill,
        string dialogue, string[] optionTexts, int[] scores, string explanation)
    {
        var options = new List<ConversationOption>();
        for (int i = 0; i < optionTexts.Length; i++)
            options.Add(new ConversationOption { Text = optionTexts[i], Score = i < scores.Length ? scores[i] : 30 });

        return new ConversationQuestion
        {
            CharacterId       = charId,
            CharacterName     = charName,
            CharacterDialogue = dialogue,
            QuestionText      = "What do you say?",
            Options           = options,
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
    private class OptionJSON
    {
        public string text;
        public int    score;
    }

    [Serializable]
    private class ConversationJSON
    {
        public string     dialogue;
        public string     question;
        public OptionJSON[] options;
        public string     explanation;
        public string     toneContext;
        public string     skillFocus;
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
