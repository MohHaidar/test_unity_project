# 📚 Educational Platform — Curriculum Reference

This document is the authoritative planning reference for all subjects, challenges, and steps.
Use it when designing new challenges, auditing prerequisites, or ensuring the hardcoded catalog in
`ChallengeDataManager.cs` and the Supabase seed migrations stay aligned.

---

## How the Content Model Works

| Concept | Code Class | Description |
|---------|-----------|-------------|
| **Subject** | string key | Top-level grouping (Math, Physics, History). |
| **Challenge** | `Challenge` | A self-contained chapter or milestone. Has a list of steps and an optional list of challenge prerequisites that must be fully completed before this challenge unlocks. |
| **Step** | `Step` | A single focused concept inside a challenge. Mastered by reaching a 5-question streak. Each step may require earlier steps within the same (or another) challenge to be completed first. |

A step is considered **complete** when `Step.IsFullyComplete` returns true — streak reached, and
ultimate challenge done if `RequireUltimateChallenge = true` (UI not yet implemented; avoid using
this flag until the UI is built).

---

## Prerequisite Notation

```
Challenge: [prerequisite challenge IDs in brackets]
  Step N: description  [prerequisite step IDs / labels]
```

- A **challenge** with no bracket entry has no prerequisite and is unlocked by default.
- A **challenge** locked behind others requires all steps of those challenges to be in `CompletedSteps`.
- A **step** with no bracket entry is the first (entry) step of its challenge — unlocked as soon as its challenge is.
- A **step** locked behind another step requires that step to be in `CompletedSteps`.

---

## 🔢 Math

### Journey Overview

The full Math subject journey runs from basic arithmetic to Calculus III (complex numbers and multivariable calculus). The curriculum is divided into **stages** — each stage is a named milestone that groups related challenges. Stages are always visible to the player (challenges inside are locked until prerequisites are met), giving a clear view of the road ahead without making progress feel trivial.

| Stage | Name | Challenges | Status |
|-------|------|------------|--------|
| 1 | **Arithmetic Foundations** | Addition, Subtraction, Multiplication I, Division I | ✅ implemented |
| 2 | **Arithmetic Mastery** | Multiplication II, Division II, Multiplication III, Division III, Order of Operations | ✅ implemented |
| 3 | **Pre-Algebra Bridge** | Arithmetic Review, Expressions with Variables | ✅ implemented |
| 4 | **Algebra Foundations** | One-Step Equations, Two-Step Equations, Systems of Equations | ✅ implemented |
| 5 | **Algebra Mastery** | Inequalities, Linear Functions, Quadratic Equations | 📋 planned |
| 6 | **Advanced Algebra** | Polynomials, Rational Expressions, Radical Equations | 📋 planned |
| 7 | **Pre-Calculus** | Trigonometry Basics, Functions & Transformations, Conic Sections | 📋 planned |
| 8 | **Calculus I** | Limits, Derivatives, Applications of Derivatives | 📋 planned |
| 9 | **Calculus II** | Integrals, Techniques of Integration, Series | 📋 planned |
| 10 | **Calculus III** | Multivariable Calculus, Vectors, Complex Numbers | 📋 planned |

**Stage design intent:** A player who reaches Systems of Equations has completed Stage 4 of 10. They see "Stage 4 · Algebra Foundations · 3/3 challenges complete" — a concrete milestone that feels earned, with seven more stages visible on the horizon.

**Full prerequisite chain (current):**  
Addition → Subtraction → Multiplication I → Division I → Multiplication II → Division II → Multiplication III → Division III → Order of Operations → Arithmetic Review → Expressions with Variables → One-Step Equations → Two-Step Equations → Systems of Equations

---

### Stage 1 · Arithmetic Foundations

#### Challenge 1 — Addition
> **ID:** `b1000000-0000-0000-0000-000000000000` · **Slug:** `addition`
> **Prereqs:** _(none — entry challenge)_
> **Goal:** Build fluency with sums from within 10 through two-digit addition.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Add Within 10 | `c1000000-…` | _(none)_ |
| 2 | Make 10 | `c2000000-…` | Step 1 |
| 3 | Two-Digit No Carry | `c3000000-…` | Step 2 |
| 4 | Two-Digit With Carry | `c4000000-…` | Step 3 |

