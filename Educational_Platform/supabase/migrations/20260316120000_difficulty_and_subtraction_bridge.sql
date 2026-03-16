-- ============================================================
-- Migration: difficulty_and_subtraction_bridge
-- 
-- • Adds `difficulty` (0.0–1.0 global curriculum scale) to
--   challenges and steps, where 0.0 = first lesson, 1.0 = Calculus III
-- • Inserts two new Subtraction bridge steps:
--     Step 3 – Subtract Within 20 (fc…)
--     Step 4 – Subtract from Tens  (fd…)
--   and renumbers the old steps 3→5 (c9…) and 4→6 (ca…)
-- • Seeds difficulty values for every challenge and step row
-- ============================================================

-- ── Schema ────────────────────────────────────────────────────
ALTER TABLE challenges ADD COLUMN IF NOT EXISTS difficulty float NOT NULL DEFAULT 0.5;
ALTER TABLE steps      ADD COLUMN IF NOT EXISTS difficulty float NOT NULL DEFAULT 0.5;

-- ── New Subtraction bridge steps ─────────────────────────────
-- Renumber OLD steps 3→5 and 4→6 FIRST (unique constraint on challenge_id+number)
UPDATE steps SET number = 5 WHERE id = 'c9000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 6 WHERE id = 'ca000000-0000-0000-0000-000000000000';

INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, require_ultimate, difficulty)
VALUES
  ('fc000000-0000-0000-0000-000000000000',
   'b2000000-0000-0000-0000-000000000000',
   3, 'Subtract Within 20',
   'Bridge through 10 to subtract numbers up to 20 (e.g. 16 - 7)',
   5, 0.80, false, 0.10),
  ('fd000000-0000-0000-0000-000000000000',
   'b2000000-0000-0000-0000-000000000000',
   4, 'Subtract from Tens',
   'Subtract a single-digit from a multiple of 10 (e.g. 30 - 7)',
   5, 0.80, false, 0.11)
ON CONFLICT (id) DO UPDATE SET
    number      = EXCLUDED.number,
    title       = EXCLUDED.title,
    description = EXCLUDED.description,
    difficulty  = EXCLUDED.difficulty;

-- Prerequisites for new steps
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('fc000000-0000-0000-0000-000000000000', 'c6000000-0000-0000-0000-000000000000'),  -- Sub-Within-20 ← Missing-Addend
  ('fd000000-0000-0000-0000-000000000000', 'fc000000-0000-0000-0000-000000000000')   -- Sub-from-Tens ← Sub-Within-20
ON CONFLICT DO NOTHING;

-- Old step 5 (c9) now follows new step 4 (fd) instead of step 2 (c6)
UPDATE step_prerequisites
   SET requires_step_id = 'fd000000-0000-0000-0000-000000000000'
 WHERE step_id               = 'c9000000-0000-0000-0000-000000000000'
   AND requires_step_id  = 'c6000000-0000-0000-0000-000000000000';

-- ── Challenge difficulty values ───────────────────────────────
-- Stage 1 · Arithmetic Foundations
UPDATE challenges SET difficulty = 0.05 WHERE id = 'b1000000-0000-0000-0000-000000000000'; -- Addition
UPDATE challenges SET difficulty = 0.10 WHERE id = 'b2000000-0000-0000-0000-000000000000'; -- Subtraction
UPDATE challenges SET difficulty = 0.15 WHERE id = 'b5000000-0000-0000-0000-000000000000'; -- Multiplication I
UPDATE challenges SET difficulty = 0.19 WHERE id = 'b6000000-0000-0000-0000-000000000000'; -- Division I
-- Stage 2 · Arithmetic Mastery
UPDATE challenges SET difficulty = 0.24 WHERE id = 'bc000000-0000-0000-0000-000000000000'; -- Multiplication II
UPDATE challenges SET difficulty = 0.29 WHERE id = 'be000000-0000-0000-0000-000000000000'; -- Division II
UPDATE challenges SET difficulty = 0.33 WHERE id = 'bd000000-0000-0000-0000-000000000000'; -- Multiplication III
UPDATE challenges SET difficulty = 0.37 WHERE id = 'bf000000-0000-0000-0000-000000000000'; -- Division III
UPDATE challenges SET difficulty = 0.41 WHERE id = 'b7000000-0000-0000-0000-000000000000'; -- Order of Operations
-- Stage 3 · Pre-Algebra Bridge
UPDATE challenges SET difficulty = 0.46 WHERE id = 'b0000000-0000-0000-0000-000000000000'; -- Arithmetic Review
UPDATE challenges SET difficulty = 0.51 WHERE id = 'b8000000-0000-0000-0000-000000000000'; -- Expressions with Variables
-- Stage 4 · Algebra Foundations
UPDATE challenges SET difficulty = 0.56 WHERE id = 'b9000000-0000-0000-0000-000000000000'; -- One-Step Equations
UPDATE challenges SET difficulty = 0.62 WHERE id = 'ba000000-0000-0000-0000-000000000000'; -- Two-Step Equations
UPDATE challenges SET difficulty = 0.68 WHERE id = 'bb000000-0000-0000-0000-000000000000'; -- Systems of Equations
-- Other subjects
UPDATE challenges SET difficulty = 0.15 WHERE id = 'b3000000-0000-0000-0000-000000000000'; -- Force and Motion
UPDATE challenges SET difficulty = 0.10 WHERE id = 'b4000000-0000-0000-0000-000000000000'; -- Ancient Rome

