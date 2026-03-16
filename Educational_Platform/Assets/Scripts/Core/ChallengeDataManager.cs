using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the subject/challenge/step catalog.
/// Hardcoded data (using fixed UUIDs that match the migration seed) serves as offline fallback.
/// Call LoadFromSupabaseAsync() at session start to override with live cloud data.
/// </summary>
public class ChallengeDataManager
{
    private static ChallengeDataManager _instance;
    public static ChallengeDataManager Instance
    {
        get { if (_instance == null) _instance = new ChallengeDataManager(); return _instance; }
    }

    // ─── Fixed UUIDs — must match the seeded Supabase catalog migrations ──
    public const string SUBJECT_MATH_ID    = "a1000000-0000-0000-0000-000000000000";
    public const string SUBJECT_PHYSICS_ID = "a2000000-0000-0000-0000-000000000000";
    public const string SUBJECT_HISTORY_ID = "a3000000-0000-0000-0000-000000000000";

    public const string CHALLENGE_ADDITION_ID    = "b1000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_SUBTRACTION_ID = "b2000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_MULTIPLICATION_ID = "b5000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_DIVISION_ID = "b6000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_ORDER_OF_OPERATIONS_ID = "b7000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_EXPRESSIONS_ID = "b8000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_ONE_STEP_EQUATIONS_ID = "b9000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_TWO_STEP_EQUATIONS_ID = "ba000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_SYSTEMS_OF_EQUATIONS_ID = "bb000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_FORCE_ID       = "b3000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_ROME_ID        = "b4000000-0000-0000-0000-000000000000";

    public const string STEP_ADDITION_1_ID    = "c1000000-0000-0000-0000-000000000000";
    public const string STEP_ADDITION_2_ID    = "c2000000-0000-0000-0000-000000000000";
    public const string STEP_ADDITION_3_ID    = "c3000000-0000-0000-0000-000000000000";
    public const string STEP_ADDITION_4_ID    = "c4000000-0000-0000-0000-000000000000";
    public const string STEP_SUBTRACTION_1_ID = "c5000000-0000-0000-0000-000000000000";
    public const string STEP_SUBTRACTION_2_ID = "c6000000-0000-0000-0000-000000000000";
    public const string STEP_SUBTRACTION_3_ID = "c9000000-0000-0000-0000-000000000000";
    public const string STEP_SUBTRACTION_4_ID = "ca000000-0000-0000-0000-000000000000";
    public const string STEP_MULTIPLICATION_1_ID = "cb000000-0000-0000-0000-000000000000";
    public const string STEP_MULTIPLICATION_2_ID = "cc000000-0000-0000-0000-000000000000";
    public const string STEP_MULTIPLICATION_3_ID = "cd000000-0000-0000-0000-000000000000";
    public const string STEP_MULTIPLICATION_4_ID = "ce000000-0000-0000-0000-000000000000";
    public const string STEP_DIVISION_1_ID = "cf000000-0000-0000-0000-000000000000";
    public const string STEP_DIVISION_2_ID = "d1000000-0000-0000-0000-000000000000";
    public const string STEP_DIVISION_3_ID = "d2000000-0000-0000-0000-000000000000";
    public const string STEP_DIVISION_4_ID = "d3000000-0000-0000-0000-000000000000";
    public const string STEP_ORDER_OF_OPERATIONS_1_ID = "d4000000-0000-0000-0000-000000000000";
    public const string STEP_ORDER_OF_OPERATIONS_2_ID = "d5000000-0000-0000-0000-000000000000";
    public const string STEP_ORDER_OF_OPERATIONS_3_ID = "d6000000-0000-0000-0000-000000000000";
    public const string STEP_ORDER_OF_OPERATIONS_4_ID = "d7000000-0000-0000-0000-000000000000";
    public const string STEP_EXPRESSIONS_1_ID = "d8000000-0000-0000-0000-000000000000";
    public const string STEP_EXPRESSIONS_2_ID = "d9000000-0000-0000-0000-000000000000";
    public const string STEP_EXPRESSIONS_3_ID = "da000000-0000-0000-0000-000000000000";
    public const string STEP_EXPRESSIONS_4_ID = "db000000-0000-0000-0000-000000000000";
    public const string STEP_ONE_STEP_EQUATIONS_1_ID = "dc000000-0000-0000-0000-000000000000";
    public const string STEP_ONE_STEP_EQUATIONS_2_ID = "dd000000-0000-0000-0000-000000000000";
    public const string STEP_ONE_STEP_EQUATIONS_3_ID = "de000000-0000-0000-0000-000000000000";
    public const string STEP_ONE_STEP_EQUATIONS_4_ID = "df000000-0000-0000-0000-000000000000";
    public const string STEP_TWO_STEP_EQUATIONS_1_ID = "e1000000-0000-0000-0000-000000000000";
    public const string STEP_TWO_STEP_EQUATIONS_2_ID = "e2000000-0000-0000-0000-000000000000";
    public const string STEP_TWO_STEP_EQUATIONS_3_ID = "e3000000-0000-0000-0000-000000000000";
    public const string STEP_TWO_STEP_EQUATIONS_4_ID = "e4000000-0000-0000-0000-000000000000";
    public const string STEP_SYSTEMS_OF_EQUATIONS_1_ID = "e5000000-0000-0000-0000-000000000000";
    public const string STEP_SYSTEMS_OF_EQUATIONS_2_ID = "e6000000-0000-0000-0000-000000000000";
    public const string STEP_SYSTEMS_OF_EQUATIONS_3_ID = "e7000000-0000-0000-0000-000000000000";
    public const string STEP_SYSTEMS_OF_EQUATIONS_4_ID = "e8000000-0000-0000-0000-000000000000";
    public const string STEP_SYSTEMS_OF_EQUATIONS_5_ID = "e9000000-0000-0000-0000-000000000000";
    public const string STEP_FORCE_1_ID       = "c7000000-0000-0000-0000-000000000000";
    public const string STEP_ROME_1_ID        = "c8000000-0000-0000-0000-000000000000";

