using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single step within a challenge.
/// A step contains multiple questions and has a streak goal.
/// Designed to support future "Ultimate Challenge" feature (mastery check after 5-streak).
/// </summary>
[System.Serializable]
public class Step
{
    public string Id { get; set; }           // UUID (matches DB primary key)
    public string ChallengeId { get; set; }  // UUID FK → challenges.id
    public int Number { get; set; }
    public string Description { get; set; }
    public string Subject { get; set; }
    public string Challenge { get; set; }

    // Step UUIDs that must be completed before this step unlocks
    public List<string> PrerequisiteStepIds { get; set; } = new List<string>();

    // Streak-based progression
    public int StreakGoal { get; set; } = 5;
    public int StreakCurrent { get; set; } = 0;

    // Mastery tracking
    public float MasteryTarget { get; set; } = 0.80f;
    public float MasteryCurrent { get; set; } = 0.0f;

    // Question tracking
    public int QuestionsCompleted { get; set; } = 0;

    // Status
    public StepStatus Status { get; set; } = StepStatus.NotStarted;

    // DESIGNED FOR FUTURE: Ultimate Challenge (mastery check after 5-streak)
    // Implementation: Phase 4+
    [Tooltip("If true, player must complete an ultimate challenge after 5-streak for final mastery check")]
    public bool RequireUltimateChallenge { get; set; } = false;
    public bool UltimateChallengeCompleted { get; set; } = false;

    /// <summary>
    /// Whether the 5-streak goal is reached.
    /// </summary>
    public bool IsStreakComplete => StreakCurrent >= StreakGoal;

    /// <summary>
    /// Whether the step is fully complete (both streak AND ultimate challenge if required).
    /// Use this to determine if step is done.
    /// </summary>
    public bool IsFullyComplete
    {
        get
        {
            // Must reach 5-streak first
            if (!IsStreakComplete) return false;

            // If ultimate challenge is required, it must be completed
            if (RequireUltimateChallenge && !UltimateChallengeCompleted)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Returns current phase within this step.
    /// Useful for UI to show "5-Streak (X/5)" vs "Ultimate Challenge"
    /// </summary>
    public StepPhase GetCurrentPhase()
    {
        if (!IsStreakComplete)
            return StepPhase.StreakBuilding;

        if (RequireUltimateChallenge && !UltimateChallengeCompleted)
            return StepPhase.UltimateChallenge;

        return StepPhase.Complete;
    }

    public override string ToString()
    {
        return $"Step {Number}: {Description} | Streak: {StreakCurrent}/{StreakGoal} | Phase: {GetCurrentPhase()}";
    }
}

/// <summary>
/// Status of a step (where in the progression it is).
/// </summary>
public enum StepStatus
{
    NotStarted,
    InProgress,
    Completed
}

/// <summary>
/// Current phase within a step.
/// </summary>
public enum StepPhase
{
    StreakBuilding,      // Working on 5-streak
    UltimateChallenge,   // Completed streak, now do ultimate challenge
    Complete             // All done
}