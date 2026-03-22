# Challenge & Step Authoring Guide

This guide explains how to add, edit, or remove challenges and steps — either via
the Supabase dashboard alone, or with code changes when needed.

---

## Quick Reference

| Task | Supabase only? | Code needed? |
|------|---------------|--------------|
| Add a step to an existing challenge | ✅ Yes | ❌ No |
| Add a brand-new challenge | ✅ Yes | ❌ No |
| Edit a step's title / difficulty | ✅ Yes | ❌ No |
| Change what a step unlocks | ✅ Yes | ❌ No |
| Add a fallback question for a new step | ❌ No | ✅ `OllamaQuestionGenerator.cs` |
| Retire / delete a challenge | ✅ Yes | ✅ Remove from `InitializeHardcodedChallenges` |

---

## How It Works

The game has two data sources that it merges at startup:

```
1. Hardcoded C# catalog  ← always available (offline fallback)
        ↓ overridden by ↓
2. Supabase database     ← authoritative live data
```

When Supabase is reachable, its data wins completely.
When offline, the hardcoded catalog is used as-is.

### Challenge Unlock Model (AND semantics)

A challenge becomes available when **all** of its prerequisite steps are completed.
This is declared on the **challenge** itself (not on individual steps):

```
challenge_step_prerequisites
  challenge_id   → the challenge that will be unlocked
  requires_step_id → a step that must be completed (one row per required step)
```

All rows for a given `challenge_id` must be satisfied before the challenge unlocks.
The unlock message ("🔓 Unlocked: Division") appears on the step completion overlay of
the **last** step that satisfies the requirement.

Example: Division requires completing `×10` (one row). If you later add
`requires_step_id = Sharing Equally step`, Division would need BOTH to unlock.

### Prompt Constraints

The key property that makes new steps fully Supabase-driven is **`prompt_constraints`**:
if a step has this field populated in the DB, the Ollama question generator uses it
directly — no code changes needed.

---

## Part A — Supabase-Only Changes

### A1. Add a Step to an Existing Challenge

#### In Supabase SQL Editor

```sql
-- 1. Pick a UUID (follow the convention: next unused hex in the step range)
-- 2. Insert the step
INSERT INTO steps (id, challenge_id, number, title, description,
                   streak_goal, mastery_target, require_ultimate, difficulty,
                   prompt_constraints)
VALUES (
  'NEW_UUID-0000-0000-0000-000000000000',
  'CHALLENGE_UUID-0000-0000-0000-000000000000',
  6,                    -- step number (sequential within challenge)
  'My New Step',        -- short display title
  'Longer description', -- shown in tooltips / overlays
  5,                    -- streak goal (default 5)
  0.80,                 -- mastery target (default 0.80)
  false,                -- require ultimate challenge (leave false)
  0.25,                 -- global difficulty 0.0–1.0
  '- Use multiplication by 6 only\n- Other factor between 1 and 12'
    -- ↑ THIS is what the Ollama generator will use to make questions
);

-- 3. Lock it behind the previous step
INSERT INTO step_prerequisites (step_id, requires_step_id)
VALUES ('NEW_UUID-0000-0000-0000-000000000000',
        'PREV_STEP_UUID-0000-0000-0000-000000000000');

-- 4. (Optional) Unlock a challenge when ALL its prerequisite steps are completed
--    Add one row per required step to challenge_step_prerequisites
INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id)
VALUES ('TARGET_CHALLENGE_UUID-0000-0000-000000000000',
        'NEW_UUID-0000-0000-0000-000000000000');
-- If the challenge needs MULTIPLE steps, add one row per step:
-- INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id)
-- VALUES ('TARGET_CHALLENGE_UUID', 'ANOTHER_STEP_UUID');
```

#### As a migration file (recommended for reproducibility)

```
supabase migration new add_my_new_step
# Edit the generated file in supabase/migrations/
supabase db push
```

---

### A2. Add a Brand-New Challenge

```sql
-- 1. Insert the challenge
INSERT INTO challenges (id, subject_id, name, slug, description,
                        stage_number, stage_name, difficulty)
VALUES (
  'NEW_CHALLENGE_UUID-0000-0000-000000000000',
  'a1000000-0000-0000-0000-000000000000',  -- Math subject
  'Fractions',
  'fractions',         -- lowercase, underscores, unique
  'Understand parts of a whole; compare and simplify fractions',
  2,                   -- stage number
  'Arithmetic Mastery',
  0.35
);

-- 2. (Optional) Require another challenge first
INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id)
VALUES ('NEW_CHALLENGE_UUID-0000-0000-000000000000',
        'b6000000-0000-0000-0000-000000000000');  -- requires Division

-- 3. Add steps (see A1)
INSERT INTO steps (...) VALUES (...);
```