**Concept ladder:** number bonds within 10 → the anchoring "make 10" strategy → column addition without regrouping → column addition with regrouping (carry).

---

#### Challenge 2 — Subtraction
> **ID:** `b2000000-0000-0000-0000-000000000000` · **Slug:** `subtraction`
> **Prereqs:** Addition
> **Goal:** Use subtraction to find differences, missing parts, and two-digit answers.

| # | Step | ID | Prereq Step | Difficulty |
|---|------|----|-------------|------------|
| 1 | Subtract Within 10 | `c5000000-…` | _(none)_ | 0.08 |
| 2 | Find the Missing Addend | `c6000000-…` | Step 1 | 0.09 |
| 3 | Subtract Within 20 | `fc000000-…` | Step 2 | 0.10 |
| 4 | Subtract from Tens | `fd000000-…` | Step 3 | 0.11 |
| 5 | Two-Digit No Borrow | `c9000000-…` | Step 4 | 0.12 |
| 6 | Two-Digit With Borrow | `ca000000-…` | Step 5 | 0.13 |

**Concept ladder:** removal / difference within 10 → subtraction as missing-addend (3 + ? = 8) → cross-ten subtraction within 20 (bridge-through-10) → anchor subtraction from multiples of 10 → place-value column subtraction without regrouping → column subtraction with regrouping (borrow).

---

#### Challenge 3 — Multiplication I
> **ID:** `b5000000-0000-0000-0000-000000000000` · **Slug:** `multiplication` (also `multiplication_i`)
> **Prereqs:** Subtraction
> **Goal:** Treat multiplication as repeated groups; build fluency with ×2, ×5, ×10.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Equal Groups | `cb000000-…` | _(none)_ |
| 2 | Multiply by 2 | `cc000000-…` | Step 1 |
| 3 | Multiply by 5 | `cd000000-…` | Step 2 |
| 4 | Multiply by 10 | `ce000000-…` | Step 3 |

**Concept ladder:** conceptual grounding (groups of) → doubling facts → skip-counting by 5s → place-value connection (×10 shifts digits left).

---

#### Challenge 4 — Division I
> **ID:** `b6000000-0000-0000-0000-000000000000` · **Slug:** `division` (also `division_i`)
> **Prereqs:** Multiplication I
> **Goal:** Connect division to equal sharing; build fluency with ÷2, ÷5, ÷10.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Sharing Equally | `cf000000-…` | _(none)_ |
| 2 | Divide by 2 | `d1000000-…` | Step 1 |
| 3 | Divide by 5 | `d2000000-…` | Step 2 |
| 4 | Divide by 10 | `d3000000-…` | Step 3 |

**Concept ladder:** partitive division concept → halving as inverse of doubling → ÷5 as inverse of ×5 skip-counting → ÷10 as place-value shift.

---

### Stage 2 · Arithmetic Mastery

#### Challenge 5 — Multiplication II
> **ID:** `bc000000-0000-0000-0000-000000000000` · **Slug:** `multiplication_ii`
> **Prereqs:** Division I
> **Goal:** Master the ×3, ×4, ×6, ×7 times tables.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Multiply by 3 | `ea000000-…` | _(none)_ |
| 2 | Multiply by 4 | `eb000000-…` | Step 1 |
| 3 | Multiply by 6 | `ec000000-…` | Step 2 |
| 4 | Multiply by 7 | `ed000000-…` | Step 3 |

**Concept ladder:** ×3 (triple / add the number twice more) → ×4 (double-double) → ×6 (5× + 1×) → ×7 (hardest isolated table, needs drill).

---

#### Challenge 6 — Division II
> **ID:** `be000000-0000-0000-0000-000000000000` · **Slug:** `division_ii`
> **Prereqs:** Multiplication II
> **Goal:** Divide by 3, 4, 6, and 7 using known times-table inverses.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Divide by 3 | `f1000000-…` | _(none)_ |
| 2 | Divide by 4 | `f2000000-…` | Step 1 |
| 3 | Divide by 6 | `f3000000-…` | Step 2 |
| 4 | Divide by 7 | `f4000000-…` | Step 3 |

**Concept ladder:** mirrors Multiplication II — each divisor is the inverse of the corresponding multiplication step.

---

