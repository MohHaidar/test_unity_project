-- Migration: Step-based challenge unlocks + single Multiplication challenge
-- Replaces Multiplication I, II, III with one unified challenge.
-- Division I, II, III are now unlocked by specific Multiplication steps.

-- ── 1. Create step_unlocks table ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS step_unlocks (
    step_id            uuid NOT NULL REFERENCES steps(id)      ON DELETE CASCADE,
    unlocks_challenge_id uuid NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    PRIMARY KEY (step_id, unlocks_challenge_id)
);

-- ── 2. Rename Multiplication I → Multiplication and add remaining steps ───
-- Update the Multiplication I challenge name/description
UPDATE challenges
SET name        = 'Multiplication',
    description = 'Master all single-digit times tables through carefully sequenced steps; intermediate steps unlock matching Division challenges'
WHERE id = 'b5000000-0000-0000-0000-000000000000';

-- Remove Multiplication II and III challenges (steps will be moved/reused)
DELETE FROM challenge_prerequisites WHERE challenge_id IN (
    'bc000000-0000-0000-0000-000000000000',  -- Mult II
    'bd000000-0000-0000-0000-000000000000'   -- Mult III
);
DELETE FROM steps WHERE challenge_id IN (
    'bc000000-0000-0000-0000-000000000000',
    'bd000000-0000-0000-0000-000000000000'
);
DELETE FROM challenges WHERE id IN (
    'bc000000-0000-0000-0000-000000000000',
    'bd000000-0000-0000-0000-000000000000'
);

-- ── 3. Reorder and rename existing Multiplication steps ───────────────────
-- Old: cb=Equal Groups, cc=×2, cd=×5, ce=×10
-- New: cb=Equal Groups, cc=×10, cd=×2, ce=×5 (reorder by difficulty)
UPDATE steps SET title='Multiply by 10', number=2, difficulty=0.14 WHERE id='cc000000-0000-0000-0000-000000000000';
UPDATE steps SET title='Multiply by 2',  number=3, difficulty=0.15 WHERE id='cd000000-0000-0000-0000-000000000000';
UPDATE steps SET title='Multiply by 5',  number=4, difficulty=0.16 WHERE id='ce000000-0000-0000-0000-000000000000';

-- Update step prerequisites to reflect new order
DELETE FROM step_prerequisites WHERE step_id IN (
    'cc000000-0000-0000-0000-000000000000',
    'cd000000-0000-0000-0000-000000000000',
    'ce000000-0000-0000-0000-000000000000'
);
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
    ('cc000000-0000-0000-0000-000000000000', 'cb000000-0000-0000-0000-000000000000'),  -- ×10 requires Equal Groups
    ('cd000000-0000-0000-0000-000000000000', 'cc000000-0000-0000-0000-000000000000'),  -- ×2 requires ×10
    ('ce000000-0000-0000-0000-000000000000', 'cd000000-0000-0000-0000-000000000000');  -- ×5 requires ×2

-- ── 4. Add steps 5–11 to the single Multiplication challenge ─────────────
INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, require_ultimate) VALUES
    ('ea000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000',  5, 'Multiply by 4', 5, 0.80, 0.18, false),
    ('eb000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000',  6, 'Multiply by 3', 5, 0.80, 0.20, false),
    ('ec000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000',  7, 'Multiply by 8', 5, 0.80, 0.23, false),
    ('ed000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000',  8, 'Multiply by 6', 5, 0.80, 0.25, false),
    ('ee000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000',  9, 'Multiply by 7', 5, 0.80, 0.27, false),
    ('ef000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 10, 'Multiply by 9', 5, 0.80, 0.28, false),
    ('f0000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 11, 'Mixed Times Tables (1–10)', 5, 0.80, 0.30, false)
ON CONFLICT (id) DO UPDATE
    SET challenge_id=EXCLUDED.challenge_id, number=EXCLUDED.number, title=EXCLUDED.title,
        streak_goal=EXCLUDED.streak_goal, mastery_target=EXCLUDED.mastery_target,
        difficulty=EXCLUDED.difficulty, require_ultimate=EXCLUDED.require_ultimate;

INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
    ('ea000000-0000-0000-0000-000000000000', 'ce000000-0000-0000-0000-000000000000'),  -- ×4 requires ×5
    ('eb000000-0000-0000-0000-000000000000', 'ea000000-0000-0000-0000-000000000000'),  -- ×3 requires ×4
    ('ec000000-0000-0000-0000-000000000000', 'eb000000-0000-0000-0000-000000000000'),  -- ×8 requires ×3
    ('ed000000-0000-0000-0000-000000000000', 'ec000000-0000-0000-0000-000000000000'),  -- ×6 requires ×8
    ('ee000000-0000-0000-0000-000000000000', 'ed000000-0000-0000-0000-000000000000'),  -- ×7 requires ×6
    ('ef000000-0000-0000-0000-000000000000', 'ee000000-0000-0000-0000-000000000000'),  -- ×9 requires ×7
    ('f0000000-0000-0000-0000-000000000000', 'ef000000-0000-0000-0000-000000000000')   -- Mixed requires ×9
ON CONFLICT DO NOTHING;

-- ── 5. Remove challenge prerequisites from Division challenges ────────────
-- Division I, II, III are now unlocked by steps, not by completing a whole challenge
DELETE FROM challenge_prerequisites WHERE challenge_id IN (
    'b6000000-0000-0000-0000-000000000000',  -- Division I
    'be000000-0000-0000-0000-000000000000',  -- Division II
    'bf000000-0000-0000-0000-000000000000'   -- Division III
);

-- Update Division I description
UPDATE challenges
SET description = 'Connect division to equal sharing; build fluency with ÷2, ÷5, ÷10 — unlocked after mastering ×5'
WHERE id = 'b6000000-0000-0000-0000-000000000000';

-- Update Division II description
UPDATE challenges
SET description = 'Divide by 3 and 4 using known times-table inverses — unlocked after mastering ×3'
WHERE id = 'be000000-0000-0000-0000-000000000000';

-- Update Division III description
UPDATE challenges
SET description = 'Divide by 6, 7, and 8, then fluently mix all division facts — unlocked after mastering ×7'
WHERE id = 'bf000000-0000-0000-0000-000000000000';

-- ── 6. Seed step_unlocks entries ──────────────────────────────────────────
INSERT INTO step_unlocks (step_id, unlocks_challenge_id) VALUES
    ('ce000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000'),  -- ×5 unlocks Division I
    ('eb000000-0000-0000-0000-000000000000', 'be000000-0000-0000-0000-000000000000'),  -- ×3 unlocks Division II
    ('ee000000-0000-0000-0000-000000000000', 'bf000000-0000-0000-0000-000000000000')   -- ×7 unlocks Division III
ON CONFLICT DO NOTHING;

-- ── 7. Keep Multiplication prerequisite on Subtraction (unchanged) ────────
-- challenge_prerequisites row: Multiplication requires Subtraction should already exist;
-- update just in case it points to an old Mult I entry.
INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id) VALUES
    ('b5000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000')  -- Multiplication requires Subtraction
ON CONFLICT DO NOTHING;
