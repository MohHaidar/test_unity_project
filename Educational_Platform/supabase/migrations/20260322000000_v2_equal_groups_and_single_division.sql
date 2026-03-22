-- ============================================================
--  v2 Curriculum Migration: Equal Groups expansion + single Division
--  Applied after: 20260321000000_step_unlocks_and_single_multiplication.sql
--
--  Actual DB column names (verified from prior migrations):
--    steps:  id, challenge_id, number, title, description,
--            streak_goal, mastery_target, require_ultimate, difficulty
--    challenges: id, subject_id, name, slug, description,
--                stage_number, stage_name, difficulty
--    challenge_prerequisites: challenge_id, requires_challenge_id
--
--  Multiplication challenge ID: b5000000-0000-0000-0000-000000000000
--  Actual mult step UUIDs (after migration 20260321):
--    cb=Equal Groups(1), cc=×10(2), cd=×2(3), ce=×5(4),
--    ea=×4(5), eb=×3(6), ec=×8(7), ed=×6(8), ee=×7(9), ef=×9(10), f0=Mixed(11)
--  Division I (b6) steps:
--    cf=Sharing Equally(1), d1=÷2(2), d2=÷5(3), d3=÷10(4)
--  Division II (be) steps: f1=÷3(1), f2=÷4(2), f3=÷6(3), f4=÷7(4)
--  Division III (bf) steps: f5=÷8(1), f6=÷9(2), f7=Mixed(3)
-- ============================================================

-- ────────────────────────────────────────────────────────────
-- 1. Expand Multiplication: rename step 1, shift steps 2–11 → 6–15
-- ────────────────────────────────────────────────────────────
UPDATE steps SET title = '2 Equal Groups' WHERE id = 'cb000000-0000-0000-0000-000000000000';

-- Shift existing times-table steps up by 4 (descending to avoid any unique-index conflicts)
UPDATE steps SET number = 15 WHERE id = 'f0000000-0000-0000-0000-000000000000';  -- Mixed 11→15
UPDATE steps SET number = 14 WHERE id = 'ef000000-0000-0000-0000-000000000000';  -- ×9   10→14
UPDATE steps SET number = 13 WHERE id = 'ee000000-0000-0000-0000-000000000000';  -- ×7    9→13
UPDATE steps SET number = 12 WHERE id = 'ed000000-0000-0000-0000-000000000000';  -- ×6    8→12
UPDATE steps SET number = 11 WHERE id = 'ec000000-0000-0000-0000-000000000000';  -- ×8    7→11
UPDATE steps SET number = 10 WHERE id = 'eb000000-0000-0000-0000-000000000000';  -- ×3    6→10
UPDATE steps SET number =  9 WHERE id = 'ea000000-0000-0000-0000-000000000000';  -- ×4    5→9
UPDATE steps SET number =  8 WHERE id = 'ce000000-0000-0000-0000-000000000000';  -- ×5    4→8
UPDATE steps SET number =  7 WHERE id = 'cd000000-0000-0000-0000-000000000000';  -- ×2    3→7
UPDATE steps SET number =  6 WHERE id = 'cc000000-0000-0000-0000-000000000000';  -- ×10   2→6

