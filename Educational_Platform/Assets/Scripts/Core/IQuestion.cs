/// <summary>
/// Base interface for all question types.
/// Enables extensibility: MultipleChoice, DragDrop, FreeForm, etc.
/// </summary>
public interface IQuestion
{
    /// <summary>
    /// Type of question (e.g., "multiple_choice", "drag_drop", "free_form").
    /// </summary>
    string QuestionType { get; }

    /// <summary>
    /// The question text/prompt shown to the student.
    /// </summary>
    string QuestionText { get; }

    /// <summary>
    /// Difficulty level (0.0 = easiest, 1.0 = hardest).
    /// </summary>
    float Difficulty { get; }

    /// <summary>
    /// Concept being tested (e.g., "2-digit addition with carrying").
    /// </summary>
    string SkillFocus { get; }

    /// <summary>
    /// Checks if the given answer is correct.
    /// </summary>
    bool CheckAnswer(string studentAnswer);
}