#### Writing `prompt_constraints`

`prompt_constraints` is a plain-text string passed verbatim into the Ollama prompt.
Write it as a bullet list of rules:

```
- Use fractions with denominators 2, 3, 4, 6, 8 only
- Both numerator and denominator must be whole numbers
- Ask "Which fraction is larger?" or "Simplify X/Y"
- Correct answer must be a single fraction in simplest form
```

**Tips:**
- Each rule on its own line starting with `- `
- Be specific: wrong constraints → bad questions
- Test by playing through the step once; tweak if questions are off
- For visual steps (counting, grouping), instruct the model to include
  a bracket-dot visual: `[● ● ●]  [● ● ●]`

---

### A3. Edit a Step

```sql
-- Change title and difficulty
UPDATE steps SET title = 'New Title', difficulty = 0.30
WHERE id = 'STEP_UUID-0000-0000-0000-000000000000';

-- Update prompt constraints (takes effect next game session)
UPDATE steps SET prompt_constraints = '- New rules here'
WHERE id = 'STEP_UUID-0000-0000-0000-000000000000';

-- Change what challenge a step unlocks
DELETE FROM step_unlocks WHERE step_id = 'STEP_UUID-...';
INSERT INTO step_unlocks (step_id, unlocks_challenge_id) VALUES ('STEP_UUID-...', 'NEW_CHALLENGE_UUID-...');
```

---

### A4. Retire / Remove a Challenge

```sql
-- Step 1: Move any steps you want to keep to another challenge
UPDATE steps SET challenge_id = 'KEEP_CHALLENGE_UUID'
WHERE challenge_id = 'RETIRE_CHALLENGE_UUID';

-- Step 2: Remove it from prerequisite chains
DELETE FROM challenge_prerequisites
WHERE challenge_id = 'RETIRE_CHALLENGE_UUID'
   OR requires_challenge_id = 'RETIRE_CHALLENGE_UUID';

-- Step 3: Remove step unlocks pointing to it
DELETE FROM step_unlocks WHERE unlocks_challenge_id = 'RETIRE_CHALLENGE_UUID';

-- Step 4: Delete the challenge (remaining steps cascade-delete)
DELETE FROM challenges WHERE id = 'RETIRE_CHALLENGE_UUID';
```

> **Never hard-delete a challenge that has player progress data in `player_completed_steps`.**
> Query first: `SELECT COUNT(*) FROM player_completed_steps WHERE step_id IN (SELECT id FROM steps WHERE challenge_id = 'X');`
> If > 0, leave it in the DB but stop showing it (omit from `InitializeHardcodedChallenges`).

---

## Part B — Code Changes (when needed)

Code changes are only needed when:
- Adding **fallback questions** for new steps (used when Ollama fails)
- Adding a new **challenge** to the hardcoded offline catalog
- Changing **step order constants** referenced elsewhere in code

### B1. UUID Constants (`ChallengeDataManager.cs`)

Add new UUIDs at the top of the class following the existing pattern:

```csharp
// UUID convention:
//   Challenges:  bX000000-0000-0000-0000-000000000000
//   Steps:       use next available hex block (check existing constants)

public const string CHALLENGE_FRACTIONS_ID    = "b3000000-0000-0000-0000-000000000000";
public const string STEP_FRACTIONS_HALVES_ID  = "14000000-0000-0000-0000-000000000000";
public const string STEP_FRACTIONS_THIRDS_ID  = "15000000-0000-0000-0000-000000000000";
```

> Never reuse a UUID that was ever assigned to anything — even retired items.

---

### B2. Hardcoded Catalog (`InitializeHardcodedChallenges()`)

Only needed for **offline fallback** (Supabase is always authoritative).
Add the challenge and its steps following the existing pattern:

```csharp
var fractions = new Challenge(CHALLENGE_FRACTIONS_ID, "Fractions", "Math",
    "Understand parts of a whole", "fractions", SUBJECT_MATH_ID)
    { StageNumber = 2, StageName = "Arithmetic Mastery", Difficulty = 0.35f };
fractions.Prerequisites = new List<string> { CHALLENGE_DIVISION_ID };
fractions.Steps = new List<Step>
{
    MakeStep(STEP_FRACTIONS_HALVES_ID, CHALLENGE_FRACTIONS_ID, 1, "Halves",
        "Math", "Fractions", new List<string>(), 0.30f),
    MakeStep(STEP_FRACTIONS_THIRDS_ID, CHALLENGE_FRACTIONS_ID, 2, "Thirds",
        "Math", "Fractions", new List<string> { STEP_FRACTIONS_HALVES_ID }, 0.32f),
};
// Register at the end of the method:
Register("Math", fractions);
```

