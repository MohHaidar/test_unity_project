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

Full prerequisite chain: Addition → Subtraction → Multiplication → Division →
Order of Operations → Expressions with Variables → One-Step Equations →
Two-Step Equations → Systems of Equations.

---

### Challenge 1 — Addition
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

### Challenge 2 — Subtraction
> **ID:** `b2000000-0000-0000-0000-000000000000` · **Slug:** `subtraction`
> **Prereqs:** Addition (all steps)
> **Goal:** Use subtraction to find differences, missing parts, and two-digit answers.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Subtract Within 10 | `c5000000-…` | _(none)_ |
| 2 | Find the Missing Addend | `c6000000-…` | Step 1 |
| 3 | Two-Digit No Borrow | `c9000000-…` | Step 2 |
| 4 | Two-Digit With Borrow | `ca000000-…` | Step 3 |

**Concept ladder:** basic take-away facts → inverse addition (missing addend bridges to algebraic thinking) → column subtraction without regrouping → column subtraction with regrouping (borrow).

---

### Challenge 3 — Multiplication
> **ID:** `b5000000-0000-0000-0000-000000000000` · **Slug:** `multiplication`
> **Prereqs:** Subtraction (all steps)
> **Goal:** Treat multiplication as repeated groups and build fluency with key facts.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Equal Groups | `cb000000-…` | _(none)_ |
| 2 | Multiply by 2 | `cc000000-…` | Step 1 |
| 3 | Multiply by 5 | `cd000000-…` | Step 2 |
| 4 | Multiply by 10 | `ce000000-…` | Step 3 |

**Concept ladder:** grouping concept → doubling (×2) → skip-count by 5 → place-value shortcut (×10). Deliberately focuses on the three "gateway" multipliers before the full times table.

---

### Challenge 4 — Division
> **ID:** `b6000000-0000-0000-0000-000000000000` · **Slug:** `division`
> **Prereqs:** Multiplication (all steps)
> **Goal:** Connect division to equal sharing and inverse multiplication facts.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Sharing Equally | `cf000000-…` | _(none)_ |
| 2 | Divide by 2 | `d1000000-…` | Step 1 |
| 3 | Divide by 5 | `d2000000-…` | Step 2 |
| 4 | Divide by 10 | `d3000000-…` | Step 3 |

**Concept ladder:** fair-share intuition → halving (÷2 as inverse of ×2) → ÷5 → ÷10 place-value shortcut. Mirrors the multiplication steps so each fact family is complete before moving on.

---

### Challenge 5 — Order of Operations
> **ID:** `b7000000-0000-0000-0000-000000000000` · **Slug:** `order_of_operations`
> **Prereqs:** Division (all steps)
> **Goal:** Evaluate short expressions by choosing the correct operation order.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Multiply Then Add | `d4000000-…` | _(none)_ |
| 2 | Multiply Then Subtract | `d5000000-…` | Step 1 |
| 3 | Parentheses First | `d6000000-…` | Step 2 |
| 4 | Mixed Expressions | `d7000000-…` | Step 3 |

**Concept ladder:** MDAS without parentheses (× before +) → (× before −) → parentheses override → combined two-or-three-operation expressions. Each step isolates one rule so the AI prompt can enforce it cleanly.

---

### Challenge 6 — Expressions with Variables
> **ID:** `b8000000-0000-0000-0000-000000000000` · **Slug:** `expressions_with_variables`
> **Prereqs:** Order of Operations (all steps)
> **Goal:** Evaluate expressions by substituting a given value for one variable.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Evaluate x + a | `d8000000-…` | _(none)_ |
| 2 | Evaluate x − a | `d9000000-…` | Step 1 |
| 3 | Evaluate ax | `da000000-…` | Step 2 |
| 4 | Evaluate x / a | `db000000-…` | Step 3 |

**Concept ladder:** variable as placeholder (plug in and add) → plug in and subtract → plug in and multiply → plug in and divide. Prepares students to reverse the operation, which is what equations require.

---

### Challenge 7 — One-Step Equations
> **ID:** `b9000000-0000-0000-0000-000000000000` · **Slug:** `one_step_equations`
> **Prereqs:** Expressions with Variables (all steps)
> **Goal:** Solve equations using one inverse operation.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Solve x + a = b | `dc000000-…` | _(none)_ |
| 2 | Solve x − a = b | `dd000000-…` | Step 1 |
| 3 | Solve ax = b | `de000000-…` | Step 2 |
| 4 | Solve x / a = b | `df000000-…` | Step 3 |

**Concept ladder:** undo addition (subtract) → undo subtraction (add) → undo multiplication (divide) → undo division (multiply). Students practice the balance principle with a single move per equation.

---

