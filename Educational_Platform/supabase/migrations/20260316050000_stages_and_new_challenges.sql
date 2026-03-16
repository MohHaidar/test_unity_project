-- Migration: stages_and_new_challenges
-- Adds stage_number / stage_name columns to challenges,
-- inserts Multiplication II/III, Division II/III, and Arithmetic Review challenges,
-- and repoints the prerequisite chain introduced by Stage 2.
-- Safe to re-run (INSERT … ON CONFLICT DO UPDATE / DO NOTHING).

-- ─── 1. Add stage columns ────────────────────────────────────────────────────
ALTER TABLE challenges ADD COLUMN IF NOT EXISTS stage_number integer NOT NULL DEFAULT 1;
ALTER TABLE challenges ADD COLUMN IF NOT EXISTS stage_name   text    NOT NULL DEFAULT '';

-- ─── 2. Stamp stage numbers on all existing Math challenges ──────────────────
-- Stage 1: Arithmetic Foundations
UPDATE challenges SET stage_number = 1, stage_name = 'Arithmetic Foundations'
  WHERE id IN (
    'b1000000-0000-0000-0000-000000000000',  -- Addition
    'b2000000-0000-0000-0000-000000000000',  -- Subtraction
    'b5000000-0000-0000-0000-000000000000',  -- Multiplication (now "Multiplication I")
    'b6000000-0000-0000-0000-000000000000'   -- Division (now "Division I")
  );

-- Rename Multiplication → Multiplication I, Division → Division I
UPDATE challenges SET name = 'Multiplication I' WHERE id = 'b5000000-0000-0000-0000-000000000000';
UPDATE challenges SET name = 'Division I'        WHERE id = 'b6000000-0000-0000-0000-000000000000';

-- Stage 2: Arithmetic Mastery (existing Order of Operations moves here)
UPDATE challenges SET stage_number = 2, stage_name = 'Arithmetic Mastery'
  WHERE id = 'b7000000-0000-0000-0000-000000000000';  -- Order of Operations

-- Stage 3: Pre-Algebra Bridge (existing Expressions with Variables moves here)
UPDATE challenges SET stage_number = 3, stage_name = 'Pre-Algebra Bridge'
  WHERE id = 'b8000000-0000-0000-0000-000000000000';  -- Expressions with Variables

-- Stage 4: Algebra Foundations
UPDATE challenges SET stage_number = 4, stage_name = 'Algebra Foundations'
  WHERE id IN (
    'b9000000-0000-0000-0000-000000000000',  -- One-Step Equations
    'ba000000-0000-0000-0000-000000000000',  -- Two-Step Equations
    'bb000000-0000-0000-0000-000000000000'   -- Systems of Equations
  );

-- ─── 3. Insert new Stage 2 challenges ────────────────────────────────────────
INSERT INTO challenges (id, subject_id, name, slug, description, stage_number, stage_name, created_at)
VALUES
  ('bc000000-0000-0000-0000-000000000000',
   'a1000000-0000-0000-0000-000000000000',
   'Multiplication II', 'multiplication_ii',
   'Master the ×3, ×4, ×6, ×7 times tables',
   2, 'Arithmetic Mastery', now()),

  ('bd000000-0000-0000-0000-000000000000',
   'a1000000-0000-0000-0000-000000000000',
   'Multiplication III', 'multiplication_iii',
   'Complete the times tables: master ×8, ×9, then fluently mix all facts',
   2, 'Arithmetic Mastery', now()),

  ('be000000-0000-0000-0000-000000000000',
   'a1000000-0000-0000-0000-000000000000',
   'Division II', 'division_ii',
   'Divide by 3, 4, 6, and 7 using known times-table inverses',
   2, 'Arithmetic Mastery', now()),

  ('bf000000-0000-0000-0000-000000000000',
   'a1000000-0000-0000-0000-000000000000',
   'Division III', 'division_iii',
   'Divide by 8 and 9, then fluently mix all division facts',
   2, 'Arithmetic Mastery', now()),

  ('b0000000-0000-0000-0000-000000000000',
   'a1000000-0000-0000-0000-000000000000',
   'Arithmetic Review', 'arithmetic_review',
   'Consolidate all four operations with mixed practice before entering algebra',
   3, 'Pre-Algebra Bridge', now())

ON CONFLICT (id) DO UPDATE
  SET name         = EXCLUDED.name,
      description  = EXCLUDED.description,
      stage_number = EXCLUDED.stage_number,
      stage_name   = EXCLUDED.stage_name;

