using System.Collections.Generic;

/// <summary>
/// Multiple choice question implementation.
/// First question type supported; others can be added later by implementing IQuestion.
/// </summary>
[System.Serializable]
public class MultipleChoiceQuestion : IQuestion
{
    public string QuestionType => "multiple_choice";

    public string QuestionText { get; set; }
    public List<string> Options { get; set; } = new List<string>();
    public string CorrectAnswer { get; set; }
    public float Difficulty { get; set; } = 0.5f;
    public string SkillFocus { get; set; }
    public string Explanation { get; set; }
    public string CommonMistakeExplanation { get; set; }
    public int EstimatedTimeSeconds { get; set; } = 30;

    /// <summary>
    /// Checks if a student's answer is correct.
    /// </summary>
    public bool CheckAnswer(string studentAnswer)
    {
        if (string.IsNullOrEmpty(studentAnswer) || string.IsNullOrEmpty(CorrectAnswer))
            return false;

        return CorrectAnswer.Equals(studentAnswer, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that answer is one of the available options.
    /// </summary>
    public bool IsValidOption(string answer)
    {
        return Options.Contains(answer);
    }

    /// <summary>
    /// Gets the index of the correct answer in the options list.
    /// </summary>
    public int GetCorrectAnswerIndex()
    {
        return Options.IndexOf(CorrectAnswer);
    }

    public override string ToString()
    {
        return $"Q: {QuestionText} | Options: {Options.Count} | Difficulty: {Difficulty:F2}";
    }
}