#### Challenge 7 — Multiplication III
> **ID:** `bd000000-0000-0000-0000-000000000000` · **Slug:** `multiplication_iii`
> **Prereqs:** Division II
> **Goal:** Complete the times tables: master ×8, ×9, then fluently mix all facts.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Multiply by 8 | `ee000000-…` | _(none)_ |
| 2 | Multiply by 9 | `ef000000-…` | Step 1 |
| 3 | Mixed Times Tables (1–9) | `f0000000-…` | Step 2 |

**Concept ladder:** ×8 (double-double-double) → ×9 (10× − 1×, finger trick) → full-table fluency under randomised drill.

---

#### Challenge 8 — Division III
> **ID:** `bf000000-0000-0000-0000-000000000000` · **Slug:** `division_iii`
> **Prereqs:** Multiplication III
> **Goal:** Divide by 8 and 9, then fluently mix all division facts.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Divide by 8 | `f5000000-…` | _(none)_ |
| 2 | Divide by 9 | `f6000000-…` | Step 1 |
| 3 | Mixed Division Facts (1–9) | `f7000000-…` | Step 2 |

**Concept ladder:** mirrors Multiplication III — inversely applies ×8 and ×9 knowledge, culminating in full randomised division drill.

---

#### Challenge 9 — Order of Operations
> **ID:** `b7000000-0000-0000-0000-000000000000` · **Slug:** `order_of_operations`
> **Prereqs:** Division III
> **Goal:** Evaluate short expressions by choosing the correct operation order.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Multiply Then Add | `d4000000-…` | _(none)_ |
| 2 | Multiply Then Subtract | `d5000000-…` | Step 1 |
| 3 | Parentheses First | `d6000000-…` | Step 2 |
| 4 | Mixed Expressions | `d7000000-…` | Step 3 |

**Concept ladder:** ×-before-+ rule with addition → ×-before-− rule → parentheses override → combining all rules in short mixed expressions.

---

### Stage 3 · Pre-Algebra Bridge

#### Challenge 10 — Arithmetic Review
> **ID:** `bg000000-0000-0000-0000-000000000000` · **Slug:** `arithmetic_review`
> **Prereqs:** Order of Operations
> **Goal:** Consolidate all four operations with mixed practice before entering algebra.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Mixed Addition and Subtraction | `f8000000-…` | _(none)_ |
| 2 | Mixed Multiplication and Division | `f9000000-…` | Step 1 |
| 3 | All Four Operations | `fa000000-…` | Step 2 |
| 4 | Multi-Step Mental Math | `fb000000-…` | Step 3 |

**Concept ladder:** fluency drill mixing +/− → fluency drill mixing ×/÷ → unrestricted four-operation mixing → chaining two operations in a single mental calculation.

---

#### Challenge 11 — Expressions with Variables
> **ID:** `b8000000-0000-0000-0000-000000000000` · **Slug:** `expressions_with_variables`
> **Prereqs:** Arithmetic Review
> **Goal:** Evaluate expressions by replacing one variable with a given number.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Evaluate x + a | `d8000000-…` | _(none)_ |
| 2 | Evaluate x − a | `d9000000-…` | Step 1 |
| 3 | Evaluate ax | `da000000-…` | Step 2 |
| 4 | Evaluate x / a | `db000000-…` | Step 3 |

**Concept ladder:** substitution with addition → with subtraction → with multiplication (coefficient notation) → with division — one operation each, so the concept of substitution is isolated.

---

### Stage 4 · Algebra Foundations

#### Challenge 12 — One-Step Equations
> **ID:** `b9000000-0000-0000-0000-000000000000` · **Slug:** `one_step_equations`
> **Prereqs:** Expressions with Variables
> **Goal:** Solve equations with one inverse operation.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Solve x + a = b | `dc000000-…` | _(none)_ |
| 2 | Solve x − a = b | `dd000000-…` | Step 1 |
| 3 | Solve ax = b | `de000000-…` | Step 2 |
| 4 | Solve x / a = b | `df000000-…` | Step 3 |

**Concept ladder:** undo addition (subtract) → undo subtraction (add) → undo multiplication (divide) → undo division (multiply).

---

