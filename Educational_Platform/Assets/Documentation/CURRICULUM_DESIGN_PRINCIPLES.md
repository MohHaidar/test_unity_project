# Educational Platform — Curriculum Design Principles

This document captures the agreed design rules for building and extending the curriculum.
Every new session should read this before adding or modifying challenges.

---

## 1. Single Challenge Per Concept

**Rule:** Every concept (multiplication, division, fractions, …) lives in **exactly one Challenge**.
No "Multiplication I / II / III" pattern.

- Keeps the challenge selector clean.
- Progress is visible in one place.
- Step numbers inside a challenge form a natural learning arc.

> **When adding a new concept:** create one challenge, sequence all steps inside it,
> and use step-level prerequisites to gate the harder steps.

---

## 2. Step-Based Challenge Unlocking

Challenges are unlocked by completing a specific **step**, not by completing another challenge.

```
Step.UnlocksChallengeIds   → list of challenge IDs unlocked when this step is finished
ChallengeDataManager._challengeUnlockedBySteps  → reverse index built at startup
```

### How to wire an unlock

```csharp
// In ChallengeDataManager.InitializeHardcodedChallenges():
MakeStep(STEP_MULT_BY_10_ID, ..., unlocksChallengeIds: new List<string> { CHALLENGE_DIVISION_ID })
```

The Step Complete overlay automatically announces: **"🔓 Unlocked: Division"**

### Rule of thumb

| Step position in challenge | Typical unlock target |
|----------------------------|-----------------------|
| "gateway" step (first hard concept) | the next related challenge |
| Final/mixed step | advanced topic or stage gate |

---

## 3. Cross-Challenge Step Prerequisites

Division steps may require *multiplication* steps as prerequisites:

```
÷3  requires:  ÷4 (prior division step)  +  ×3 (matching mult step)
```

This is expressed with `Step.PrerequisiteStepIds` — a list of step UUIDs regardless of which challenge they belong to.
`IsStepUnlocked()` in `ChallengeDataManager` handles cross-challenge lookups automatically.

---

## 4. Visual Representation for Concrete-Manipulative Steps

Steps that introduce a new concept via **counting or grouping** (Equal Groups, Arrays, Fractions
of a Set, …) must embed a **visual representation** directly in the question text.

### Format

```
[● ● ●]  [● ● ●]  [● ● ●]
```

- Use `●` (U+25CF BLACK CIRCLE) for items.
- Wrap each group in `[` … `]`.
- Separate groups with two spaces.
- The Ollama prompt **must instruct** the model to include this visual (see `GetStepConstraints()`).
- The fallback question generator (`GetFallbackQuestion`) must call `MakeGroupsVisual(numGroups, items)`.

### Helper in OllamaQuestionGenerator

```csharp
private static string MakeGroupsVisual(int numGroups, int itemsPerGroup)
{
    string group = "[" + string.Join(" ", Enumerable.Repeat("●", itemsPerGroup)) + "]";
    return string.Join("  ", Enumerable.Repeat(group, numGroups));
}
```

---

## 5. Step Difficulty Ordering Within a Challenge

Use this ordering as the default within any arithmetic challenge (adapt as needed):

| Arithmetic | Recommended order |
|------------|-------------------|
| Multiplication | Equal Groups (×1) → ×10 → ×2 → ×5 → ×4 → ×3 → ×8 → ×6 → ×7 → ×9 → Mixed |
| Division | Sharing Equally → ÷10 → ÷2 → ÷5 → ÷4 → ÷3 → ÷8 → ÷6 → ÷7 → ÷9 → Mixed |

**Rationale:** ×10 and ×2 are the easiest patterns; ×7 and ×9 are the hardest.
Division mirrors multiplication to maximise transfer.

---

## 6. Question Mode Assignments

| Step type | Primary question mode |
|-----------|-----------------------|
| Concept introduction (Equal Groups, Sharing Equally) | Multiple Choice only |
| Single-factor fluency (×3, ÷5, …) | Multiple Choice, then Fill-in-the-blank |
| Mixed / consolidation | Fill-in-the-blank preferred |

> When fill-in-the-blank is introduced for a step, update `GetStepConstraints()` to include
> `"question_type": "fill_in_blank"` in the prompt constraints.

---

## 7. Fallback Question Standard

Every step must have a **procedural fallback question** (no Ollama required) in
`OllamaQuestionGenerator.GetFallbackQuestion(Step)`.

- Dispatch on `step.Challenge` slug + `step.Number`.
- Cycle through ≥ 3 variants using `step.QuestionsCompleted % 3`.
- Concrete-manipulative steps must use `MakeGroupsVisual`.
- Fluency steps call `FallbackMultBy(n, variant)` or `FallbackDivBy(n, variant)`.

---

## 8. UUID Assignment Convention

| Type | Format |
|------|--------|
| Challenge | `bX000000-0000-0000-0000-000000000000` |
| Step | `cX000000–fX000000-0000-0000-0000-000000000000` |
| New steps (overflow) | `10000000-0000-0000-0000-000000000000`, `11000000-…`, etc. |

Always keep UUIDs stable — they are stored in Supabase and in player progress records.
Never reuse a UUID that was previously assigned to a different concept.

---

## 9. Supabase Migration Rules

- Every curriculum change that affects challenges or steps **must** have a corresponding migration file.
- File naming: `YYYYMMDDHHMMSS_short_description.sql`
- Migrations must be idempotent (`ON CONFLICT DO NOTHING`, `UPDATE … WHERE id = '…'`).
- To retire a challenge: `UPDATE challenges SET active = false WHERE id = '…'` — never hard-delete.

---

## 10. Debug Mode

`QuestionFlowManager` has a **Debug Mode** panel (hidden in production):

| Button | Effect |
|--------|--------|
| Win Question | Injects the correct answer for the current question |
| Win Step | Sets streak to goal, advances the step |
| Win Challenge | Marks ALL steps in the current challenge complete |

Enable via the `debugMode` toggle in the Inspector.
The debug log for question generation is available at `OllamaQuestionGenerator.LastGenerationDebugLog`.