### Challenge 8 — Two-Step Equations
> **ID:** `ba000000-0000-0000-0000-000000000000` · **Slug:** `two_step_equations`
> **Prereqs:** One-Step Equations (all steps)
> **Goal:** Solve equations by undoing two operations in the correct order.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Solve ax + b = c | `e1000000-…` | _(none)_ |
| 2 | Solve ax − b = c | `e2000000-…` | Step 1 |
| 3 | Solve x/a + b = c | `e3000000-…` | Step 2 |
| 4 | Solve x/a − b = c | `e4000000-…` | Step 3 |

**Concept ladder:** "multiply then add" reversed (subtract then divide) → same with subtraction → fraction coefficient (divide then add/subtract). Each step keeps one operation type at the outer layer and varies the inner one.

---

### Challenge 9 — Systems of Equations _(final Math chapter)_
> **ID:** `bb000000-0000-0000-0000-000000000000` · **Slug:** `systems_of_equations`
> **Prereqs:** Two-Step Equations (all steps)
> **Goal:** Use substitution and paired equations to find two unknown variables.

| # | Step | ID | Prereq Step |
|---|------|----|-------------|
| 1 | Substitute x into y = x + a | `e5000000-…` | _(none)_ |
| 2 | Solve a System — Find x | `e6000000-…` | Step 1 |
| 3 | Solve a System — Find y | `e7000000-…` | Step 2 |
| 4 | Standard Form: Find x | `e8000000-…` | Step 3 |
| 5 | Standard Form: Find y | `e9000000-…` | Step 4 |

**Concept ladder:** pure substitution with a known value → substitution to isolate x → use that x to find y → standard-form system (add/subtract equations) to isolate x → same system to report y. The two-variable nature is introduced gradually so the AI can always give an unambiguous numeric answer.

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

When designing a new challenge, follow these rules so the AI generator can produce clean,
streak-friendly questions for each step:

1. **Each step = one concept.** A question for that step should require knowing exactly one
   technique. If a step needs two techniques, split it.

2. **Steps form a ladder.** Step N should be strictly harder than step N−1, and mastering step N
   should make step N+1 obviously accessible.

3. **Answers must be numeric or clearly stated.** The current prompt requires answers to be
   unambiguous single values so the evaluator can mark them correct/incorrect reliably.

4. **Avoid `RequireUltimateChallenge = true`** until the Ultimate Challenge UI is implemented.
   The flag exists in `Step.cs` but the UI to launch the challenge is not built yet.

5. **Use simple whole-number inputs** at the early steps of any new challenge, and keep results
   within a range a student can reasonably check mentally. Introduce complexity through structure
   (e.g., two-variable systems) rather than large numbers.

6. **Add step constraints to `OllamaQuestionGenerator.GetStepConstraints()`** for every new
   challenge slug. The switch normalises challenge names with `Replace(" ", "_")`, so a challenge
   named "Ancient Rome" → case `"ancient_rome"`.

---

## 📋 Adding a New Challenge — Checklist

```
[ ] 1. Pick a UUID for the challenge (bX000000-…) and for each step (cX000000-…)
        Use the next free hex digit after the existing ones.
[ ] 2. Add public const string CHALLENGE_XXX_ID and STEP_XXX_N_ID to ChallengeDataManager.cs.
[ ] 3. Add the Challenge and Steps to InitializeHardcodedChallenges() following the pattern.
[ ] 4. Add step constraints to OllamaQuestionGenerator.GetStepConstraints() for the slug.
[ ] 5. Add the challenge and its steps to the Supabase migration file
        supabase/migrations/20260315231000_expand_math_curriculum.sql (or a new 003 migration).
[ ] 6. Update this CURRICULUM.md document with the new challenge table and concept ladder.
[ ] 7. Run the game in the editor and verify the challenge appears, steps unlock in order,
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
| `b1…` | Challenge: Addition |
| `b2…` | Challenge: Subtraction |
| `b3…` | Challenge: Force and Motion |
| `b4…` | Challenge: Ancient Rome |
| `b5…` | Challenge: Multiplication |
| `b6…` | Challenge: Division |
| `b7…` | Challenge: Order of Operations |
| `b8…` | Challenge: Expressions with Variables |
| `b9…` | Challenge: One-Step Equations |
| `ba…` | Challenge: Two-Step Equations |
| `bb…` | Challenge: Systems of Equations |
| `c1…–c4…` | Steps: Addition 1–4 |
| `c5…, c6…, c9…, ca…` | Steps: Subtraction 1–4 |
| `c7…` | Step: Force/Newton's First Law |
| `c8…` | Step: Ancient Rome/Roman Republic |
| `cb…–ce…` | Steps: Multiplication 1–4 |
| `cf…, d1…–d3…` | Steps: Division 1–4 |
| `d4…–d7…` | Steps: Order of Operations 1–4 |
| `d8…–db…` | Steps: Expressions with Variables 1–4 |
| `dc…–df…` | Steps: One-Step Equations 1–4 |
| `e1…–e4…` | Steps: Two-Step Equations 1–4 |
| `e5…–e9…` | Steps: Systems of Equations 1–5 |