#### Challenge 13 — Two-Step Equations
> **ID:** `ba000000-0000-0000-0000-000000000000` · **Slug:** `two_step_equations`
> **Prereqs:** One-Step Equations
> **Goal:** Solve equations by undoing two operations in the correct order.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Solve ax + b = c | `e1000000-…` | _(none)_ |
| 2 | Solve ax − b = c | `e2000000-…` | Step 1 |
| 3 | Solve x/a + b = c | `e3000000-…` | Step 2 |
| 4 | Solve x/a − b = c | `e4000000-…` | Step 3 |

**Concept ladder:** undo addition then division (multiply form) → undo subtraction then division → undo addition then multiplication (divide form) → undo subtraction then multiplication.

---

#### Challenge 14 — Systems of Equations
> **ID:** `bb000000-0000-0000-0000-000000000000` · **Slug:** `systems_of_equations`
> **Prereqs:** Two-Step Equations
> **Goal:** Use substitution and paired equations to solve for two variables.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Substitute x into y = x + a | `e5000000-…` | _(none)_ |
| 2 | Solve a System — Find x | `e6000000-…` | Step 1 |
| 3 | Solve a System — Find y | `e7000000-…` | Step 2 |
| 4 | Standard Form: Find x | `e8000000-…` | Step 3 |
| 5 | Standard Form: Find y | `e9000000-…` | Step 4 |

**Concept ladder:** pure substitution (given x, compute y) → substitution to find x → back-substitute to find y → addition/elimination method for x → addition/elimination method for y.

---

## ⚗️ Physics _(placeholder)_

### Challenge — Force and Motion
> **ID:** `b3000000-0000-0000-0000-000000000000` · **Slug:** `force`
> **Prereqs:** Math: Addition (all steps)
> **Goal:** Newton's laws and force concepts. _(content not yet developed)_

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Newton's First Law | `c7000000-…` | _(none)_ |

---

## 🏛️ History _(placeholder)_

### Challenge — Ancient Rome
> **ID:** `b4000000-0000-0000-0000-000000000000` · **Slug:** `ancient_rome`
> **Prereqs:** _(none)_
> **Goal:** The Roman Republic and Empire. _(content not yet developed)_

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Roman Republic | `c8000000-…` | _(none)_ |

---

## 🧩 Design Conventions

### Step Design Rules

When designing a new challenge, follow these rules so the AI generator can produce clean,
streak-friendly questions for each step:

1. **Each step = one concept.** A question for that step should require knowing exactly one
   technique. If a step needs two techniques, split it.

2. **Steps form a ladder.** Step N should be strictly harder than step N−1, and mastering step N
   should make step N+1 obviously accessible.

3. **Answers must be numeric or clearly stated.** The current prompt requires answers to be
   unambiguous single values so the evaluator can mark them correct/incorrect reliably.

4. **Phrase constraints as examples.** In `OllamaQuestionGenerator.GetStepConstraints()`, always
   include a sample phrasing like `'What is the value of x in x + 3 = 7?'`. The validator rejects
   bare equations as question text; example phrasing in the constraint prevents that.

5. **Avoid `RequireUltimateChallenge = true`** until the Ultimate Challenge UI is implemented.

6. **Use simple whole-number inputs** at the early steps. Introduce complexity through structure
   (e.g., two variables) rather than large numbers.

7. **Add step constraints to `OllamaQuestionGenerator.GetStepConstraints()`** for every new
   challenge slug. The switch normalises challenge names with `Replace(" ", "_")`.

### Stage Design Rules

8. **Every challenge belongs to a stage.** Set `StageNumber` and `StageName` in
   `ChallengeDataManager.cs` when creating a challenge. Stages are purely visual groupings — they
   do not add extra locking beyond the normal prerequisite chain.

9. **Stage names should describe the cognitive level**, not list challenge names.
   Good: "Arithmetic Mastery", "Pre-Algebra Bridge". Bad: "Division and Multiplication".

10. **Stages should feel roughly equal in effort.** Aim for 4–6 challenges per stage. A single
    review/consolidation challenge at the end of a stage (before moving to the next stage) is
    strongly recommended.

11. **Consolidation challenges** (like Arithmetic Review) should:
    - Come last in their stage, gating the first challenge of the next stage.
    - Mix all skills from the current stage rather than introducing new ones.
    - Have steps that progressively widen the mix (e.g., Step 1: only +/−; Step 3: all four ops).

