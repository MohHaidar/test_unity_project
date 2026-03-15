using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages loading and caching of challenge definitions.
/// Hardcoded challenges serve as default/fallback data.
/// Call LoadFromSupabaseAsync() at session start to override with cloud data if available.
/// </summary>
public class ChallengeDataManager
{
    private static ChallengeDataManager _instance;
    private Dictionary<string, Dictionary<string, Challenge>> _challengeCache = new Dictionary<string, Dictionary<string, Challenge>>();

    public static ChallengeDataManager Instance
    {
        get { if (_instance == null) _instance = new ChallengeDataManager(); return _instance; }
    }

    public ChallengeDataManager()
    {
        InitializeHardcodedChallenges();
    }

    /// <summary>
    /// Loads challenges and steps from Supabase, overriding the hardcoded cache.
    /// Falls back silently to hardcoded data if Supabase is unavailable or tables are empty.
    /// </summary>
    public async Task LoadFromSupabaseAsync()
    {
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogWarning("[ChallengeDataManager] SupabaseClient not ready — using hardcoded challenges.");
            return;
        }

        try
        {
            string challengeJson = await client.GetAsync("challenges", "select=*");
            var challengeRows = JsonHelper.FromJsonArray<ChallengeRow>(challengeJson);
            if (challengeRows == null || challengeRows.Length == 0)
            {
                Debug.Log("[ChallengeDataManager] No challenges in Supabase — using hardcoded data.");
                return;
            }

            string stepJson = await client.GetAsync("steps", "select=*&order=number.asc");
            var stepRows = JsonHelper.FromJsonArray<StepRow>(stepJson);

            var newCache = new Dictionary<string, Dictionary<string, Challenge>>();
            foreach (var cr in challengeRows)
            {
                if (!newCache.ContainsKey(cr.subject))
                    newCache[cr.subject] = new Dictionary<string, Challenge>();

                var challenge = new Challenge(cr.id, cr.name, cr.subject, cr.description ?? "");
                if (stepRows != null)
                    challenge.Steps = stepRows
                        .Where(s => s.challenge_id == cr.id)
                        .Select(s => new Step
                        {
                            Id = s.id,
                            Number = s.number,
                            Description = s.description ?? "",
                            Subject = s.subject,
                            Challenge = cr.name,
                            StreakGoal = s.streak_goal,
                            MasteryTarget = s.mastery_target,
                            RequireUltimateChallenge = s.require_ultimate,
                            Status = StepStatus.NotStarted
                        }).ToList();

                newCache[cr.subject][cr.id] = challenge;
            }

            _challengeCache = newCache;
            Debug.Log($"[ChallengeDataManager] Loaded {challengeRows.Length} challenges from Supabase.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ChallengeDataManager] LoadFromSupabaseAsync failed — using hardcoded data. {e.Message}");
        }
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
    /// Initializes hardcoded challenges (default/fallback data).
    /// TODO: Seed these into Supabase once and rely on LoadFromSupabaseAsync() going forward.
    /// </summary>
    private void InitializeHardcodedChallenges()
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

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    [System.Serializable] private class ChallengeRow
    {
        public string id;
        public string name;
        public string description;
        public string subject;
    }

    [System.Serializable] private class StepRow
    {
        public string id;
        public int number;
        public string description;
        public string subject;
        public string challenge_id;
        public int streak_goal;
        public float mastery_target;
        public bool require_ultimate;
    }
}
