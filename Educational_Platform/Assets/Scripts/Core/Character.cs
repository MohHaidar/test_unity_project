using System;

/// <summary>
/// Represents a virtual character that appears in parlour challenges.
/// Characters have distinct personalities and speaking styles that drive
/// how the AI generator frames questions and dialogue.
/// </summary>
[Serializable]
public class Character
{
    /// <summary>UUID matching characters.id in Supabase.</summary>
    public string Id { get; set; }

    /// <summary>Display name shown in the UI (e.g. "Maya", "Dr. Chen").</summary>
    public string Name { get; set; }

    /// <summary>
    /// Free-text description of the character's personality, injected into
    /// the AI prompt to shape how they speak and react.
    /// </summary>
    public string PersonalityDescription { get; set; }

    /// <summary>
    /// Describes the character's linguistic style (formality, humour, sentence structure).
    /// Injected directly into the prompt CHARACTER BLOCK.
    /// </summary>
    public string SpeakingStyle { get; set; }

    /// <summary>
    /// Key used to load the character's portrait sprite from Resources/Characters/.
    /// Falls back to a generic placeholder if the asset is missing.
    /// </summary>
    public string AvatarKey { get; set; }

    /// <summary>Optional subject UUID this character is scoped to. Null = universal.</summary>
    public string SubjectId { get; set; }

    public Character() { }

    public Character(string id, string name, string personality, string speakingStyle, string avatarKey, string subjectId = null)
    {
        Id                    = id;
        Name                  = name;
        PersonalityDescription = personality;
        SpeakingStyle         = speakingStyle;
        AvatarKey             = avatarKey;
        SubjectId             = subjectId;
    }

    public override string ToString() => $"Character({Name})";
}
