using System;
using System.Collections.Generic;

/// <summary>
/// Simplified Player model for AI-driven adaptive learning.
/// Tracks step-based progression with mastery metrics.
/// Supports resume, replay, and step navigation.
/// </summary>
[System.Serializable]
public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Step-based navigation
    public string CurrentSubject { get; set; } = "Math";
    public string CurrentChallenge { get; set; } = "Addition";
    public int CurrentStep { get; set; } = 1;

    // Step-specific metrics
    // Key format: "{subject}:{challenge}:{step}"
    public Dictionary<string, float> MasteryByStep { get; set; } = new Dictionary<string, float>();
    public int StreakInCurrentStep { get; set; } = 0;
    public int QuestionsInCurrentStep { get; set; } = 0;

    // Overall history
    public List<QuestionResult> QuestionHistory { get; set; } = new List<QuestionResult>();
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>
    /// Records a student's answer to a question in the current step.
    /// Updates streak and adds to history.
    /// </summary>
    public void RecordAnswer(QuestionResult result)
    {
        QuestionsInCurrentStep++;

        if (result.IsCorrect)
        {
            StreakInCurrentStep++;
        }
        else
        {
            StreakInCurrentStep = 0;
        }

        // Keep only last 50 answers
        QuestionHistory.Add(result);
        if (QuestionHistory.Count > 50)
        {
            QuestionHistory.RemoveAt(0);
        }

        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Updates mastery for the current step.
    /// </summary>
    public void UpdateStepMastery(float newMastery)
    {
        string key = GetCurrentStepKey();
        newMastery = Mathf.Clamp01(newMastery);
        MasteryByStep[key] = newMastery;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Gets mastery for a specific step.
    /// </summary>
    public float GetStepMastery(string subject, string challenge, int step)
    {
        string key = $"{subject}:{challenge}:{step}";
        return MasteryByStep.ContainsKey(key) ? MasteryByStep[key] : 0.0f;
    }

    /// <summary>
    /// Gets mastery for the current step.
    /// </summary>
    public float GetCurrentStepMastery()
    {
        return GetStepMastery(CurrentSubject, CurrentChallenge, CurrentStep);
    }

    /// <summary>
    /// Advances to the next step in the current challenge.
    /// </summary>
    public void AdvanceToNextStep()
    {
        CurrentStep++;
        StreakInCurrentStep = 0;
        QuestionsInCurrentStep = 0;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Restarts the current step (clears progress but keeps mastery).
    /// </summary>
    public void RestartCurrentStep()
    {
        StreakInCurrentStep = 0;
        QuestionsInCurrentStep = 0;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Selects a different challenge/step to work on.
    /// </summary>
    public void SelectStep(string challenge, int stepNumber)
    {
        CurrentChallenge = challenge;
        CurrentStep = stepNumber;
        StreakInCurrentStep = 0;
        QuestionsInCurrentStep = 0;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Resets progress for a step (used for replay).
    /// Keeps mastery so player can see improvement.
    /// </summary>
    public void ReplayStep(int stepNumber)
    {
        CurrentStep = stepNumber;
        StreakInCurrentStep = 0;
        QuestionsInCurrentStep = 0;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Gets the key used in MasteryByStep dictionary.
    /// </summary>
    private string GetCurrentStepKey()
    {
        return $"{CurrentSubject}:{CurrentChallenge}:{CurrentStep}";
    }

    public override string ToString()
    {
        return $"{Name} | {CurrentSubject}/{CurrentChallenge}/Step {CurrentStep} | Mastery: {GetCurrentStepMastery():F2} | Streak: {StreakInCurrentStep}";
    }
}

/// <summary>
/// Represents a single question result/answer.
/// </summary>
[System.Serializable]
public class QuestionResult
{
    public string QuestionText { get; set; }
    public string StudentAnswer { get; set; }
    public string CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public float TimeTakenSeconds { get; set; }
    public float Difficulty { get; set; }
    public string ErrorType { get; set; } // "conceptual_gap", "careless_mistake", "timing_issue", or null if correct
    public DateTime AnsweredAt { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"{(IsCorrect ? "✓" : "✗")} {QuestionText} → {StudentAnswer} ({TimeTakenSeconds:F1}s)";
    }
}
