-- Migration: Add "Subtraction Fluency Within 10" step (fe) between step 2 and step 3 of Subtraction.
--
-- Current numbering after 20260316120000:
--   c5=1, c6=2, fc=3, fd=4, c9=5, ca=6
--
-- New numbering:
--   c5=1, c6=2, fe=3 (NEW), fc=4, fd=5, c9=6, ca=7
--
-- IMPORTANT: Renumber in reverse order (highest first) to avoid UNIQUE(challenge_id, number) conflicts.
-- IMPORTANT: Insert fe BEFORE updating fc's prerequisite FK.

-- 1. Shift existing steps up to make room for step 3 (fe)
UPDATE steps SET number = 7 WHERE id = 'ca000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 6 WHERE id = 'c9000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 5 WHERE id = 'fd000000-0000-0000-0000-000000000000';
UPDATE steps SET number = 4 WHERE id = 'fc000000-0000-0000-0000-000000000000';

-- 2. Insert the new step (must exist before any FK references to fe)
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, difficulty, require_ultimate)
VALUES (
    'fe000000-0000-0000-0000-000000000000',
    'b2000000-0000-0000-0000-000000000000',  -- Subtraction challenge
    3,
    'Subtraction Fluency Within 10',
    'Practise all subtraction facts within 0–10 for speed and accuracy across the full range of pairs',
    5,
    0.80,
    0.095,
    false
);

-- 3. Wire the new step's prerequisite (fe requires c6)
INSERT INTO step_prerequisites (step_id, requires_step_id)
VALUES ('fe000000-0000-0000-0000-000000000000', 'c6000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- 4. Update prerequisites: fc used to require c6; now it requires fe
UPDATE step_prerequisites
SET requires_step_id = 'fe000000-0000-0000-0000-000000000000'
WHERE step_id = 'fc000000-0000-0000-0000-000000000000'
  AND requires_step_id = 'c6000000-0000-0000-0000-000000000000';

-- 5. Update difficulty values to reflect the expanded ladder
UPDATE steps SET difficulty = 0.11 WHERE id = 'fc000000-0000-0000-0000-000000000000';
UPDATE steps SET difficulty = 0.12 WHERE id = 'fd000000-0000-0000-0000-000000000000';
UPDATE steps SET difficulty = 0.13 WHERE id = 'c9000000-0000-0000-0000-000000000000';
UPDATE steps SET difficulty = 0.14 WHERE id = 'ca000000-0000-0000-0000-000000000000';