    // ─── New Math challenges (Stage 2 & 3) ──────────────────────────────────
    public const string CHALLENGE_MULTIPLICATION_II_ID  = "bc000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_MULTIPLICATION_III_ID = "bd000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_DIVISION_II_ID        = "be000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_DIVISION_III_ID       = "bf000000-0000-0000-0000-000000000000";
    public const string CHALLENGE_ARITHMETIC_REVIEW_ID  = "bg000000-0000-0000-0000-000000000000";

    // Multiplication II steps: ×3, ×4, ×6, ×7
    public const string STEP_MULT_II_1_ID = "ea000000-0000-0000-0000-000000000000";
    public const string STEP_MULT_II_2_ID = "eb000000-0000-0000-0000-000000000000";
    public const string STEP_MULT_II_3_ID = "ec000000-0000-0000-0000-000000000000";
    public const string STEP_MULT_II_4_ID = "ed000000-0000-0000-0000-000000000000";
    // Multiplication III steps: ×8, ×9, mixed
    public const string STEP_MULT_III_1_ID = "ee000000-0000-0000-0000-000000000000";
    public const string STEP_MULT_III_2_ID = "ef000000-0000-0000-0000-000000000000";
    public const string STEP_MULT_III_3_ID = "f0000000-0000-0000-0000-000000000000";
    // Division II steps: ÷3, ÷4, ÷6, ÷7
    public const string STEP_DIV_II_1_ID = "f1000000-0000-0000-0000-000000000000";
    public const string STEP_DIV_II_2_ID = "f2000000-0000-0000-0000-000000000000";
    public const string STEP_DIV_II_3_ID = "f3000000-0000-0000-0000-000000000000";
    public const string STEP_DIV_II_4_ID = "f4000000-0000-0000-0000-000000000000";
    // Division III steps: ÷8, ÷9, mixed
    public const string STEP_DIV_III_1_ID = "f5000000-0000-0000-0000-000000000000";
    public const string STEP_DIV_III_2_ID = "f6000000-0000-0000-0000-000000000000";
    public const string STEP_DIV_III_3_ID = "f7000000-0000-0000-0000-000000000000";
    // Arithmetic Review steps
    public const string STEP_ARITH_REVIEW_1_ID = "f8000000-0000-0000-0000-000000000000";
    public const string STEP_ARITH_REVIEW_2_ID = "f9000000-0000-0000-0000-000000000000";
    public const string STEP_ARITH_REVIEW_3_ID = "fa000000-0000-0000-0000-000000000000";
    public const string STEP_ARITH_REVIEW_4_ID = "fb000000-0000-0000-0000-000000000000";

    // ─── Lookup dictionaries ─────────────────────────────────────────────────
    private Dictionary<string, Challenge> _challengeById   = new Dictionary<string, Challenge>();
    private Dictionary<string, Challenge> _challengeBySlug = new Dictionary<string, Challenge>();
    private Dictionary<string, Step>      _stepById        = new Dictionary<string, Step>();
    // subjectName → ordered list of challenges
    private Dictionary<string, List<Challenge>> _subjectChallenges = new Dictionary<string, List<Challenge>>();

    public ChallengeDataManager()
    {
        InitializeHardcodedChallenges();
    }

    // ─── Public query API ────────────────────────────────────────────────────

    public List<string> GetAllSubjects() => _subjectChallenges.Keys.ToList();

    public List<Challenge> GetChallengesForSubject(string subjectName)
    {
        return _subjectChallenges.TryGetValue(subjectName, out var list) ? list : new List<Challenge>();
    }

    /// <summary>Looks up a challenge by subject name + slug (or name). Backward compatible with old callers.</summary>
    public Challenge GetChallenge(string subjectName, string slugOrName)
    {
        if (_challengeBySlug.TryGetValue(slugOrName.ToLower(), out var c)) return c;
        // Fallback: search by name in that subject
        return _subjectChallenges.TryGetValue(subjectName, out var list)
            ? list.FirstOrDefault(ch => ch.Name.Equals(slugOrName, System.StringComparison.OrdinalIgnoreCase))
            : null;
    }