-- ── Step difficulty values ────────────────────────────────────
-- Addition (c1–c4)
UPDATE steps SET difficulty = 0.02 WHERE id = 'c1000000-0000-0000-0000-000000000000'; -- Add Within 10
UPDATE steps SET difficulty = 0.03 WHERE id = 'c2000000-0000-0000-0000-000000000000'; -- Make 10
UPDATE steps SET difficulty = 0.05 WHERE id = 'c3000000-0000-0000-0000-000000000000'; -- Two-Digit No Carry
UPDATE steps SET difficulty = 0.07 WHERE id = 'c4000000-0000-0000-0000-000000000000'; -- Two-Digit With Carry

-- Subtraction (c5, c6, fc, fd, c9, ca)
UPDATE steps SET difficulty = 0.08 WHERE id = 'c5000000-0000-0000-0000-000000000000'; -- Subtract Within 10
UPDATE steps SET difficulty = 0.09 WHERE id = 'c6000000-0000-0000-0000-000000000000'; -- Find the Missing Addend
-- fc (0.10) and fd (0.11) already set in INSERT above
UPDATE steps SET difficulty = 0.12 WHERE id = 'c9000000-0000-0000-0000-000000000000'; -- Two-Digit No Borrow
UPDATE steps SET difficulty = 0.13 WHERE id = 'ca000000-0000-0000-0000-000000000000'; -- Two-Digit With Borrow

-- Multiplication I (cb–ce)
UPDATE steps SET difficulty = 0.14 WHERE id = 'cb000000-0000-0000-0000-000000000000'; -- Equal Groups
UPDATE steps SET difficulty = 0.15 WHERE id = 'cc000000-0000-0000-0000-000000000000'; -- Multiply by 2
UPDATE steps SET difficulty = 0.16 WHERE id = 'cd000000-0000-0000-0000-000000000000'; -- Multiply by 5
UPDATE steps SET difficulty = 0.17 WHERE id = 'ce000000-0000-0000-0000-000000000000'; -- Multiply by 10

-- Division I (cf, d1, d2, d3)
UPDATE steps SET difficulty = 0.18 WHERE id = 'cf000000-0000-0000-0000-000000000000'; -- Sharing Equally
UPDATE steps SET difficulty = 0.19 WHERE id = 'd1000000-0000-0000-0000-000000000000'; -- Divide by 2
UPDATE steps SET difficulty = 0.20 WHERE id = 'd2000000-0000-0000-0000-000000000000'; -- Divide by 5
UPDATE steps SET difficulty = 0.21 WHERE id = 'd3000000-0000-0000-0000-000000000000'; -- Divide by 10

-- Multiplication II (ea–ed)
UPDATE steps SET difficulty = 0.22 WHERE id = 'ea000000-0000-0000-0000-000000000000'; -- Multiply by 3
UPDATE steps SET difficulty = 0.23 WHERE id = 'eb000000-0000-0000-0000-000000000000'; -- Multiply by 4
UPDATE steps SET difficulty = 0.25 WHERE id = 'ec000000-0000-0000-0000-000000000000'; -- Multiply by 6
UPDATE steps SET difficulty = 0.27 WHERE id = 'ed000000-0000-0000-0000-000000000000'; -- Multiply by 7

-- Division II (f1–f4)
UPDATE steps SET difficulty = 0.28 WHERE id = 'f1000000-0000-0000-0000-000000000000'; -- Divide by 3
UPDATE steps SET difficulty = 0.29 WHERE id = 'f2000000-0000-0000-0000-000000000000'; -- Divide by 4
UPDATE steps SET difficulty = 0.30 WHERE id = 'f3000000-0000-0000-0000-000000000000'; -- Divide by 6
UPDATE steps SET difficulty = 0.31 WHERE id = 'f4000000-0000-0000-0000-000000000000'; -- Divide by 7

-- Multiplication III (ee–f0)
UPDATE steps SET difficulty = 0.32 WHERE id = 'ee000000-0000-0000-0000-000000000000'; -- Multiply by 8
UPDATE steps SET difficulty = 0.33 WHERE id = 'ef000000-0000-0000-0000-000000000000'; -- Multiply by 9
UPDATE steps SET difficulty = 0.35 WHERE id = 'f0000000-0000-0000-0000-000000000000'; -- Mixed Times Tables

