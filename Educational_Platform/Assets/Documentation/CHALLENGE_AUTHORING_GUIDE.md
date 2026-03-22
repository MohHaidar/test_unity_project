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

---

## Part D — Parlour Challenges (Verbal Communication Subject)

Parlour challenges are conversation-based and use a fundamentally different generator
(`ParlourQuestionGenerator`) and question type (`ConversationQuestion`). They live in
the `Verbal Communication` subject and have slugs that start with `parlour_`.

### How Parlour Challenges Work

```
1. Player selects a parlour challenge (e.g. "The Coffee Shop")
2. QuestionFlowManager detects slug starts with "parlour_" → uses ParlourQuestionGenerator
3. Generator reads step.PromptConstraints JSON to get: character_id, skill_focus, scene
4. CharacterManager resolves the character (name, personality, speaking style)
5. A three-part prompt is sent to Ollama:
     BLOCK 1: Character personality + speaking style
     BLOCK 2: Player context (EXP tier, completed steps, mastery gaps)
     BLOCK 3: Scene, skill focus, difficulty guidance
6. Ollama returns JSON with: dialogue, question, options[4], correct, explanation
7. ConversationDisplay renders: character portrait + dialogue bubble → question + options
```

### Quick Reference for Parlour Content

| Task | Supabase only? | Code needed? |
|------|---------------|--------------|
| Add a new parlour step | ✅ Yes | ❌ No |
| Add a new character | ✅ Yes | ❌ No (portrait asset needed) |
| Add a new parlour challenge | ✅ Yes | ❌ No |
| Change character for a step | ✅ Yes (edit prompt_constraints) | ❌ No |
| Add a character portrait sprite | ❌ No | ✅ Put in Assets/Resources/Characters/ |

### UUID Allocation (Verbal Communication)

| UUID | Type | Content |
|------|------|---------|
| `a4000000-...` | Subject | Verbal Communication |
| `0c000000-...` | Character | Maya |
| `0c100000-...` | Character | Victor |
| `0c200000-...` | Character | Zoe |
| `0c300000-...` | Character | Dr. Chen |
| `0c400000-...` | Character | Alex |
| `26000000-...` | Challenge | The Coffee Shop |
| `27000000-...` | Challenge | The Job Interview |
| `28000000-...` | Challenge | The Book Club |
| `14000000-...` | Step | CS: First Impressions |
| `15000000-...` | Step | CS: Reading the Context |
| `16000000-...` | Step | CS: Tone Matching |
| `17000000-...` | Step | CS: Reading the Room |
| `18000000-...` | Step | CS: Between the Lines |
| `19000000-...` | Step | CS: Graceful Exit |
| `1a000000-...` | Step | JI: Making an Entrance |
| `1b000000-...` | Step | JI: Tell Me About Yourself |
| `1c000000-...` | Step | JI: Tricky Questions |
| `1d000000-...` | Step | JI: Staying Composed |
| `1e000000-...` | Step | JI: Reading the Offer |
| `1f000000-...` | Step | JI: Closing Statement |
| `20000000-...` | Step | BC: Opening Discussion |
| `21000000-...` | Step | BC: Word Choice Matters |
| `22000000-...` | Step | BC: What Did the Author Mean? |
| `23000000-...` | Step | BC: A Polite Disagreement |
| `24000000-...` | Step | BC: Tone and Intent |
| `25000000-...` | Step | BC: Your Turn |

**Next free parlour step UUIDs:** `29000000-...` onwards
**Next free parlour challenge UUIDs:** `29000000-...` onwards (use same range)
**Next free character UUIDs:** `0c500000-...` onwards

### Adding a New Character

#### Supabase
```sql
INSERT INTO characters (id, name, personality, speaking_style, avatar_key, subject_id) VALUES
  ('0c500000-0000-0000-0000-000000000000',
   'Sam',
   'Empathetic and reflective. Sam listens carefully before responding and often mirrors the player''s emotional register.',
   'Gentle pacing. Uses "I hear you..." or "It sounds like...". Medium-length sentences. Warm but never gushing.',
   'sam_placeholder',
   'a4000000-0000-0000-0000-000000000000');
```

#### C# (hardcoded fallback in CharacterManager.cs)
```csharp
public const string CHAR_SAM_ID = "0c500000-0000-0000-0000-000000000000";

Register(new Character(
    CHAR_SAM_ID,
    "Sam",
    "Empathetic and reflective. Listens carefully before responding.",
    "Gentle pacing. Uses \"I hear you...\". Warm but never gushing.",
    "sam_placeholder",
    VC_SUBJECT_ID));
```

#### Portrait Asset
Place sprite at: `Assets/Resources/Characters/Sam.png`
The AvatarKey in the DB must match the filename without extension (`"Sam"`).
`CharacterManager.CHAR_SAM_ID` constant maps to the DB row.

---

### Adding a New Parlour Challenge