    public Challenge GetChallengeById(string id) =>
        _challengeById.TryGetValue(id, out var c) ? c : null;

    public Step GetStepById(string id) =>
        _stepById.TryGetValue(id, out var s) ? s : null;

    /// <summary>
    /// Returns true if all prerequisite challenges have all their steps in player.CompletedSteps.
    /// Challenges with no prerequisites are always unlocked.
    /// </summary>
    public bool IsChallengeUnlocked(string challengeId, Player player)
    {
        if (!_challengeById.TryGetValue(challengeId, out var challenge)) return false;
        if (challenge.Prerequisites == null || challenge.Prerequisites.Count == 0) return true;

        foreach (var prereqId in challenge.Prerequisites)
        {
            if (!_challengeById.TryGetValue(prereqId, out var prereq)) continue;
            foreach (var step in prereq.Steps)
            {
                if (player.CompletedSteps == null || !player.CompletedSteps.Contains(step.Id))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if the step's challenge is unlocked AND all prerequisite steps are completed.
    /// </summary>
    public bool IsStepUnlocked(string stepId, Player player)
    {
        if (!_stepById.TryGetValue(stepId, out var step)) return false;
        if (!IsChallengeUnlocked(step.ChallengeId, player)) return false;

        if (step.PrerequisiteStepIds == null || step.PrerequisiteStepIds.Count == 0) return true;

        foreach (var prereqId in step.PrerequisiteStepIds)
        {
            if (player.CompletedSteps == null || !player.CompletedSteps.Contains(prereqId))
                return false;
        }
        return true;
    }

    // ─── Supabase loader ─────────────────────────────────────────────────────

    /// <summary>
    /// Loads the full catalog from Supabase and rebuilds all lookup dictionaries.
    /// Falls back to hardcoded data on error or when Supabase is unavailable.
    /// </summary>
    public async Task LoadFromSupabaseAsync()
    {
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogWarning("[ChallengeDataManager] SupabaseClient not ready — using hardcoded catalog.");
            return;
        }

        try
        {
            string subjectJson   = await client.GetAsync("subjects",   "select=*&order=name.asc");
            string challengeJson = await client.GetAsync("challenges",  "select=*&order=created_at.asc");
            string prereqJson    = await client.GetAsync("challenge_prerequisites", "select=*");
            string stepJson      = await client.GetAsync("steps",       "select=*&order=number.asc");
            string stepPrereqJson= await client.GetAsync("step_prerequisites", "select=*");

            var subjectRows    = JsonHelper.FromJsonArray<SubjectRow>(subjectJson);
            var challengeRows  = JsonHelper.FromJsonArray<ChallengeRow>(challengeJson);
            var prereqRows     = JsonHelper.FromJsonArray<ChPrereqRow>(prereqJson);
            var stepRows       = JsonHelper.FromJsonArray<StepRow>(stepJson);
            var stepPreReqRows = JsonHelper.FromJsonArray<StepPrereqRow>(stepPrereqJson);

            if (challengeRows == null || challengeRows.Length == 0)
            {
                Debug.Log("[ChallengeDataManager] No challenges in Supabase — using hardcoded catalog.");
                return;
            }

            // Subject name map
            var subjectNames = new Dictionary<string, string>(); // id → name
            if (subjectRows != null)
                foreach (var sr in subjectRows) subjectNames[sr.id] = sr.name;

            // Challenge prerequisites: challengeId → list of required challenge IDs
            var chPrereqMap = new Dictionary<string, List<string>>();
            if (prereqRows != null)
                foreach (var pr in prereqRows)
                {
                    if (!chPrereqMap.ContainsKey(pr.challenge_id)) chPrereqMap[pr.challenge_id] = new List<string>();
                    chPrereqMap[pr.challenge_id].Add(pr.requires_challenge_id);
                }

            // Step prerequisites: stepId → list of required step IDs
            var stepPrereqMap = new Dictionary<string, List<string>>();
            if (stepPreReqRows != null)
                foreach (var spr in stepPreReqRows)
                {
                    if (!stepPrereqMap.ContainsKey(spr.step_id)) stepPrereqMap[spr.step_id] = new List<string>();
                    stepPrereqMap[spr.step_id].Add(spr.requires_step_id);
                }

            // Build new catalog
            var newChallengeById   = new Dictionary<string, Challenge>();
            var newChallengeBySlug = new Dictionary<string, Challenge>();
            var newSubjectChallenges = new Dictionary<string, List<Challenge>>();

            foreach (var cr in challengeRows)
            {
                string subjectName = subjectNames.TryGetValue(cr.subject_id, out var sn) ? sn : cr.subject_id;
                var challenge = new Challenge(cr.id, cr.name, subjectName, cr.description ?? "", cr.slug, cr.subject_id);
                challenge.Prerequisites = chPrereqMap.TryGetValue(cr.id, out var prereqs) ? prereqs : new List<string>();
                challenge.StageNumber = cr.stage_number > 0 ? cr.stage_number : 1;
                challenge.StageName = cr.stage_name ?? "";

                if (stepRows != null)
                    challenge.Steps = stepRows
                        .Where(s => s.challenge_id == cr.id)
                        .Select(s => new Step
                        {
                            Id = s.id,
                            ChallengeId = cr.id,
                            Number = s.number,
                            Description = s.title,
                            Subject = subjectName,
                            Challenge = cr.name,
                            StreakGoal = s.streak_goal > 0 ? s.streak_goal : 5,
                            MasteryTarget = s.mastery_target > 0 ? s.mastery_target : 0.80f,
                            RequireUltimateChallenge = s.require_ultimate,
                            PrerequisiteStepIds = stepPrereqMap.TryGetValue(s.id, out var sp) ? sp : new List<string>(),
                            Status = StepStatus.NotStarted
                        }).ToList();

                newChallengeById[cr.id] = challenge;
                newChallengeBySlug[cr.slug.ToLower()] = challenge;

                if (!newSubjectChallenges.ContainsKey(subjectName))
                    newSubjectChallenges[subjectName] = new List<Challenge>();
                newSubjectChallenges[subjectName].Add(challenge);
            }

            // Commit
            _challengeById     = newChallengeById;
            _challengeBySlug   = newChallengeBySlug;
            _subjectChallenges = newSubjectChallenges;

            // Rebuild step index
            _stepById = new Dictionary<string, Step>();
            foreach (var ch in _challengeById.Values)
                foreach (var step in ch.Steps)
                    _stepById[step.Id] = step;

            Debug.Log($"[ChallengeDataManager] Loaded from Supabase: {challengeRows.Length} challenges, {_stepById.Count} steps.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ChallengeDataManager] LoadFromSupabaseAsync failed — using hardcoded catalog. {e.Message}");
        }
    }

    // ─── Hardcoded fallback ───────────────────────────────────────────────────

    private void InitializeHardcodedChallenges()
    {
        // ── Stage 1: Arithmetic Foundations ─────────────────────────────────
        var addition = new Challenge(CHALLENGE_ADDITION_ID, "Addition", "Math", "Build fluency with sums from within 10 through two-digit addition", "addition", SUBJECT_MATH_ID)
            { StageNumber = 1, StageName = "Arithmetic Foundations" };
        addition.Steps = new List<Step>
        {
            MakeStep(STEP_ADDITION_1_ID, CHALLENGE_ADDITION_ID, 1, "Add Within 10", "Math", "Addition", new List<string>()),
            MakeStep(STEP_ADDITION_2_ID, CHALLENGE_ADDITION_ID, 2, "Make 10", "Math", "Addition", new List<string> { STEP_ADDITION_1_ID }),
            MakeStep(STEP_ADDITION_3_ID, CHALLENGE_ADDITION_ID, 3, "Two-Digit No Carry", "Math", "Addition", new List<string> { STEP_ADDITION_2_ID }),
            MakeStep(STEP_ADDITION_4_ID, CHALLENGE_ADDITION_ID, 4, "Two-Digit With Carry", "Math", "Addition", new List<string> { STEP_ADDITION_3_ID }),
        };

        var subtraction = new Challenge(CHALLENGE_SUBTRACTION_ID, "Subtraction", "Math", "Use subtraction to find differences, missing parts, and two-digit answers", "subtraction", SUBJECT_MATH_ID)
            { StageNumber = 1, StageName = "Arithmetic Foundations" };
        subtraction.Prerequisites = new List<string> { CHALLENGE_ADDITION_ID };
        subtraction.Steps = new List<Step>
        {
            MakeStep(STEP_SUBTRACTION_1_ID, CHALLENGE_SUBTRACTION_ID, 1, "Subtract Within 10", "Math", "Subtraction", new List<string>()),
            MakeStep(STEP_SUBTRACTION_2_ID, CHALLENGE_SUBTRACTION_ID, 2, "Find the Missing Addend", "Math", "Subtraction", new List<string> { STEP_SUBTRACTION_1_ID }),
            MakeStep(STEP_SUBTRACTION_3_ID, CHALLENGE_SUBTRACTION_ID, 3, "Two-Digit No Borrow", "Math", "Subtraction", new List<string> { STEP_SUBTRACTION_2_ID }),
            MakeStep(STEP_SUBTRACTION_4_ID, CHALLENGE_SUBTRACTION_ID, 4, "Two-Digit With Borrow", "Math", "Subtraction", new List<string> { STEP_SUBTRACTION_3_ID }),
        };

        var multiplication = new Challenge(CHALLENGE_MULTIPLICATION_ID, "Multiplication I", "Math", "Treat multiplication as repeated groups; build fluency with ×2, ×5, ×10", "multiplication", SUBJECT_MATH_ID)
            { StageNumber = 1, StageName = "Arithmetic Foundations" };
        multiplication.Prerequisites = new List<string> { CHALLENGE_SUBTRACTION_ID };
        multiplication.Steps = new List<Step>
        {
            MakeStep(STEP_MULTIPLICATION_1_ID, CHALLENGE_MULTIPLICATION_ID, 1, "Equal Groups", "Math", "Multiplication I", new List<string>()),
            MakeStep(STEP_MULTIPLICATION_2_ID, CHALLENGE_MULTIPLICATION_ID, 2, "Multiply by 2", "Math", "Multiplication I", new List<string> { STEP_MULTIPLICATION_1_ID }),
            MakeStep(STEP_MULTIPLICATION_3_ID, CHALLENGE_MULTIPLICATION_ID, 3, "Multiply by 5", "Math", "Multiplication I", new List<string> { STEP_MULTIPLICATION_2_ID }),
            MakeStep(STEP_MULTIPLICATION_4_ID, CHALLENGE_MULTIPLICATION_ID, 4, "Multiply by 10", "Math", "Multiplication I", new List<string> { STEP_MULTIPLICATION_3_ID }),
        };

        var division = new Challenge(CHALLENGE_DIVISION_ID, "Division I", "Math", "Connect division to equal sharing; build fluency with ÷2, ÷5, ÷10", "division", SUBJECT_MATH_ID)
            { StageNumber = 1, StageName = "Arithmetic Foundations" };
        division.Prerequisites = new List<string> { CHALLENGE_MULTIPLICATION_ID };
        division.Steps = new List<Step>
        {
            MakeStep(STEP_DIVISION_1_ID, CHALLENGE_DIVISION_ID, 1, "Sharing Equally", "Math", "Division I", new List<string>()),
            MakeStep(STEP_DIVISION_2_ID, CHALLENGE_DIVISION_ID, 2, "Divide by 2", "Math", "Division I", new List<string> { STEP_DIVISION_1_ID }),
            MakeStep(STEP_DIVISION_3_ID, CHALLENGE_DIVISION_ID, 3, "Divide by 5", "Math", "Division I", new List<string> { STEP_DIVISION_2_ID }),
            MakeStep(STEP_DIVISION_4_ID, CHALLENGE_DIVISION_ID, 4, "Divide by 10", "Math", "Division I", new List<string> { STEP_DIVISION_3_ID }),
        };

        // ── Stage 2: Arithmetic Mastery ──────────────────────────────────────
        var multiplicationII = new Challenge(CHALLENGE_MULTIPLICATION_II_ID, "Multiplication II", "Math", "Master the ×3, ×4, ×6, ×7 times tables", "multiplication_ii", SUBJECT_MATH_ID)
            { StageNumber = 2, StageName = "Arithmetic Mastery" };
        multiplicationII.Prerequisites = new List<string> { CHALLENGE_DIVISION_ID };
        multiplicationII.Steps = new List<Step>
        {
            MakeStep(STEP_MULT_II_1_ID, CHALLENGE_MULTIPLICATION_II_ID, 1, "Multiply by 3", "Math", "Multiplication II", new List<string>()),
            MakeStep(STEP_MULT_II_2_ID, CHALLENGE_MULTIPLICATION_II_ID, 2, "Multiply by 4", "Math", "Multiplication II", new List<string> { STEP_MULT_II_1_ID }),
            MakeStep(STEP_MULT_II_3_ID, CHALLENGE_MULTIPLICATION_II_ID, 3, "Multiply by 6", "Math", "Multiplication II", new List<string> { STEP_MULT_II_2_ID }),
            MakeStep(STEP_MULT_II_4_ID, CHALLENGE_MULTIPLICATION_II_ID, 4, "Multiply by 7", "Math", "Multiplication II", new List<string> { STEP_MULT_II_3_ID }),
        };

        var divisionII = new Challenge(CHALLENGE_DIVISION_II_ID, "Division II", "Math", "Divide by 3, 4, 6, and 7 using known times-table inverses", "division_ii", SUBJECT_MATH_ID)
            { StageNumber = 2, StageName = "Arithmetic Mastery" };
        divisionII.Prerequisites = new List<string> { CHALLENGE_MULTIPLICATION_II_ID };
        divisionII.Steps = new List<Step>
        {
            MakeStep(STEP_DIV_II_1_ID, CHALLENGE_DIVISION_II_ID, 1, "Divide by 3", "Math", "Division II", new List<string>()),
            MakeStep(STEP_DIV_II_2_ID, CHALLENGE_DIVISION_II_ID, 2, "Divide by 4", "Math", "Division II", new List<string> { STEP_DIV_II_1_ID }),
            MakeStep(STEP_DIV_II_3_ID, CHALLENGE_DIVISION_II_ID, 3, "Divide by 6", "Math", "Division II", new List<string> { STEP_DIV_II_2_ID }),
            MakeStep(STEP_DIV_II_4_ID, CHALLENGE_DIVISION_II_ID, 4, "Divide by 7", "Math", "Division II", new List<string> { STEP_DIV_II_3_ID }),
        };

        var multiplicationIII = new Challenge(CHALLENGE_MULTIPLICATION_III_ID, "Multiplication III", "Math", "Complete the times tables: master ×8, ×9, then fluently mix all facts", "multiplication_iii", SUBJECT_MATH_ID)
            { StageNumber = 2, StageName = "Arithmetic Mastery" };
        multiplicationIII.Prerequisites = new List<string> { CHALLENGE_DIVISION_II_ID };
        multiplicationIII.Steps = new List<Step>
        {
            MakeStep(STEP_MULT_III_1_ID, CHALLENGE_MULTIPLICATION_III_ID, 1, "Multiply by 8", "Math", "Multiplication III", new List<string>()),
            MakeStep(STEP_MULT_III_2_ID, CHALLENGE_MULTIPLICATION_III_ID, 2, "Multiply by 9", "Math", "Multiplication III", new List<string> { STEP_MULT_III_1_ID }),
            MakeStep(STEP_MULT_III_3_ID, CHALLENGE_MULTIPLICATION_III_ID, 3, "Mixed Times Tables (1–9)", "Math", "Multiplication III", new List<string> { STEP_MULT_III_2_ID }),
        };

        var divisionIII = new Challenge(CHALLENGE_DIVISION_III_ID, "Division III", "Math", "Divide by 8 and 9, then fluently mix all division facts", "division_iii", SUBJECT_MATH_ID)
            { StageNumber = 2, StageName = "Arithmetic Mastery" };
        divisionIII.Prerequisites = new List<string> { CHALLENGE_MULTIPLICATION_III_ID };
        divisionIII.Steps = new List<Step>
        {
            MakeStep(STEP_DIV_III_1_ID, CHALLENGE_DIVISION_III_ID, 1, "Divide by 8", "Math", "Division III", new List<string>()),
            MakeStep(STEP_DIV_III_2_ID, CHALLENGE_DIVISION_III_ID, 2, "Divide by 9", "Math", "Division III", new List<string> { STEP_DIV_III_1_ID }),
            MakeStep(STEP_DIV_III_3_ID, CHALLENGE_DIVISION_III_ID, 3, "Mixed Division Facts (1–9)", "Math", "Division III", new List<string> { STEP_DIV_III_2_ID }),
        };

        var orderOfOperations = new Challenge(CHALLENGE_ORDER_OF_OPERATIONS_ID, "Order of Operations", "Math", "Evaluate short expressions by choosing the correct operation order", "order_of_operations", SUBJECT_MATH_ID)
            { StageNumber = 2, StageName = "Arithmetic Mastery" };
        orderOfOperations.Prerequisites = new List<string> { CHALLENGE_DIVISION_III_ID };
        orderOfOperations.Steps = new List<Step>
        {
            MakeStep(STEP_ORDER_OF_OPERATIONS_1_ID, CHALLENGE_ORDER_OF_OPERATIONS_ID, 1, "Multiply Then Add", "Math", "Order of Operations", new List<string>()),
            MakeStep(STEP_ORDER_OF_OPERATIONS_2_ID, CHALLENGE_ORDER_OF_OPERATIONS_ID, 2, "Multiply Then Subtract", "Math", "Order of Operations", new List<string> { STEP_ORDER_OF_OPERATIONS_1_ID }),
            MakeStep(STEP_ORDER_OF_OPERATIONS_3_ID, CHALLENGE_ORDER_OF_OPERATIONS_ID, 3, "Parentheses First", "Math", "Order of Operations", new List<string> { STEP_ORDER_OF_OPERATIONS_2_ID }),
            MakeStep(STEP_ORDER_OF_OPERATIONS_4_ID, CHALLENGE_ORDER_OF_OPERATIONS_ID, 4, "Mixed Expressions", "Math", "Order of Operations", new List<string> { STEP_ORDER_OF_OPERATIONS_3_ID }),
        };

        // ── Stage 3: Pre-Algebra Bridge ──────────────────────────────────────
        var arithmeticReview = new Challenge(CHALLENGE_ARITHMETIC_REVIEW_ID, "Arithmetic Review", "Math", "Consolidate all four operations with mixed practice before entering algebra", "arithmetic_review", SUBJECT_MATH_ID)
            { StageNumber = 3, StageName = "Pre-Algebra Bridge" };
        arithmeticReview.Prerequisites = new List<string> { CHALLENGE_ORDER_OF_OPERATIONS_ID };
        arithmeticReview.Steps = new List<Step>
        {
            MakeStep(STEP_ARITH_REVIEW_1_ID, CHALLENGE_ARITHMETIC_REVIEW_ID, 1, "Mixed Addition and Subtraction", "Math", "Arithmetic Review", new List<string>()),
            MakeStep(STEP_ARITH_REVIEW_2_ID, CHALLENGE_ARITHMETIC_REVIEW_ID, 2, "Mixed Multiplication and Division", "Math", "Arithmetic Review", new List<string> { STEP_ARITH_REVIEW_1_ID }),
            MakeStep(STEP_ARITH_REVIEW_3_ID, CHALLENGE_ARITHMETIC_REVIEW_ID, 3, "All Four Operations", "Math", "Arithmetic Review", new List<string> { STEP_ARITH_REVIEW_2_ID }),
            MakeStep(STEP_ARITH_REVIEW_4_ID, CHALLENGE_ARITHMETIC_REVIEW_ID, 4, "Multi-Step Mental Math", "Math", "Arithmetic Review", new List<string> { STEP_ARITH_REVIEW_3_ID }),
        };

        var expressions = new Challenge(CHALLENGE_EXPRESSIONS_ID, "Expressions with Variables", "Math", "Evaluate expressions by replacing one variable with a given number", "expressions_with_variables", SUBJECT_MATH_ID)
            { StageNumber = 3, StageName = "Pre-Algebra Bridge" };
        expressions.Prerequisites = new List<string> { CHALLENGE_ARITHMETIC_REVIEW_ID };
        expressions.Steps = new List<Step>
        {
            MakeStep(STEP_EXPRESSIONS_1_ID, CHALLENGE_EXPRESSIONS_ID, 1, "Evaluate x + a", "Math", "Expressions with Variables", new List<string>()),
            MakeStep(STEP_EXPRESSIONS_2_ID, CHALLENGE_EXPRESSIONS_ID, 2, "Evaluate x - a", "Math", "Expressions with Variables", new List<string> { STEP_EXPRESSIONS_1_ID }),
            MakeStep(STEP_EXPRESSIONS_3_ID, CHALLENGE_EXPRESSIONS_ID, 3, "Evaluate ax", "Math", "Expressions with Variables", new List<string> { STEP_EXPRESSIONS_2_ID }),
            MakeStep(STEP_EXPRESSIONS_4_ID, CHALLENGE_EXPRESSIONS_ID, 4, "Evaluate x / a", "Math", "Expressions with Variables", new List<string> { STEP_EXPRESSIONS_3_ID }),
        };

        // ── Stage 4: Algebra Foundations ─────────────────────────────────────
        var oneStepEquations = new Challenge(CHALLENGE_ONE_STEP_EQUATIONS_ID, "One-Step Equations", "Math", "Solve equations with one inverse operation", "one_step_equations", SUBJECT_MATH_ID)
            { StageNumber = 4, StageName = "Algebra Foundations" };
        oneStepEquations.Prerequisites = new List<string> { CHALLENGE_EXPRESSIONS_ID };
        oneStepEquations.Steps = new List<Step>
        {
            MakeStep(STEP_ONE_STEP_EQUATIONS_1_ID, CHALLENGE_ONE_STEP_EQUATIONS_ID, 1, "Solve x + a = b", "Math", "One-Step Equations", new List<string>()),
            MakeStep(STEP_ONE_STEP_EQUATIONS_2_ID, CHALLENGE_ONE_STEP_EQUATIONS_ID, 2, "Solve x - a = b", "Math", "One-Step Equations", new List<string> { STEP_ONE_STEP_EQUATIONS_1_ID }),
            MakeStep(STEP_ONE_STEP_EQUATIONS_3_ID, CHALLENGE_ONE_STEP_EQUATIONS_ID, 3, "Solve ax = b", "Math", "One-Step Equations", new List<string> { STEP_ONE_STEP_EQUATIONS_2_ID }),
            MakeStep(STEP_ONE_STEP_EQUATIONS_4_ID, CHALLENGE_ONE_STEP_EQUATIONS_ID, 4, "Solve x / a = b", "Math", "One-Step Equations", new List<string> { STEP_ONE_STEP_EQUATIONS_3_ID }),
        };

        var twoStepEquations = new Challenge(CHALLENGE_TWO_STEP_EQUATIONS_ID, "Two-Step Equations", "Math", "Solve equations by undoing two operations in the correct order", "two_step_equations", SUBJECT_MATH_ID)
            { StageNumber = 4, StageName = "Algebra Foundations" };
        twoStepEquations.Prerequisites = new List<string> { CHALLENGE_ONE_STEP_EQUATIONS_ID };
        twoStepEquations.Steps = new List<Step>
        {
            MakeStep(STEP_TWO_STEP_EQUATIONS_1_ID, CHALLENGE_TWO_STEP_EQUATIONS_ID, 1, "Solve ax + b = c", "Math", "Two-Step Equations", new List<string>()),
            MakeStep(STEP_TWO_STEP_EQUATIONS_2_ID, CHALLENGE_TWO_STEP_EQUATIONS_ID, 2, "Solve ax - b = c", "Math", "Two-Step Equations", new List<string> { STEP_TWO_STEP_EQUATIONS_1_ID }),
            MakeStep(STEP_TWO_STEP_EQUATIONS_3_ID, CHALLENGE_TWO_STEP_EQUATIONS_ID, 3, "Solve x / a + b = c", "Math", "Two-Step Equations", new List<string> { STEP_TWO_STEP_EQUATIONS_2_ID }),
            MakeStep(STEP_TWO_STEP_EQUATIONS_4_ID, CHALLENGE_TWO_STEP_EQUATIONS_ID, 4, "Solve x / a - b = c", "Math", "Two-Step Equations", new List<string> { STEP_TWO_STEP_EQUATIONS_3_ID }),
        };

        var systemsOfEquations = new Challenge(CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, "Systems of Equations", "Math", "Use substitution and paired equations to solve for two variables", "systems_of_equations", SUBJECT_MATH_ID)
            { StageNumber = 4, StageName = "Algebra Foundations" };
        systemsOfEquations.Prerequisites = new List<string> { CHALLENGE_TWO_STEP_EQUATIONS_ID };
        systemsOfEquations.Steps = new List<Step>
        {
            MakeStep(STEP_SYSTEMS_OF_EQUATIONS_1_ID, CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, 1, "Substitute x into y = x + a", "Math", "Systems of Equations", new List<string>()),
            MakeStep(STEP_SYSTEMS_OF_EQUATIONS_2_ID, CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, 2, "Solve a System and Find x", "Math", "Systems of Equations", new List<string> { STEP_SYSTEMS_OF_EQUATIONS_1_ID }),
            MakeStep(STEP_SYSTEMS_OF_EQUATIONS_3_ID, CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, 3, "Solve a System and Find y", "Math", "Systems of Equations", new List<string> { STEP_SYSTEMS_OF_EQUATIONS_2_ID }),
            MakeStep(STEP_SYSTEMS_OF_EQUATIONS_4_ID, CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, 4, "Standard Form: Find x", "Math", "Systems of Equations", new List<string> { STEP_SYSTEMS_OF_EQUATIONS_3_ID }),
            MakeStep(STEP_SYSTEMS_OF_EQUATIONS_5_ID, CHALLENGE_SYSTEMS_OF_EQUATIONS_ID, 5, "Standard Form: Find y", "Math", "Systems of Equations", new List<string> { STEP_SYSTEMS_OF_EQUATIONS_4_ID }),
        };

        // ── Other subjects (placeholder) ──────────────────────────────────────
        var force = new Challenge(CHALLENGE_FORCE_ID, "Force and Motion", "Physics", "Newton's laws and force concepts", "force", SUBJECT_PHYSICS_ID)
            { StageNumber = 1, StageName = "Mechanics Foundations" };
        force.Prerequisites = new List<string> { CHALLENGE_ADDITION_ID };
        force.Steps = new List<Step>
        {
            MakeStep(STEP_FORCE_1_ID, CHALLENGE_FORCE_ID, 1, "Newton's First Law", "Physics", "Force and Motion", new List<string>()),
        };

        var rome = new Challenge(CHALLENGE_ROME_ID, "Ancient Rome", "History", "The Roman Republic and Empire", "ancient_rome", SUBJECT_HISTORY_ID)
            { StageNumber = 1, StageName = "Ancient World" };
        rome.Steps = new List<Step>
        {
            MakeStep(STEP_ROME_1_ID, CHALLENGE_ROME_ID, 1, "Roman Republic", "History", "Ancient Rome", new List<string>()),
        };

        // ── Registration order determines UI display order ─────────────────
        Register("Math", addition);
        Register("Math", subtraction);
        Register("Math", multiplication);
        Register("Math", division);
        Register("Math", multiplicationII);
        Register("Math", divisionII);
        Register("Math", multiplicationIII);
        Register("Math", divisionIII);
        Register("Math", orderOfOperations);
        Register("Math", arithmeticReview);
        Register("Math", expressions);
        Register("Math", oneStepEquations);
        Register("Math", twoStepEquations);
        Register("Math", systemsOfEquations);
        Register("Physics", force);
        Register("History", rome);

        Debug.Log($"[ChallengeDataManager] Hardcoded catalog: {_challengeById.Count} challenges, {_stepById.Count} steps.");
    }

    private void Register(string subjectName, Challenge c)
    {
        _challengeById[c.Id] = c;
        _challengeBySlug[c.Slug.ToLower()] = c;
        if (!_subjectChallenges.ContainsKey(subjectName)) _subjectChallenges[subjectName] = new List<Challenge>();
        _subjectChallenges[subjectName].Add(c);
        foreach (var s in c.Steps) _stepById[s.Id] = s;
    }

    private static Step MakeStep(string id, string challengeId, int number, string description, string subject, string challenge, List<string> prereqStepIds, bool requireUltimate = false)
    {
        return new Step
        {
            Id = id,
            ChallengeId = challengeId,
            Number = number,
            Description = description,
            Subject = subject,
            Challenge = challenge,
            StreakGoal = 5,
            MasteryTarget = 0.80f,
            RequireUltimateChallenge = requireUltimate,
            PrerequisiteStepIds = prereqStepIds,
            Status = StepStatus.NotStarted
        };
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    [System.Serializable] private class SubjectRow   { public string id; public string name; }
    [System.Serializable] private class ChallengeRow { public string id; public string subject_id; public string name; public string slug; public string description; public int stage_number; public string stage_name; }
    [System.Serializable] private class ChPrereqRow  { public string challenge_id; public string requires_challenge_id; }
    [System.Serializable] private class StepRow      { public string id; public string challenge_id; public int number; public string title; public string description; public int streak_goal; public float mastery_target; public bool require_ultimate; }
    [System.Serializable] private class StepPrereqRow{ public string step_id; public string requires_step_id; }
}

