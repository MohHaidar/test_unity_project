using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Simplified Player model for AI-driven adaptive learning.
/// Tracks step-based progression with mastery metrics.
/// Supports resume, replay, and step navigation.
/// </summary>
[System.Serializable]
public class Player
{
    public string Id { get; set; }  // UUID assigned by Supabase
    public string Name { get; set; }

    // Step-based navigation (runtime helpers — also persisted as current_step_id FK)
    public string CurrentSubject { get; set; } = "Math";
    public string CurrentChallenge { get; set; } = "addition";
    public int CurrentStep { get; set; } = 1;
    public string CurrentStepId { get; set; }  // UUID FK → steps.id (primary DB reference)

    // Step-specific metrics
    // Key format: step UUID string
    public Dictionary<string, float> MasteryByStep { get; set; } = new Dictionary<string, float>();
    public int StreakInCurrentStep { get; set; } = 0;
    public int QuestionsInCurrentStep { get; set; } = 0;

    // Progress and rewards
    // Persistent currency and experience
    public int Coins { get; set; } = 0;
    public int TotalExp { get; set; } = 0;

    // Completed steps tracking — contains step UUIDs
    public List<string> CompletedSteps { get; set; } = new List<string>();

    /// <summary>
    /// Verbal communication personality tendencies, keyed by skill_focus.
    /// Value is a rolling average score (0.0–1.0) from parlour answers.
    /// Higher = stronger in that skill. Used to personalise parlour character behaviour.
    /// </summary>
    public Dictionary<string, float> PersonalityProfile { get; set; } = new Dictionary<string, float>();

    /// <summary>
    /// Updates the rolling personality average for a skill with a new score (0.0–1.0).
    /// Uses exponential moving average (alpha=0.3) so recent answers carry more weight.
    /// </summary>
    public void UpdatePersonality(string skill, float score01)
    {
        if (string.IsNullOrEmpty(skill)) return;
        const float alpha = 0.3f;
        float current = PersonalityProfile.TryGetValue(skill, out float v) ? v : 0.5f;
        PersonalityProfile[skill] = current + alpha * (score01 - current);
        LastUpdated = DateTime.Now;
    }

    /// <summary>Marks a step completed by its UUID.</summary>
    public void MarkStepCompleted(string stepId)
    {
        if (!string.IsNullOrEmpty(stepId) && !CompletedSteps.Contains(stepId))
        {
            CompletedSteps.Add(stepId);
            LastUpdated = DateTime.Now;
        }
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        TotalExp += amount;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Adds coins (currency) to the player.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        LastUpdated = DateTime.Now;
    }

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
    /// Updates mastery for the current step (keyed by CurrentStepId UUID).
    /// </summary>
    public void UpdateStepMastery(float newMastery)
    {
        string key = GetCurrentStepKey();
        newMastery = Mathf.Clamp01(newMastery);
        MasteryByStep[key] = newMastery;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Gets mastery for a step by UUID (preferred) or legacy composite key.
    /// </summary>
    public float GetStepMastery(string stepIdOrKey)
    {
        return MasteryByStep.TryGetValue(stepIdOrKey, out float m) ? m : 0f;
    }

    /// <summary>Legacy overload: looks up by subject/challenge/stepNumber composite key.</summary>
    public float GetStepMastery(string subject, string challenge, int step)
    {
        return MasteryByStep.TryGetValue($"{subject}:{challenge}:{step}", out float m) ? m : 0f;
    }

    /// <summary>
    /// Gets mastery for the current step. Uses CurrentStepId (UUID) when available.
    /// </summary>
    public float GetCurrentStepMastery()
    {
        if (!string.IsNullOrEmpty(CurrentStepId) && MasteryByStep.ContainsKey(CurrentStepId))
            return MasteryByStep[CurrentStepId];
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
    /// Selects a different challenge/step. Optionally supply the step UUID.
    /// </summary>
    public void SelectStep(string challenge, int stepNumber, string stepId = null)
    {
        CurrentChallenge = challenge;
        CurrentStep = stepNumber;
        if (stepId != null) CurrentStepId = stepId;
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
    /// Returns CurrentStepId (UUID) when set, otherwise legacy composite key.
    /// </summary>
    private string GetCurrentStepKey()
    {
        return !string.IsNullOrEmpty(CurrentStepId)
            ? CurrentStepId
            : $"{CurrentSubject}:{CurrentChallenge}:{CurrentStep}";
    }

    /// <summary>
    /// Returns the stage number of the player's currently selected challenge.
    /// </summary>
    public int GetCurrentStageNumber(List<Challenge> allChallenges)
    {
        if (allChallenges == null) return 1;
        var c = allChallenges.FirstOrDefault(ch =>
            ch.Slug.Equals(CurrentChallenge, StringComparison.OrdinalIgnoreCase) ||
            ch.Name.Equals(CurrentChallenge,  StringComparison.OrdinalIgnoreCase));
        return c?.StageNumber ?? 1;
    }

    /// <summary>
    /// Returns the stage name of the player's currently selected challenge.
    /// </summary>
    public string GetCurrentStageName(List<Challenge> allChallenges)
    {
        if (allChallenges == null) return "";
        var c = allChallenges.FirstOrDefault(ch =>
            ch.Slug.Equals(CurrentChallenge, StringComparison.OrdinalIgnoreCase) ||
            ch.Name.Equals(CurrentChallenge,  StringComparison.OrdinalIgnoreCase));
        return c?.StageName ?? "";
    }

    /// <summary>
    /// Returns (completedChallenges, totalChallenges) for a given stage in a subject.
    /// A challenge counts as completed when all its steps are in CompletedSteps.
    /// </summary>
    public (int completed, int total) GetStageProgress(string subject, int stageNumber, List<Challenge> allChallenges)
    {
        if (allChallenges == null) return (0, 0);
        var stageChallenges = allChallenges
            .Where(c => c.Subject == subject && c.StageNumber == stageNumber)
            .ToList();
        int total = stageChallenges.Count;
        int completed = stageChallenges.Count(c =>
            c.Steps.Count > 0 && c.Steps.All(s => CompletedSteps.Contains(s.Id)));
        return (completed, total);
    }

    /// <summary>
    /// Returns 0–1 overall progress through all challenges in a subject (step-level granularity).
    /// </summary>
    public float GetSubjectProgress(string subject, List<Challenge> allChallenges)
    {
        if (allChallenges == null) return 0f;
        var subjectChallenges = allChallenges.Where(c => c.Subject == subject).ToList();
        int totalSteps = subjectChallenges.Sum(c => c.Steps.Count);
        if (totalSteps == 0) return 0f;
        int doneSteps = subjectChallenges.Sum(c => c.Steps.Count(s => CompletedSteps.Contains(s.Id)));
        return (float)doneSteps / totalSteps;
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

    // Cross-subject context — set by QuestionFlowManager at answer time
    public string SubjectName { get; set; }   // e.g. "Math", "Verbal Communication"
    public string ChallengeSlug { get; set; } // e.g. "multiplication_1", "parlour_coffee_shop"
    public string StepDescription { get; set; } // e.g. "Active Listening"

    public override string ToString()
    {
        return $"{(IsCorrect ? "✓" : "✗")} {QuestionText} → {StudentAnswer} ({TimeTakenSeconds:F1}s)";
    }
}