-- Insert the 4 new Equal Groups steps (numbers 2–5) into Multiplication (b5)
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, require_ultimate, difficulty)
VALUES
  ('10000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 2, '3 Equal Groups',  '3 equal groups — count all items', 5, 0.80, false, 0.12),
  ('11000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 3, '4 Equal Groups',  '4 equal groups — count all items', 5, 0.80, false, 0.13),
  ('12000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 4, '5 Equal Groups',  '5 equal groups — count all items', 5, 0.80, false, 0.13),
  ('13000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 5, 'Groups & Arrays', 'Bridge equal groups to multiplication notation', 5, 0.80, false, 0.14)
ON CONFLICT (id) DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 2. Update step prerequisites for the new Equal Groups chain
-- ────────────────────────────────────────────────────────────
-- Old: ×10(cc) required Equal Groups(cb) directly → remove it
DELETE FROM step_prerequisites
WHERE step_id = 'cc000000-0000-0000-0000-000000000000'
  AND requires_step_id = 'cb000000-0000-0000-0000-000000000000';

-- New chain: 2EG(cb) → 3EG → 4EG → 5EG → Bridge → ×10(cc)
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('10000000-0000-0000-0000-000000000000', 'cb000000-0000-0000-0000-000000000000'),
  ('11000000-0000-0000-0000-000000000000', '10000000-0000-0000-0000-000000000000'),
  ('12000000-0000-0000-0000-000000000000', '11000000-0000-0000-0000-000000000000'),
  ('13000000-0000-0000-0000-000000000000', '12000000-0000-0000-0000-000000000000'),
  ('cc000000-0000-0000-0000-000000000000', '13000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 3. Update step_unlocks: ×10(cc) unlocks Division; remove old links
-- ────────────────────────────────────────────────────────────
DELETE FROM step_unlocks
WHERE unlocks_challenge_id IN (
  'b6000000-0000-0000-0000-000000000000',
  'be000000-0000-0000-0000-000000000000',
  'bf000000-0000-0000-0000-000000000000'
);

INSERT INTO step_unlocks (step_id, unlocks_challenge_id) VALUES
  ('cc000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 4. Consolidate Division into single challenge b6
-- ────────────────────────────────────────────────────────────
UPDATE challenges
SET name        = 'Division',
    description = 'Connect division to equal sharing; steps unlock in step with the Multiplication times tables you have mastered',
    slug        = 'division'
WHERE id = 'b6000000-0000-0000-0000-000000000000';

-- Clear old Division I step prerequisites (will be rebuilt with cross-challenge prereqs)
DELETE FROM step_prerequisites
WHERE step_id IN (
  'd1000000-0000-0000-0000-000000000000',
  'd2000000-0000-0000-0000-000000000000',
  'd3000000-0000-0000-0000-000000000000'
);

-- Renumber Division I steps: current d1=2(÷2), d2=3(÷5), d3=4(÷10)
--   → target d3=2(÷10), d1=3(÷2), d2=4(÷5)
-- Use temp numbers first to avoid any unique-index conflict
UPDATE steps SET number = 40 WHERE id = 'd3000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 41 WHERE id = 'd1000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 42 WHERE id = 'd2000000-0000-0000-0000-000000000000';
-- Then apply final numbers
UPDATE steps SET number = 2, title = 'Divide by 10' WHERE id = 'd3000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 3, title = 'Divide by 2'  WHERE id = 'd1000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 4, title = 'Divide by 5'  WHERE id = 'd2000000-0000-0000-0000-000000000000';

-- Clear old Division II/III step prerequisites
DELETE FROM step_prerequisites
WHERE step_id IN (
  'f1000000-0000-0000-0000-000000000000',
  'f2000000-0000-0000-0000-000000000000',
  'f3000000-0000-0000-0000-000000000000',
  'f4000000-0000-0000-0000-000000000000',
  'f5000000-0000-0000-0000-000000000000',
  'f6000000-0000-0000-0000-000000000000',
  'f7000000-0000-0000-0000-000000000000'
);

-- Move Division II + III steps into the Division challenge (b6) with new numbers
-- (number and challenge_id set atomically in each UPDATE to avoid constraint conflicts)
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number =  5, title = 'Divide by 4'               WHERE id = 'f2000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number =  6, title = 'Divide by 3'               WHERE id = 'f1000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number =  7, title = 'Divide by 8'               WHERE id = 'f5000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number =  8, title = 'Divide by 6'               WHERE id = 'f3000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number =  9, title = 'Divide by 7'               WHERE id = 'f4000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number = 10, title = 'Divide by 9'               WHERE id = 'f6000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', number = 11, title = 'Mixed Division Facts (1-10)' WHERE id = 'f7000000-0000-0000-0000-000000000000';

-- Add cross-challenge step prerequisites for all division steps
-- (Correct mult step UUIDs: cc=×10, cd=×2, ce=×5, ea=×4, eb=×3, ec=×8, ed=×6, ee=×7, ef=×9, f0=Mixed)
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('d3000000-0000-0000-0000-000000000000', 'cf000000-0000-0000-0000-000000000000'),  -- ÷10 req Sharing Equally
  ('d3000000-0000-0000-0000-000000000000', 'cc000000-0000-0000-0000-000000000000'),  -- ÷10 req ×10
  ('d1000000-0000-0000-0000-000000000000', 'd3000000-0000-0000-0000-000000000000'),  -- ÷2 req ÷10
  ('d1000000-0000-0000-0000-000000000000', 'cd000000-0000-0000-0000-000000000000'),  -- ÷2 req ×2
  ('d2000000-0000-0000-0000-000000000000', 'd1000000-0000-0000-0000-000000000000'),  -- ÷5 req ÷2
  ('d2000000-0000-0000-0000-000000000000', 'ce000000-0000-0000-0000-000000000000'),  -- ÷5 req ×5
  ('f2000000-0000-0000-0000-000000000000', 'd2000000-0000-0000-0000-000000000000'),  -- ÷4 req ÷5
  ('f2000000-0000-0000-0000-000000000000', 'ea000000-0000-0000-0000-000000000000'),  -- ÷4 req ×4
  ('f1000000-0000-0000-0000-000000000000', 'f2000000-0000-0000-0000-000000000000'),  -- ÷3 req ÷4
  ('f1000000-0000-0000-0000-000000000000', 'eb000000-0000-0000-0000-000000000000'),  -- ÷3 req ×3
  ('f5000000-0000-0000-0000-000000000000', 'f1000000-0000-0000-0000-000000000000'),  -- ÷8 req ÷3
  ('f5000000-0000-0000-0000-000000000000', 'ec000000-0000-0000-0000-000000000000'),  -- ÷8 req ×8
  ('f3000000-0000-0000-0000-000000000000', 'f5000000-0000-0000-0000-000000000000'),  -- ÷6 req ÷8
  ('f3000000-0000-0000-0000-000000000000', 'ed000000-0000-0000-0000-000000000000'),  -- ÷6 req ×6
  ('f4000000-0000-0000-0000-000000000000', 'f3000000-0000-0000-0000-000000000000'),  -- ÷7 req ÷6
  ('f4000000-0000-0000-0000-000000000000', 'ee000000-0000-0000-0000-000000000000'),  -- ÷7 req ×7
  ('f6000000-0000-0000-0000-000000000000', 'f4000000-0000-0000-0000-000000000000'),  -- ÷9 req ÷7
  ('f6000000-0000-0000-0000-000000000000', 'ef000000-0000-0000-0000-000000000000'),  -- ÷9 req ×9
  ('f7000000-0000-0000-0000-000000000000', 'f6000000-0000-0000-0000-000000000000'),  -- Mixed req ÷9
  ('f7000000-0000-0000-0000-000000000000', 'f0000000-0000-0000-0000-000000000000')   -- Mixed req Mixed mult
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 5. Retire Division II and III challenges
--    Steps have already been moved to b6 above.
-- ────────────────────────────────────────────────────────────
-- Remove challenge prerequisites that reference or belong to be/bf
DELETE FROM challenge_prerequisites
WHERE challenge_id        IN ('be000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000')
   OR requires_challenge_id IN ('be000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000');

-- Delete the now-empty challenges
DELETE FROM challenges WHERE id IN (
  'be000000-0000-0000-0000-000000000000',
  'bf000000-0000-0000-0000-000000000000'
);

-- ────────────────────────────────────────────────────────────
-- 6. Update Order of Operations: requires Division (b6) not Division III (bf)
-- ────────────────────────────────────────────────────────────
DELETE FROM challenge_prerequisites
WHERE challenge_id = 'b7000000-0000-0000-0000-000000000000';

INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id) VALUES
  ('b7000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