> You do NOT need to fill in `prompt_constraints` here — the Supabase data overrides it.
> Leave them empty in the hardcoded catalog; they're only needed in the DB.

---

### B3. Fallback Questions (`OllamaQuestionGenerator.cs` → `GetFallbackQuestion()`)

Fallback questions are used when Ollama fails after all retry attempts.
Add a case for your new step:

```csharp
"fractions" => step.Number switch
{
    1 => MakeFallbackMC(
            "Which picture shows 1/2?",
            "One part out of two equal parts",
            new[] { "One part out of two equal parts", "Two parts out of three", "One part out of four", "Two halves" },
            "Halves"),
    2 => MakeFallbackMC("What is 1/3 of 9?", "3",
            Shuffle(3, Distractors(3, 3)), "Thirds"),
    _ => MakeFallbackMC("What is 1/2 of 10?", "5",
            Shuffle(5, Distractors(5, 3)), "Fractions"),
},
```

For numeric steps, use the helpers:
```csharp
FallbackMultBy(7, variant)     // "What is X × 7?"
FallbackDivBy(5, variant)      // "What is Y ÷ 5?"
MakeGroupsVisual(3, 4)         // "[● ● ● ●]  [● ● ● ●]  [● ● ● ●]"
```

---

## Part C — Full Checklist

### Adding a step to an existing challenge

- [ ] Choose UUID (next available; never reuse)
- [ ] `INSERT INTO steps` with all fields including `prompt_constraints`
- [ ] `INSERT INTO step_prerequisites` (what must be done before)
- [ ] (Optional) `INSERT INTO step_unlocks` if it gates another challenge
- [ ] (Optional) Add fallback question in `GetFallbackQuestion()` in code
- [ ] Run `supabase db push` to apply

### Adding a new challenge

- [ ] Choose challenge UUID
- [ ] Choose step UUIDs
- [ ] `INSERT INTO challenges`
- [ ] (Optional) `INSERT INTO challenge_prerequisites`
- [ ] `INSERT INTO steps` (with `prompt_constraints`)
- [ ] `INSERT INTO step_prerequisites`
- [ ] Add UUID constants to `ChallengeDataManager.cs`
- [ ] Add hardcoded entry in `InitializeHardcodedChallenges()`
- [ ] Add fallback questions in `GetFallbackQuestion()`
- [ ] Run `supabase db push`

### Editing a step

- [ ] `UPDATE steps SET ... WHERE id = '...'`
- [ ] Adjust `step_prerequisites` / `step_unlocks` if needed
- [ ] If step number changed: also update hardcoded catalog in code

### Retiring a challenge

- [ ] Move/reassign steps if any should survive
- [ ] Clean up `challenge_prerequisites` and `step_unlocks`
- [ ] `DELETE FROM challenges WHERE id = '...'`
- [ ] Remove from `InitializeHardcodedChallenges()` in code
- [ ] Remove its UUID constant (or mark with comment `// retired`)

---

## UUID Reference Sheet

### Subjects
| ID | Name |
|----|------|
| `a1000000-...` | Math |

### Math Challenges
| ID | Slug | Name |
|----|------|------|
| `b1000000-...` | `addition` | Addition |
| `b2000000-...` | `subtraction` | Subtraction |
| `b5000000-...` | `multiplication` | Multiplication |
| `b6000000-...` | `division` | Division |
| `b7000000-...` | `order_of_operations` | Order of Operations |
| `b0000000-...` | `arithmetic_review` | Arithmetic Review |
| `b8000000-...` | `expressions_with_variables` | Expressions with Variables |
| `b9000000-...` | `one_step_equations` | One-Step Equations |
| `ba000000-...` | `two_step_equations` | Two-Step Equations |
| `bb000000-...` | `systems_of_equations` | Systems of Equations |

### Next available UUIDs
- **Challenge:** `b3000000-...`, `b4000000-...`  *(b0, b1, b2, b5–bb are taken)*
- **Step:** `14000000-...` onwards  *(10–13 = Equal Groups, all cX–fX = taken)*

---

## Prompt Constraints Format Reference

```
- [rule 1]
- [rule 2]
- [rule 3]
```

Each line is a bullet rule. The generator appends these to the Ollama system prompt.
Keep rules short and unambiguous.

**Common patterns:**

| Goal | Rule text |
|------|-----------|
| Lock to one operation | `- Use multiplication by 7 only` |
| Constrain number range | `- Other factor between 1 and 12` |
| Require exact division | `- Quotient must be a whole number` |
| Add visual | `- INCLUDE a visual using bracket-dot format: "[● ●]  [● ●]"` |
| Control question phrasing | `- Ask "How many items are there in total?"` |
| Restrict answer type | `- Correct answer must be a single fraction in simplest form` |