-- ─── 4. Insert new steps ─────────────────────────────────────────────────────
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, require_ultimate)
VALUES
  -- Multiplication II
  ('ea000000-0000-0000-0000-000000000000', 'bc000000-0000-0000-0000-000000000000', 1, 'Multiply by 3', 'Multiply by 3', 5, 0.80, false),
  ('eb000000-0000-0000-0000-000000000000', 'bc000000-0000-0000-0000-000000000000', 2, 'Multiply by 4', 'Multiply by 4', 5, 0.80, false),
  ('ec000000-0000-0000-0000-000000000000', 'bc000000-0000-0000-0000-000000000000', 3, 'Multiply by 6', 'Multiply by 6', 5, 0.80, false),
  ('ed000000-0000-0000-0000-000000000000', 'bc000000-0000-0000-0000-000000000000', 4, 'Multiply by 7', 'Multiply by 7', 5, 0.80, false),
  -- Division II
  ('f1000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000', 1, 'Divide by 3', 'Divide by 3', 5, 0.80, false),
  ('f2000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000', 2, 'Divide by 4', 'Divide by 4', 5, 0.80, false),
  ('f3000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000', 3, 'Divide by 6', 'Divide by 6', 5, 0.80, false),
  ('f4000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000', 4, 'Divide by 7', 'Divide by 7', 5, 0.80, false),
  -- Multiplication III
  ('ee000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000', 1, 'Multiply by 8', 'Multiply by 8', 5, 0.80, false),
  ('ef000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000', 2, 'Multiply by 9', 'Multiply by 9', 5, 0.80, false),
  ('f0000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000', 3, 'Mixed Times Tables (1–9)', 'Mixed Times Tables (1–9)', 5, 0.80, false),
  -- Division III
  ('f5000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000', 1, 'Divide by 8', 'Divide by 8', 5, 0.80, false),
  ('f6000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000', 2, 'Divide by 9', 'Divide by 9', 5, 0.80, false),
  ('f7000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000', 3, 'Mixed Division Facts (1–9)', 'Mixed Division Facts (1–9)', 5, 0.80, false),
  -- Arithmetic Review
  ('f8000000-0000-0000-0000-000000000000', 'b0000000-0000-0000-0000-000000000000', 1, 'Mixed Addition and Subtraction', 'Mixed Addition and Subtraction', 5, 0.80, false),
  ('f9000000-0000-0000-0000-000000000000', 'b0000000-0000-0000-0000-000000000000', 2, 'Mixed Multiplication and Division', 'Mixed Multiplication and Division', 5, 0.80, false),
  ('fa000000-0000-0000-0000-000000000000', 'b0000000-0000-0000-0000-000000000000', 3, 'All Four Operations', 'All Four Operations', 5, 0.80, false),
  ('fb000000-0000-0000-0000-000000000000', 'b0000000-0000-0000-0000-000000000000', 4, 'Multi-Step Mental Math', 'Multi-Step Mental Math', 5, 0.80, false)

ON CONFLICT (id) DO NOTHING;

-- ─── 5. Step prerequisites for new challenges ────────────────────────────────
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  -- Multiplication II
  ('eb000000-0000-0000-0000-000000000000', 'ea000000-0000-0000-0000-000000000000'),
  ('ec000000-0000-0000-0000-000000000000', 'eb000000-0000-0000-0000-000000000000'),
  ('ed000000-0000-0000-0000-000000000000', 'ec000000-0000-0000-0000-000000000000'),
  -- Division II
  ('f2000000-0000-0000-0000-000000000000', 'f1000000-0000-0000-0000-000000000000'),
  ('f3000000-0000-0000-0000-000000000000', 'f2000000-0000-0000-0000-000000000000'),
  ('f4000000-0000-0000-0000-000000000000', 'f3000000-0000-0000-0000-000000000000'),
  -- Multiplication III
  ('ef000000-0000-0000-0000-000000000000', 'ee000000-0000-0000-0000-000000000000'),
  ('f0000000-0000-0000-0000-000000000000', 'ef000000-0000-0000-0000-000000000000'),
  -- Division III
  ('f6000000-0000-0000-0000-000000000000', 'f5000000-0000-0000-0000-000000000000'),
  ('f7000000-0000-0000-0000-000000000000', 'f6000000-0000-0000-0000-000000000000'),
  -- Arithmetic Review
  ('f9000000-0000-0000-0000-000000000000', 'f8000000-0000-0000-0000-000000000000'),
  ('fa000000-0000-0000-0000-000000000000', 'f9000000-0000-0000-0000-000000000000'),
  ('fb000000-0000-0000-0000-000000000000', 'fa000000-0000-0000-0000-000000000000')

ON CONFLICT DO NOTHING;

-- ─── 6. Challenge prerequisites (updated chain) ──────────────────────────────
-- Remove old Order of Operations prereq (was Division I) and replace with Division III.
-- Remove old Expressions prereq (was Order of Operations) and replace with Arithmetic Review.
DELETE FROM challenge_prerequisites
  WHERE (challenge_id = 'b7000000-0000-0000-0000-000000000000'
         AND requires_challenge_id = 'b6000000-0000-0000-0000-000000000000')
     OR (challenge_id = 'b8000000-0000-0000-0000-000000000000'
         AND requires_challenge_id = 'b7000000-0000-0000-0000-000000000000');

INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id) VALUES
  -- New Stage 2 chain
  ('bc000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000'), -- Mult II  ← Division I
  ('be000000-0000-0000-0000-000000000000', 'bc000000-0000-0000-0000-000000000000'), -- Div II   ← Mult II
  ('bd000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000'), -- Mult III ← Div II
  ('bf000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000'), -- Div III  ← Mult III
  ('b7000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000'), -- OoO      ← Div III
  -- New Stage 3 chain
  ('b0000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000'), -- Arith Review ← OoO
  ('b8000000-0000-0000-0000-000000000000', 'b0000000-0000-0000-0000-000000000000')  -- Expressions ← Arith Review

ON CONFLICT DO NOTHING;
