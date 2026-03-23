using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A single response option in a ConversationQuestion.
/// Score 0–100 reflects quality of that response for the skill being practiced.
/// Multiple options can be valid — the player is never punished for a 70+ choice.
/// Scores are set by the AI at generation time and never shown to the player.
/// </summary>
[System.Serializable]
public class ConversationOption
{
    public string Text  { get; set; }
    public int    Score { get; set; } = 50; // 0–100; higher = stronger communication
}

/// <summary>
/// A conversation-style question used in parlour challenges.
/// A virtual character speaks first (dialogue), setting a scene or posing a situation,
/// and the player selects the best verbal response from multiple choices.
///
/// Implements IQuestion so it can be handled by the standard question flow.
/// QuestionDisplay routes to ConversationDisplay when QuestionType == "conversation".
/// </summary>
[System.Serializable]
public class ConversationQuestion : IQuestion
{
    public string QuestionType => "conversation";

    // ── IQuestion fields ──────────────────────────────────────────────────────

    /// <summary>The prompt shown beneath the character dialogue (e.g. "What do you say?").</summary>
    public string QuestionText { get; set; }

    public float Difficulty { get; set; } = 0.5f;
    public string SkillFocus { get; set; }

    // ── Conversation-specific fields ─────────────────────────────────────────

    /// <summary>ID of the character who is speaking (matches characters.id in Supabase).</summary>
    public string CharacterId { get; set; }

    /// <summary>Display name of the character (denormalized for quick rendering).</summary>
    public string CharacterName { get; set; }

    /// <summary>What the character says to open the interaction.</summary>
    public string CharacterDialogue { get; set; }

    /// <summary>Describes the emotional/social context (e.g. "tense", "playful", "formal").</summary>
    public string ToneContext { get; set; }

    /// <summary>The scored response options. Scores are hidden from the player.</summary>
    public List<ConversationOption> Options { get; set; } = new List<ConversationOption>();

    /// <summary>Text of the highest-scored option — used for streak/feedback display.</summary>
    public string CorrectAnswer => Options?.OrderByDescending(o => o.Score).FirstOrDefault()?.Text;

    /// <summary>Explanation of why the top-scoring answer is strongest — shown after answer.</summary>
    public string Explanation { get; set; }

    // ── IQuestion implementation ──────────────────────────────────────────────

    /// <summary>Returns the score (0–100) for the given option text. 0 if not found.</summary>
    public int GetOptionScore(string optionText)
    {
        if (string.IsNullOrEmpty(optionText) || Options == null) return 0;
        var match = Options.FirstOrDefault(o =>
            string.Equals(o.Text, optionText, System.StringComparison.OrdinalIgnoreCase));
        return match?.Score ?? 0;
    }

    /// <summary>Score >= 70 counts as correct for streak purposes.</summary>
    public bool CheckAnswer(string studentAnswer) => GetOptionScore(studentAnswer) >= 70;

    public override string ToString() =>
        $"Conversation({CharacterName}): \"{CharacterDialogue?.Substring(0, System.Math.Min(40, CharacterDialogue?.Length ?? 0))}...\"";
}
