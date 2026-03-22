-- ============================================================
--  Migration: challenge_step_prerequisites table
--
--  Replaces the step_unlocks (step→challenge) approach with
--  challenge_step_prerequisites (challenge←steps), which supports
--  AND semantics: ALL listed steps must be completed before the
--  challenge becomes available.
--
--  step_unlocks is kept in the DB but is no longer read by the game.
-- ============================================================

CREATE TABLE IF NOT EXISTS challenge_step_prerequisites (
    challenge_id     UUID NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    requires_step_id UUID NOT NULL REFERENCES steps(id)     ON DELETE CASCADE,
    PRIMARY KEY (challenge_id, requires_step_id)
);

-- Migrate existing step_unlocks data (inverted relationship)
-- step_unlocks had: step_id unlocks_challenge_id
-- New table has:    challenge_id requires_step_id
INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id)
SELECT unlocks_challenge_id, step_id
FROM step_unlocks
ON CONFLICT DO NOTHING;
