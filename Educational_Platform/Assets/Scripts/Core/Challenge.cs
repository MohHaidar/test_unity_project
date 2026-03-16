using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a challenge (chapter) within a subject.
/// Contains multiple steps that the player must progress through.
/// </summary>
[System.Serializable]
public class Challenge
{
    public string Id { get; set; }          // UUID (matches DB primary key)
    public string SubjectId { get; set; }   // UUID FK → subjects.id
    public string Slug { get; set; }        // URL-safe name used for code lookups, e.g. "addition"
    public string Name { get; set; }
    public string Description { get; set; }
    public string Subject { get; set; }     // Runtime subject name (populated from join, not stored on challenge)

    public List<Step> Steps { get; set; } = new List<Step>();
    // Challenge UUIDs that must be completed before this challenge is available
    public List<string> Prerequisites { get; set; } = new List<string>();

    // Stage grouping — stages are visual milestones (e.g. Stage 1: Arithmetic Foundations)
    public int StageNumber { get; set; } = 1;
    public string StageName { get; set; } = "";

    public int TotalSteps => Steps.Count;
    public int CompletedSteps => Steps.Count(s => s.Status == StepStatus.Completed);
    public bool IsCompleted => CompletedSteps == TotalSteps;

    public Challenge() { }

    public Challenge(string id, string name, string subject, string description = "", string slug = "", string subjectId = "")
    {
        Id = id;
        Name = name;
        Subject = subject;
        Description = description;
        Slug = string.IsNullOrEmpty(slug) ? name.ToLower().Replace(" ", "_") : slug;
        SubjectId = subjectId;
    }

    /// <summary>
    /// Gets a step by number (1-indexed).
    /// </summary>
    public Step GetStep(int stepNumber)
    {
        return Steps.FirstOrDefault(s => s.Number == stepNumber);
    }

    /// <summary>
    /// Gets the first incomplete step.
    /// </summary>
    public Step GetFirstIncompleteStep()
    {
        return Steps.FirstOrDefault(s => s.Status != StepStatus.Completed);
    }

    /// <summary>
    /// Gets the next step after the given step number.
    /// </summary>
    public Step GetNextStep(int currentStepNumber)
    {
        var nextStep = Steps.FirstOrDefault(s => s.Number == currentStepNumber + 1);
        return nextStep;
    }

    public override string ToString()
    {
        return $"{Name} ({CompletedSteps}/{TotalSteps} steps complete)";
    }
}