-- Division III (f5–f7)
UPDATE steps SET difficulty = 0.36 WHERE id = 'f5000000-0000-0000-0000-000000000000'; -- Divide by 8
UPDATE steps SET difficulty = 0.37 WHERE id = 'f6000000-0000-0000-0000-000000000000'; -- Divide by 9
UPDATE steps SET difficulty = 0.38 WHERE id = 'f7000000-0000-0000-0000-000000000000'; -- Mixed Division Facts

-- Order of Operations (d4–d7)
UPDATE steps SET difficulty = 0.39 WHERE id = 'd4000000-0000-0000-0000-000000000000'; -- Multiply Then Add
UPDATE steps SET difficulty = 0.40 WHERE id = 'd5000000-0000-0000-0000-000000000000'; -- Multiply Then Subtract
UPDATE steps SET difficulty = 0.42 WHERE id = 'd6000000-0000-0000-0000-000000000000'; -- Parentheses First
UPDATE steps SET difficulty = 0.44 WHERE id = 'd7000000-0000-0000-0000-000000000000'; -- Mixed Expressions

-- Arithmetic Review (f8–fb)
UPDATE steps SET difficulty = 0.45 WHERE id = 'f8000000-0000-0000-0000-000000000000'; -- Mixed +/-
UPDATE steps SET difficulty = 0.46 WHERE id = 'f9000000-0000-0000-0000-000000000000'; -- Mixed ×÷
UPDATE steps SET difficulty = 0.47 WHERE id = 'fa000000-0000-0000-0000-000000000000'; -- All Four Operations
UPDATE steps SET difficulty = 0.48 WHERE id = 'fb000000-0000-0000-0000-000000000000'; -- Multi-Step Mental Math

-- Expressions with Variables (d8–db)
UPDATE steps SET difficulty = 0.50 WHERE id = 'd8000000-0000-0000-0000-000000000000'; -- Evaluate x + a
UPDATE steps SET difficulty = 0.51 WHERE id = 'd9000000-0000-0000-0000-000000000000'; -- Evaluate x - a
UPDATE steps SET difficulty = 0.52 WHERE id = 'da000000-0000-0000-0000-000000000000'; -- Evaluate ax
UPDATE steps SET difficulty = 0.53 WHERE id = 'db000000-0000-0000-0000-000000000000'; -- Evaluate x / a

-- One-Step Equations (dc–df)
UPDATE steps SET difficulty = 0.54 WHERE id = 'dc000000-0000-0000-0000-000000000000'; -- x + a = b
UPDATE steps SET difficulty = 0.55 WHERE id = 'dd000000-0000-0000-0000-000000000000'; -- x - a = b
UPDATE steps SET difficulty = 0.57 WHERE id = 'de000000-0000-0000-0000-000000000000'; -- ax = b
UPDATE steps SET difficulty = 0.58 WHERE id = 'df000000-0000-0000-0000-000000000000'; -- x / a = b

-- Two-Step Equations (e1–e4)
UPDATE steps SET difficulty = 0.60 WHERE id = 'e1000000-0000-0000-0000-000000000000'; -- ax + b = c
UPDATE steps SET difficulty = 0.61 WHERE id = 'e2000000-0000-0000-0000-000000000000'; -- ax - b = c
UPDATE steps SET difficulty = 0.63 WHERE id = 'e3000000-0000-0000-0000-000000000000'; -- x/a + b = c
UPDATE steps SET difficulty = 0.64 WHERE id = 'e4000000-0000-0000-0000-000000000000'; -- x/a - b = c

-- Systems of Equations (e5–e9)
UPDATE steps SET difficulty = 0.65 WHERE id = 'e5000000-0000-0000-0000-000000000000'; -- Substitute x
UPDATE steps SET difficulty = 0.67 WHERE id = 'e6000000-0000-0000-0000-000000000000'; -- Find x
UPDATE steps SET difficulty = 0.68 WHERE id = 'e7000000-0000-0000-0000-000000000000'; -- Find y
UPDATE steps SET difficulty = 0.70 WHERE id = 'e8000000-0000-0000-0000-000000000000'; -- Standard form, x
UPDATE steps SET difficulty = 0.70 WHERE id = 'e9000000-0000-0000-0000-000000000000'; -- Standard form, y

-- Other subjects
UPDATE steps SET difficulty = 0.15 WHERE id = 'c7000000-0000-0000-0000-000000000000'; -- Newton's First Law
UPDATE steps SET difficulty = 0.10 WHERE id = 'c8000000-0000-0000-0000-000000000000'; -- Roman Republic
