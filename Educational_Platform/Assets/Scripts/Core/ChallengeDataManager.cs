using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages loading and caching of challenge definitions.
/// Currently hardcoded; can be updated to load from JSON later.
/// </summary>
public class ChallengeDataManager
{
    private static ChallengeDataManager _instance;
    private Dictionary<string, Dictionary<string, Challenge>> _challengeCache = new Dictionary<string, Dictionary<string, Challenge>>();

    public static ChallengeDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ChallengeDataManager();
            }
            return _instance;
        }
    }

    public ChallengeDataManager()
    {
        InitializeChallenges();
    }

    /// <summary>
    /// Gets a challenge by subject and challenge ID.
    /// </summary>
    public Challenge GetChallenge(string subject, string challengeId)
    {
        if (_challengeCache.ContainsKey(subject) && _challengeCache[subject].ContainsKey(challengeId))
        {
            return _challengeCache[subject][challengeId];
        }

        Debug.LogWarning($"[ChallengeDataManager] Challenge not found: {subject}/{challengeId}");
        return null;
    }

    /// <summary>
    /// Gets all challenges for a subject.
    /// </summary>
    public List<Challenge> GetChallengesForSubject(string subject)
    {
        if (_challengeCache.ContainsKey(subject))
        {
            return _challengeCache[subject].Values.ToList();
        }

        Debug.LogWarning($"[ChallengeDataManager] Subject not found: {subject}");
        return new List<Challenge>();
    }

    /// <summary>
    /// Gets all available subjects.
    /// </summary>
    public List<string> GetAllSubjects()
    {
        return _challengeCache.Keys.ToList();
    }

    /// <summary>
    /// Initializes hardcoded challenges.
    /// TODO: Replace with JSON loading when ready.
    /// </summary>
    private void InitializeChallenges()
    {
        // Math Subject
        AddMathChallenges();

        // Physics Subject (placeholder)
        AddPhysicsChallenges();

        // History Subject (placeholder)
        AddHistoryChallenges();

        Debug.Log($"[ChallengeDataManager] Loaded {_challengeCache.Count} subjects");
    }

    private void AddMathChallenges()
    {
        var mathChallenges = new Dictionary<string, Challenge>();

        // Addition Challenge
        var additionChallenge = new Challenge("addition", "Addition", "Math", "Learn addition from basics to 2-digit numbers");
        additionChallenge.Steps = new List<Step>
        {
            new Step
            {
                Id = "math-addition-step1",
                Number = 1,
                Description = "Single Digit Addition (0-5)",
                Subject = "Math",
                Challenge = "Addition",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            },
            new Step
            {
                Id = "math-addition-step2",
                Number = 2,
                Description = "Single Digit Addition (6-9)",
                Subject = "Math",
                Challenge = "Addition",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            },
            new Step
            {
                Id = "math-addition-step3",
                Number = 3,
                Description = "Two Digit Addition (No Carrying)",
                Subject = "Math",
                Challenge = "Addition",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            },
            new Step
            {
                Id = "math-addition-step4",
                Number = 4,
                Description = "Two Digit Addition (With Carrying)",
                Subject = "Math",
                Challenge = "Addition",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted,
                RequireUltimateChallenge = true  // Ultimate challenge example
            }
        };
        mathChallenges["addition"] = additionChallenge;

        // Subtraction Challenge
        var subtractionChallenge = new Challenge("subtraction", "Subtraction", "Math", "Learn subtraction");
        subtractionChallenge.Steps = new List<Step>
        {
            new Step
            {
                Id = "math-subtraction-step1",
                Number = 1,
                Description = "Single Digit Subtraction",
                Subject = "Math",
                Challenge = "Subtraction",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            },
            new Step
            {
                Id = "math-subtraction-step2",
                Number = 2,
                Description = "Two Digit Subtraction",
                Subject = "Math",
                Challenge = "Subtraction",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            }
        };
        mathChallenges["subtraction"] = subtractionChallenge;

        _challengeCache["Math"] = mathChallenges;
    }

    private void AddPhysicsChallenges()
    {
        var physicsChallenges = new Dictionary<string, Challenge>();

        // Force Challenge (placeholder)
        var forceChallenge = new Challenge("force", "Force and Motion", "Physics", "Understand forces");
        forceChallenge.Steps = new List<Step>
        {
            new Step
            {
                Id = "physics-force-step1",
                Number = 1,
                Description = "Newton's Laws",
                Subject = "Physics",
                Challenge = "Force",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            }
        };
        physicsChallenges["force"] = forceChallenge;

        _challengeCache["Physics"] = physicsChallenges;
    }

    private void AddHistoryChallenges()
    {
        var historyChallenges = new Dictionary<string, Challenge>();

        // Ancient Rome Challenge (placeholder)
        var romeChallenge = new Challenge("ancient_rome", "Ancient Rome", "History", "Learn about Ancient Rome");
        romeChallenge.Steps = new List<Step>
        {
            new Step
            {
                Id = "history-rome-step1",
                Number = 1,
                Description = "Roman Republic",
                Subject = "History",
                Challenge = "Ancient Rome",
                StreakGoal = 5,
                MasteryTarget = 0.80f,
                Status = StepStatus.NotStarted
            }
        };
        historyChallenges["ancient_rome"] = romeChallenge;

        _challengeCache["History"] = historyChallenges;
    }
}
