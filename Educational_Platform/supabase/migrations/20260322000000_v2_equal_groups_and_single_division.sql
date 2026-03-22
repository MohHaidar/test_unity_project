-- ============================================================
--  v2 Curriculum Migration: Equal Groups expansion + single Division
--  Applied after: 20260321000000_step_unlocks_and_single_multiplication.sql
-- ============================================================

-- ────────────────────────────────────────────────────────────
-- 1. Add Equal Groups steps 2–5 to the Multiplication challenge
-- ────────────────────────────────────────────────────────────
INSERT INTO steps (id, challenge_id, step_number, name, subject, challenge_name, difficulty)
VALUES
  ('10000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000',  2, '3 Equal Groups',  'Math', 'Multiplication', 0.12),
  ('11000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000',  3, '4 Equal Groups',  'Math', 'Multiplication', 0.13),
  ('12000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000',  4, '5 Equal Groups',  'Math', 'Multiplication', 0.13),
  ('13000000-0000-0000-0000-000000000000', 'bd000000-0000-0000-0000-000000000000',  5, 'Groups & Arrays', 'Math', 'Multiplication', 0.14)
ON CONFLICT (id) DO NOTHING;

-- Rename step 1 (equal groups) and shift all former times-table step numbers up by 5
UPDATE steps SET name = '2 Equal Groups' WHERE id = 'cb000000-0000-0000-0000-000000000000';

-- Former step numbers 2–11 become 6–15
UPDATE steps SET step_number = step_number + 4
WHERE challenge_id = 'bd000000-0000-0000-0000-000000000000'
  AND step_number >= 2
  AND id != '10000000-0000-0000-0000-000000000000'
  AND id != '11000000-0000-0000-0000-000000000000'
  AND id != '12000000-0000-0000-0000-000000000000'
  AND id != '13000000-0000-0000-0000-000000000000';