12. **Skill families should be fully mastered before advancing.** For Math: multiplication through
    all digits (1–9) must be complete before higher algebra. Split a large skill family into
    multiple challenges (Multiplication I, II, III) and interleave their division inverses to
    keep learning connected and paced.

13. **The full subject journey should be visible.** Document all planned future stages in the
    Journey Overview table, even if they are not yet implemented. Seeing "Stage 4 of 10" motivates
    students more than an opaque progress bar.

14. **Every challenge and step has a `Difficulty` (0.0–1.0).** This value represents where the
    item sits on the full subject journey (0.0 = very first lesson, 1.0 = maximum subject mastery).
    Use it to scale EXP/coin rewards so higher-difficulty steps feel more rewarding, and to power
    the planned adaptive skip-unlock system (see Future Features below). Assignment guidelines:
    - Stage 1 roughly maps to 0.01–0.20, Stage 2 to 0.20–0.45, Stage 3 to 0.45–0.55, Stage 4 to 0.55–0.70.
    - Steps within a challenge should form a smooth sub-range (e.g., 0.08–0.13 for Subtraction).
    - Systems of Equations (the current endpoint) sits at ~0.68 — earned, but only 40% through a
      full Math journey to Calculus III. This keeps rewards motivating at every stage.

### Progression Pacing

- **Stage 1 (Arithmetic Foundations):** 4 challenges, ~17 steps — foundational, high repetition, very accessible.
- **Stage 2 (Arithmetic Mastery):** 5 challenges, ~19 steps — intensive drill of all arithmetic facts.
- **Stage 3 (Pre-Algebra Bridge):** 2 challenges, ~8 steps — consolidation then first variable concept.
- **Stage 4 (Algebra Foundations):** 3 challenges, ~13 steps — equation solving, culminating in Systems.
- **Future stages:** aim for 3–5 challenges each with a consolidation challenge at the end.

---

## 🔮 Future Features

### Skip-Unlock for Advanced Players

**Problem:** A player who already knows up to Calculus I must repeat every step from Addition onwards before reaching content at their level. This is de-motivating.

**Planned mechanism (not yet implemented):**

1. **Entry Assessment** — When a player starts a new subject (or explicitly triggers "Test my level"), the system probes them at increasing difficulty levels using the `Difficulty` value on each step. It asks a small number of questions (3–5) per difficulty bracket, starting from the midpoint of the subject range.

2. **Mastery Confirmation** — If the player answers correctly at difficulty D, try D + 0.15. If they fail at D, fall back to D − 0.10. The system binary-searches for the player's actual mastery ceiling.

3. **Batch Unlock** — Once the assessment confirms mastery up to difficulty D, all steps and challenges with `Difficulty ≤ D` are marked as completed with a synthetic mastery score (e.g., 0.85). The player starts at the first unlocked step above D.

4. **Adaptive Rewards** — Steps that were skipped via assessment award reduced EXP/coins (e.g., 20% of normal) if the player ever revisits them. Steps just above the skip threshold award a bonus (e.g., 120% EXP) to celebrate the bridge from known to new material.

5. **Gradual Reveal** — Rather than unlocking 50 steps at once (which can feel like the game is giving up its content), the assessment unlocks a "sprint path" — the minimum set of steps needed to reach the player's assessed level — and hides optional earlier steps unless the player explicitly asks to review them.

**Foundation already in place:**
- `Step.Difficulty` and `Challenge.Difficulty` (0.0–1.0) are the keys to querying the right difficulty bracket.
- `PlayerDataManager.SavePlayer()` and `LogQuestionResultAsync()` can record assessment results.
- `ChallengeDataManager.GetStepById()` / `GetChallenge()` allow arbitrary step selection by difficulty range.

**Implementation notes for when this is built:**
- Add an `AssessmentMode` flag to `QuestionFlowManager` that bypasses the normal streak requirement and instead collects a pass/fail signal per question.
- Add a `SkipUnlockStep(stepId, syntheticMastery)` method to `PlayerDataManager`.
- The reward scaling formula should reference `step.Difficulty` so skipped-and-revisited steps always give less than genuinely-completed ones.

---



