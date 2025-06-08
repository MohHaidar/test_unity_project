using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class Step
{
    public string StepId;
    public List<Activity> AllActivities;
    public Activity CurrentActivity;
    public bool StepCompleted = false;
    private Difficulty StartingDifficulty = Difficulty.Medium;

    public Step()
    {
        SetCurrentActivity(StartingDifficulty);
    }
    public void RefreshStep()
    {
        StepCompleted = false;
        SetCurrentActivity(StartingDifficulty);
    }
    public Activity GetActivity(Difficulty difficulty)
    {
        var candidates = AllActivities.Where(a => a.DifficultyLevel == (int)difficulty).ToList();
        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    public void SetCurrentActivity(Difficulty difficulty)
    {
        CurrentActivity = GetActivity(difficulty);
    }

    StepAction EvaluateStepProgress(double score, Difficulty current)
    {
        if (current == Difficulty.Hard)
        {
            if (score >= 0.7) return StepAction.CompleteStep;
            else if (score >= 0.5) return StepAction.DropToMedium;
            else return StepAction.DropToEasy;
        }
        else if (current == Difficulty.Medium)
        {
            return score >= 0.7 ? StepAction.TryHard : StepAction.DropToEasy;
        }
        else // Easy
        {
            return score >= 0.7 ? StepAction.TryMedium : StepAction.Stay;
        }
    }

    Difficulty DetermineNextStepEntry(double score)
    {
        if (score >= 0.9) return Difficulty.Hard;
        if (score >= 0.7) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    public void NextActivity(ActivityPerformance report)
    {
        // Get the report from the latest Activity scene
        StepAction result = EvaluateStepProgress(report.ConfidenceScore, DifficultyLevel);

        switch (result)
        {
            case StepAction.CompleteStep:
                StepCompleted = true;
                break;

            case StepAction.TryHard:
                SetCurrentActivity(Difficulty.Hard);
                break;

            case StepAction.TryMedium:
                SetCurrentActivity(Difficulty.Medium);
                break;

            case StepAction.DropToMedium:
                SetCurrentActivity(Difficulty.Medium);
                break;

            case StepAction.DropToEasy:
                SetCurrentActivity(Difficulty.Easy);
                break;

            case StepAction.Stay:
                // repeat same step/difficulty
                break;
        }
    }


}