-- Add prerequisites for the new equal-groups steps
INSERT INTO step_prerequisites (step_id, requires_step_id)
VALUES
  ('10000000-0000-0000-0000-000000000000', 'cb000000-0000-0000-0000-000000000000'),
  ('11000000-0000-0000-0000-000000000000', '10000000-0000-0000-0000-000000000000'),
  ('12000000-0000-0000-0000-000000000000', '11000000-0000-0000-0000-000000000000'),
  ('13000000-0000-0000-0000-000000000000', '12000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- First times-table step (formerly step 2 = ×10, now step 6) requires Groups & Arrays bridge
INSERT INTO step_prerequisites (step_id, requires_step_id)
VALUES ('cc000000-0000-0000-0000-000000000000', '13000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 2. Update step_unlocks: ×10 → Division  (remove old ×5/×3/×7 links)
-- ────────────────────────────────────────────────────────────
-- Remove old step-unlock entries for the retired Division I/II/III challenges
DELETE FROM step_unlocks
WHERE unlocks_challenge_id IN (
  'b6000000-0000-0000-0000-000000000000',  -- was Division I
  'be000000-0000-0000-0000-000000000000',  -- Division II (retired)
  'bf000000-0000-0000-0000-000000000000'   -- Division III (retired)
);

-- ×10 step now unlocks the single Division challenge
INSERT INTO step_unlocks (step_id, unlocks_challenge_id)
VALUES ('cc000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 3. Consolidate Division into a single challenge
--    Rename the challenge and update / re-number steps
-- ────────────────────────────────────────────────────────────
UPDATE challenges SET name = 'Division', description = 'Connect division to equal sharing; steps unlock in step with the Multiplication times tables you have mastered', slug = 'division'
WHERE id = 'b6000000-0000-0000-0000-000000000000';

-- Rename existing Division I steps and assign new step numbers
-- Old: step 1 = Sharing Equally, step 2 = Divide by 2, step 3 = Divide by 5, step 4 = Divide by 10
UPDATE steps SET step_number = 2, name = 'Divide by 10' WHERE id = 'd3000000-0000-0000-0000-000000000000';
UPDATE steps SET step_number = 3, name = 'Divide by 2'  WHERE id = 'd1000000-0000-0000-0000-000000000000';
UPDATE steps SET step_number = 4, name = 'Divide by 5'  WHERE id = 'd2000000-0000-0000-0000-000000000000';

-- Move Division II steps into Division challenge (was challenge be, now b6)
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 5,  name = 'Divide by 4' WHERE id = 'f2000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 6,  name = 'Divide by 3' WHERE id = 'f1000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 8,  name = 'Divide by 6' WHERE id = 'f3000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 9,  name = 'Divide by 7' WHERE id = 'f4000000-0000-0000-0000-000000000000';

-- Move Division III steps into Division challenge (was challenge bf, now b6)
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 7,  name = 'Divide by 8'  WHERE id = 'f5000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 10, name = 'Divide by 9'  WHERE id = 'f6000000-0000-0000-0000-000000000000';
UPDATE steps SET challenge_id = 'b6000000-0000-0000-0000-000000000000', challenge_name = 'Division',
  step_number = 11, name = 'Mixed Division Facts (1–10)' WHERE id = 'f7000000-0000-0000-0000-000000000000';

-- Add cross-challenge prerequisites (each div step requires matching mult step)
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('d3000000-0000-0000-0000-000000000000', 'cf000000-0000-0000-0000-000000000000'),  -- ÷10 req Sharing Equally
  ('d3000000-0000-0000-0000-000000000000', 'cc000000-0000-0000-0000-000000000000'),  -- ÷10 req ×10
  ('d1000000-0000-0000-0000-000000000000', 'd3000000-0000-0000-0000-000000000000'),  -- ÷2 req ÷10
  ('d1000000-0000-0000-0000-000000000000', 'cd000000-0000-0000-0000-000000000000'),  -- ÷2 req ×2
  ('d2000000-0000-0000-0000-000000000000', 'd1000000-0000-0000-0000-000000000000'),  -- ÷5 req ÷2
  ('d2000000-0000-0000-0000-000000000000', 'ce000000-0000-0000-0000-000000000000'),  -- ÷5 req ×5
  ('f2000000-0000-0000-0000-000000000000', 'd2000000-0000-0000-0000-000000000000'),  -- ÷4 req ÷5
  ('f2000000-0000-0000-0000-000000000000', 'd4000000-0000-0000-0000-000000000000'),  -- ÷4 req ×4
  ('f1000000-0000-0000-0000-000000000000', 'f2000000-0000-0000-0000-000000000000'),  -- ÷3 req ÷4
  ('f1000000-0000-0000-0000-000000000000', 'd5000000-0000-0000-0000-000000000000'),  -- ÷3 req ×3
  ('f5000000-0000-0000-0000-000000000000', 'f1000000-0000-0000-0000-000000000000'),  -- ÷8 req ÷3
  ('f5000000-0000-0000-0000-000000000000', 'd8000000-0000-0000-0000-000000000000'),  -- ÷8 req ×8
  ('f3000000-0000-0000-0000-000000000000', 'f5000000-0000-0000-0000-000000000000'),  -- ÷6 req ÷8
  ('f3000000-0000-0000-0000-000000000000', 'd6000000-0000-0000-0000-000000000000'),  -- ÷6 req ×6
  ('f4000000-0000-0000-0000-000000000000', 'f3000000-0000-0000-0000-000000000000'),  -- ÷7 req ÷6
  ('f4000000-0000-0000-0000-000000000000', 'd7000000-0000-0000-0000-000000000000'),  -- ÷7 req ×7
  ('f6000000-0000-0000-0000-000000000000', 'f4000000-0000-0000-0000-000000000000'),  -- ÷9 req ÷7
  ('f6000000-0000-0000-0000-000000000000', 'd9000000-0000-0000-0000-000000000000'),  -- ÷9 req ×9
  ('f7000000-0000-0000-0000-000000000000', 'f6000000-0000-0000-0000-000000000000'),  -- mixed req ÷9
  ('f7000000-0000-0000-0000-000000000000', 'da000000-0000-0000-0000-000000000000')   -- mixed req mult mixed
ON CONFLICT DO NOTHING;

-- ────────────────────────────────────────────────────────────
-- 4. Retire Division II and Division III challenges
--    (steps already moved; soft-delete by marking inactive)
-- ────────────────────────────────────────────────────────────
UPDATE challenges SET active = false
WHERE id IN (
  'be000000-0000-0000-0000-000000000000',  -- Division II (retired)
  'bf000000-0000-0000-0000-000000000000'   -- Division III (retired)
);

-- ────────────────────────────────────────────────────────────
-- 5. Update Order of Operations prerequisite (was Division III → now Division)
-- ────────────────────────────────────────────────────────────
UPDATE challenge_prerequisites
SET required_challenge_id = 'b6000000-0000-0000-0000-000000000000'
WHERE required_challenge_id = 'bf000000-0000-0000-0000-000000000000';