```
[ ] 1. Decide which stage the challenge belongs to. If it introduces a new cognitive level,
        consider creating a new stage.
[ ] 2. Pick the next free UUID prefix for the challenge (bX000000-…) and for each step.
        Next free challenge prefix after bg: bh, bi, …
        Next free step prefix after fb: fc, fd, …
[ ] 3. Add public const string CHALLENGE_XXX_ID and STEP_XXX_N_ID to ChallengeDataManager.cs.
[ ] 4. Add the Challenge object (with StageNumber, StageName, and Difficulty) and Steps (each
        with a Difficulty value on the 0.0–1.0 global scale) to InitializeHardcodedChallenges().
        Set prerequisites to the last challenge in the chain.
[ ] 5. Add step constraints to OllamaQuestionGenerator.GetStepConstraints() for the slug.
        Always include a sample question phrasing in the constraint string.
[ ] 6. Create a new supabase/migrations/YYYYMMDDHHMMSS_<name>.sql with the new challenges,
        steps, and prerequisite rows. Update stage_number/stage_name on existing challenges if
        the new challenge changes any stage boundaries.
[ ] 7. Update CURRICULUM.md: add the challenge to the correct stage section, update the
        Journey Overview table if needed, and add the new UUID prefixes to the Quick Reference.
[ ] 8. Run the game in the editor and verify the challenge appears, steps unlock in order,
        and Ollama generates the correct question type for each step.
```

---

## 🔑 UUID Quick Reference

Full UUIDs all follow the pattern `XXXXXXXX-0000-0000-0000-000000000000`.
The short prefix below is enough to uniquely identify each object in the seeded catalog.

| Prefix | Object |
|--------|--------|
| `a1…` | Subject: Math |
| `a2…` | Subject: Physics |
| `a3…` | Subject: History |
| `b1…` | Challenge: Addition (Math Stage 1) |
| `b2…` | Challenge: Subtraction (Math Stage 1) |
| `b3…` | Challenge: Force and Motion (Physics Stage 1) |
| `b4…` | Challenge: Ancient Rome (History Stage 1) |
| `b5…` | Challenge: Multiplication I (Math Stage 1) |
| `b6…` | Challenge: Division I (Math Stage 1) |
| `b7…` | Challenge: Order of Operations (Math Stage 2) |
| `b8…` | Challenge: Expressions with Variables (Math Stage 3) |
| `b9…` | Challenge: One-Step Equations (Math Stage 4) |
| `ba…` | Challenge: Two-Step Equations (Math Stage 4) |
| `bb…` | Challenge: Systems of Equations (Math Stage 4) |
| `bc…` | Challenge: Multiplication II (Math Stage 2) |
| `bd…` | Challenge: Multiplication III (Math Stage 2) |
| `be…` | Challenge: Division II (Math Stage 2) |
| `bf…` | Challenge: Division III (Math Stage 2) |
| `bg…` | Challenge: Arithmetic Review (Math Stage 3) |
| `c1…–c4…` | Steps: Addition 1–4 |
| `c5…, c6…, c9…, ca…` | Steps: Subtraction 1,2,5,6 (existing) |
| `fc…, fd…` | Steps: Subtraction 3,4 (Subtract Within 20, Subtract from Tens) |
| `c7…` | Step: Force / Newton's First Law |
| `c8…` | Step: Ancient Rome / Roman Republic |
| `cb…–ce…` | Steps: Multiplication I 1–4 |
| `cf…, d1…–d3…` | Steps: Division I 1–4 |
| `d4…–d7…` | Steps: Order of Operations 1–4 |
| `d8…–db…` | Steps: Expressions with Variables 1–4 |
| `dc…–df…` | Steps: One-Step Equations 1–4 |
| `e1…–e4…` | Steps: Two-Step Equations 1–4 |
| `e5…–e9…` | Steps: Systems of Equations 1–5 |
| `ea…–ed…` | Steps: Multiplication II 1–4 |
| `ee…–f0…` | Steps: Multiplication III 1–3 |
| `f1…–f4…` | Steps: Division II 1–4 |
| `f5…–f7…` | Steps: Division III 1–3 |
| `f8…–fb…` | Steps: Arithmetic Review 1–4 |
| _(next: `fe…`)_ | _(next free step prefix after fc, fd)_ |
