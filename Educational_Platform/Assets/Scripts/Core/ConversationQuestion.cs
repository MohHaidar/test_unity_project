using System.Collections.Generic;

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

    /// <summary>The prompt shown beneath the character dialogue (e.g. "How do you respond?").</summary>
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

    /// <summary>The response options the player can choose from.</summary>
    public List<string> Options { get; set; } = new List<string>();

    /// <summary>The correct response (must appear in Options).</summary>
    public string CorrectAnswer { get; set; }

    /// <summary>Explanation of why the correct answer is right — shown after answer.</summary>
    public string Explanation { get; set; }

    // ── IQuestion implementation ──────────────────────────────────────────────

    public bool CheckAnswer(string studentAnswer)
    {
        if (string.IsNullOrEmpty(studentAnswer) || string.IsNullOrEmpty(CorrectAnswer))
            return false;
        return CorrectAnswer.Equals(studentAnswer, System.StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() =>
        $"Conversation({CharacterName}): \"{CharacterDialogue?.Substring(0, System.Math.Min(40, CharacterDialogue?.Length ?? 0))}...\"";
}
