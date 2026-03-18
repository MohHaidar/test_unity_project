using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A question that asks the player to fill one or more blanks.
///
/// Modes (determined by whether DragOptions is populated):
///   DragOptions empty  → player types into TMP_InputField(s)
///   DragOptions filled → player drags tokens into drop zones
///
/// Multi-blank answers are encoded as a pipe-delimited string: "answer1|answer2".
/// <see cref="CheckAnswer"/> accepts that encoding and compares each part case-insensitively,
/// trimming whitespace.
/// </summary>
public class FillInBlankQuestion : IQuestion
{
    public string QuestionType => DragOptions != null && DragOptions.Count > 0
        ? "drag_and_drop"
        : "fill_in_blank";

    public string QuestionText { get; set; }
    public float Difficulty { get; set; }
    public string SkillFocus { get; set; }

    /// <summary>Each blank the player must fill, in order.</summary>
    public List<BlankField> Blanks { get; set; } = new List<BlankField>();

    /// <summary>
    /// Token options shown when in DragAndDrop mode.
    /// Empty / null → FillInBlank (typed) mode.
    /// </summary>
    public List<string> DragOptions { get; set; } = new List<string>();

    public bool CheckAnswer(string playerAnswer)
    {
        if (Blanks == null || Blanks.Count == 0) return false;

        // Multi-blank answers arrive pipe-delimited ("9" or "3|5")
        var parts = (playerAnswer ?? "").Split('|');

        if (parts.Length != Blanks.Count) return false;

        for (int i = 0; i < Blanks.Count; i++)
        {
            if (parts[i].Trim().ToLowerInvariant() !=
                Blanks[i].CorrectAnswer.Trim().ToLowerInvariant())
                return false;
        }
        return true;
    }

    /// <summary>Returns all correct answers as a pipe-delimited string for logging.</summary>
    public string CorrectAnswerString() =>
        string.Join("|", Blanks.Select(b => b.CorrectAnswer));
}

/// <summary>One blank slot in a <see cref="FillInBlankQuestion"/>.</summary>
[System.Serializable]
public class BlankField
{
    /// <summary>Short label shown next to the input box (e.g. "x =", "answer").</summary>
    public string Label { get; set; }

    /// <summary>The single correct value the player must enter.</summary>
    public string CorrectAnswer { get; set; }
}