#### SQL (Supabase migration or SQL editor)
```sql
-- 1. Challenge row
INSERT INTO challenges (id, subject_id, name, slug, description, stage_number, stage_name, difficulty) VALUES
  ('2a000000-0000-0000-0000-000000000000',
   'a4000000-0000-0000-0000-000000000000',
   'The Negotiation Room',
   'parlour_negotiation',                      -- MUST start with "parlour_"
   'Navigate difficult conversations with Alex, reading power dynamics and subtext',
   2, 'Parlour Advanced', 0.50);

-- 2. Steps (one INSERT per step, prompt_constraints is JSON)
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES
  ('2a100000-0000-0000-0000-000000000000',
   '2a000000-0000-0000-0000-000000000000',
   1, 'Opening Move',
   'Alex opens the negotiation with a loaded statement — how do you respond?',
   5, 0.80, 0.40,
   '{"character_id":"0c400000-0000-0000-0000-000000000000","skill_focus":"reading_power_dynamics","scene":"a tense negotiation room, Alex sits across the table with a neutral expression","difficulty_note":"Alex says something that sounds reasonable but is actually a test — player must read between the lines"}');

-- 3. Step chain prerequisites
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('2a200000-0000-0000-0000-000000000000', '2a100000-0000-0000-0000-000000000000');
  -- repeat for each step →

-- 4. Challenge unlock: requires Job Interview step 4 (Staying Composed)
INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id) VALUES
  ('2a000000-0000-0000-0000-000000000000', '1d000000-0000-0000-0000-000000000000');

-- 5. Character assignments
INSERT INTO character_step_assignments (character_id, step_id) VALUES
  ('0c400000-0000-0000-0000-000000000000', '2a100000-0000-0000-0000-000000000000');
```

#### C# (hardcoded fallback in ChallengeDataManager.cs — `AddParlourChallenges()`)
```csharp
public const string CHALLENGE_NEGOTIATION_ID    = "2a000000-0000-0000-0000-000000000000";
public const string STEP_NEG_OPENING_MOVE_ID    = "2a100000-0000-0000-0000-000000000000";
// ...

var negotiation = new Challenge(CHALLENGE_NEGOTIATION_ID, "The Negotiation Room", VC,
    "Navigate difficult conversations with Alex, reading power dynamics and subtext",
    "parlour_negotiation", SUBJECT_VERBAL_COMMUNICATION_ID)
    { StageNumber = 2, StageName = "Parlour Advanced", Difficulty = 0.50f };
negotiation.PrerequisiteStepIds = new List<string> { STEP_JI_STAYING_COMPOSED_ID };
// Add steps with MakeStep(...)...
Register(VC, negotiation);
```

---

### Prompt Constraints Format for Parlour Steps

```json
{
  "character_id": "<UUID from characters table>",
  "skill_focus":  "<tag — used in the AI prompt and stored on the ConversationQuestion>",
  "scene":        "<2–3 sentence description of the physical/social scene>",
  "difficulty_note": "<guidance for the AI on how to calibrate the distractor options>"
}
```

**Skill focus tags (use consistently for analytics):**
| Tag | Meaning |
|-----|---------|
| `informal_register` | Casual vs. formal language contrast |
| `formal_register` | Professional language requirements |
| `context_reading` | Implied meaning from situational context |
| `tone_matching` | Matching the conversational energy |
| `subtext` | Reading unstated meaning |
| `ambiguity_resolution` | Disambiguating a statement that could mean two things |
| `closing_register` | Ending a conversation gracefully |
| `professional_tone` | Appropriate self-presentation in a formal setting |
| `composure_under_pressure` | Maintaining register when challenged |
| `negotiation_register` | Language patterns in negotiation/offer contexts |
| `vocabulary` | Exact word meaning and synonyms |
| `vocabulary_nuance` | Connotation differences between near-synonyms |
| `interpretation` | Literal vs. figurative/thematic reading |
| `subtext_in_disagreement` | Polite disagreement with hidden tension |
| `tone_recognition` | Distinguishing critique from praise |
| `comprehension_synthesis` | Summarising overall meaning accurately |

---

### The Five Characters

| Name | UUID prefix | Best for | Tone |
|------|------------|---------|------|
| Maya | `0c000000` | Beginners, achievements, casual | Warm, casual, uses "we" |
| Victor | `0c100000` | Formal register, interviews | No contractions, structured |
| Zoe | `0c200000` | Tone matching, humour, ambiguity | Punchy, rhetorical questions |
| Dr. Chen | `0c300000` | Vocabulary, interpretation | Measured, Socratic |
| Alex | `0c400000` | Subtext, negotiation, pressure | Deadpan, indirect |

---

### Parlour Scaling Contract

Once Phase 1–4 is fully in place, adding new parlour content requires **zero code changes**:

1. ✅ Insert character row → character loads from Supabase
2. ✅ Add portrait sprite → loads from `Resources/Characters/<AvatarKey>`
3. ✅ Insert challenge row with `slug` starting `parlour_` → auto-detected by game
4. ✅ Insert steps with `prompt_constraints` JSON → AI uses them for generation
5. ✅ Insert `character_step_assignments` rows → character is assigned to step
6. ✅ Insert `challenge_step_prerequisites` rows → challenge unlock condition set
