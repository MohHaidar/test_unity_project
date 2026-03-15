-- Migration 002: Improved relational schema with UUID keys
-- Replaces the flat schema from migration 001 with a fully relational design.
-- Fixed UUIDs are used for seeded catalog data so hardcoded C# fallbacks stay in sync.

-- ── Drop old flat tables ────────────────────────────────────────────────────
DROP TABLE IF EXISTS player_completed_steps   CASCADE;
DROP TABLE IF EXISTS player_step_mastery      CASCADE;
DROP TABLE IF EXISTS question_history         CASCADE;
DROP TABLE IF EXISTS players                  CASCADE;
DROP TABLE IF EXISTS steps                    CASCADE;
DROP TABLE IF EXISTS challenges               CASCADE;

-- ══════════════════════════════════════════════════════════════════════════════
-- CATALOG TABLES  (seeded once, rarely change)
-- ══════════════════════════════════════════════════════════════════════════════

CREATE TABLE subjects (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        TEXT        NOT NULL UNIQUE,
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE challenges (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    subject_id  UUID        NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
    name        TEXT        NOT NULL,
    slug        TEXT        NOT NULL UNIQUE,   -- e.g. "addition" used for code lookups
    description TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Directed prerequisite graph (challenge X requires challenge Y completed first).
-- Supports cross-subject dependencies (e.g. Physics:Force requires Math:Addition).
CREATE TABLE challenge_prerequisites (
    challenge_id          UUID NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    requires_challenge_id UUID NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    PRIMARY KEY (challenge_id, requires_challenge_id),
    CHECK (challenge_id != requires_challenge_id)
);

CREATE TABLE steps (
    id               UUID    PRIMARY KEY DEFAULT gen_random_uuid(),
    challenge_id     UUID    NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    number           INTEGER NOT NULL,
    title            TEXT    NOT NULL,
    description      TEXT,
    streak_goal      INTEGER NOT NULL DEFAULT 5,
    mastery_target   FLOAT   NOT NULL DEFAULT 0.80,
    require_ultimate BOOLEAN NOT NULL DEFAULT FALSE,
    UNIQUE (challenge_id, number)
);

-- Step-level prerequisite graph.
-- Sequential within a challenge (step N requires step N-1) is seeded below.
-- Can also reference steps from other challenges.
CREATE TABLE step_prerequisites (
    step_id          UUID NOT NULL REFERENCES steps(id) ON DELETE CASCADE,
    requires_step_id UUID NOT NULL REFERENCES steps(id) ON DELETE CASCADE,
    PRIMARY KEY (step_id, requires_step_id),
    CHECK (step_id != requires_step_id)
);

-- ══════════════════════════════════════════════════════════════════════════════
-- PLAYER TABLES
-- ══════════════════════════════════════════════════════════════════════════════

CREATE TABLE players (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name            TEXT        NOT NULL UNIQUE,
    total_exp       INTEGER     NOT NULL DEFAULT 0,
    coins           INTEGER     NOT NULL DEFAULT 0,
    current_step_id UUID        REFERENCES steps(id),   -- navigation state
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_seen_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Per-step mastery and progress.
-- Written after every question answer — this is the resilience source of truth.
CREATE TABLE player_step_progress (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID        NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    step_id             UUID        NOT NULL REFERENCES steps(id)   ON DELETE CASCADE,
    mastery             FLOAT       NOT NULL DEFAULT 0.0,
    best_streak         INTEGER     NOT NULL DEFAULT 0,
    times_completed     INTEGER     NOT NULL DEFAULT 0,
    status              TEXT        NOT NULL DEFAULT 'not_started',  -- not_started | in_progress | completed
    first_completed_at  TIMESTAMPTZ,
    last_played_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (player_id, step_id)
);

-- Per-challenge completion status for lock/unlock and leaderboards.
CREATE TABLE player_challenge_progress (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id    UUID        NOT NULL REFERENCES players(id)    ON DELETE CASCADE,
    challenge_id UUID        NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    status       TEXT        NOT NULL DEFAULT 'locked',   -- locked | unlocked | in_progress | completed
    started_at   TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    UNIQUE (player_id, challenge_id)
);

-- Per-session aggregated log.
-- last_heartbeat_at updated every 30 s while session is active.
-- Orphan sessions (no heartbeat > 5 min, ended_at IS NULL) are recovered on next load.
CREATE TABLE play_sessions (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID        NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    step_id             UUID        NOT NULL REFERENCES steps(id)   ON DELETE CASCADE,
    started_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at            TIMESTAMPTZ,
    last_heartbeat_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    status              TEXT        NOT NULL DEFAULT 'active',   -- active | completed | abandoned | recovered
    questions_answered  INTEGER     NOT NULL DEFAULT 0,
    correct_answers     INTEGER     NOT NULL DEFAULT 0,
    max_streak_reached  INTEGER     NOT NULL DEFAULT 0,
    mastery_start       FLOAT       NOT NULL DEFAULT 0.0,
    mastery_end         FLOAT       NOT NULL DEFAULT 0.0,
    exp_earned          INTEGER     NOT NULL DEFAULT 0,
    coins_earned        INTEGER     NOT NULL DEFAULT 0,
    step_completed      BOOLEAN     NOT NULL DEFAULT FALSE
);

-- ══════════════════════════════════════════════════════════════════════════════
-- SEED CATALOG DATA
-- Fixed UUIDs match the hardcoded C# fallback constants in ChallengeDataManager.
-- ══════════════════════════════════════════════════════════════════════════════

INSERT INTO subjects (id, name, description) VALUES
  ('a1000000-0000-0000-0000-000000000000', 'Math',    'Mathematics from basics to advanced'),
  ('a2000000-0000-0000-0000-000000000000', 'Physics',  'Physics concepts and problem solving'),
  ('a3000000-0000-0000-0000-000000000000', 'History',  'Historical events and civilizations');

INSERT INTO challenges (id, subject_id, name, slug, description) VALUES
  ('b1000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Addition',       'addition',     'Learn addition from single digit to 2-digit numbers'),
  ('b2000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Subtraction',    'subtraction',  'Learn subtraction from single digit to 2-digit numbers'),
  ('b3000000-0000-0000-0000-000000000000', 'a2000000-0000-0000-0000-000000000000', 'Force and Motion','force',       'Newton''s laws and force concepts'),
  ('b4000000-0000-0000-0000-000000000000', 'a3000000-0000-0000-0000-000000000000', 'Ancient Rome',   'ancient_rome', 'The Roman Republic and Empire');

-- Cross-subject prerequisite: Physics:Force requires Math:Addition completed
INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id) VALUES
  ('b3000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000');

INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, require_ultimate) VALUES
  -- Addition (4 steps)
  ('c1000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 1, 'Single Digit (0–5)',     'Addition with numbers 0 through 5',          5, 0.80, FALSE),
  ('c2000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 2, 'Single Digit (6–9)',     'Addition with numbers 6 through 9',          5, 0.80, FALSE),
  ('c3000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 3, 'Two Digit No Carry',     'Two-digit addition without carrying',        5, 0.80, FALSE),
  ('c4000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 4, 'Two Digit With Carry',   'Two-digit addition with carrying',           5, 0.80, TRUE),
  -- Subtraction (2 steps)
  ('c5000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 1, 'Single Digit',           'Single-digit subtraction',                   5, 0.80, FALSE),
  ('c6000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 2, 'Two Digit',              'Two-digit subtraction',                      5, 0.80, FALSE),
  -- Force (1 step)
  ('c7000000-0000-0000-0000-000000000000', 'b3000000-0000-0000-0000-000000000000', 1, 'Newton''s First Law',    'The law of inertia',                         5, 0.80, FALSE),
  -- Ancient Rome (1 step)
  ('c8000000-0000-0000-0000-000000000000', 'b4000000-0000-0000-0000-000000000000', 1, 'Roman Republic',         'The founding and structure of the Roman Republic', 5, 0.80, FALSE);

-- Sequential step prerequisites within each challenge
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  -- Addition
  ('c2000000-0000-0000-0000-000000000000', 'c1000000-0000-0000-0000-000000000000'),
  ('c3000000-0000-0000-0000-000000000000', 'c2000000-0000-0000-0000-000000000000'),
  ('c4000000-0000-0000-0000-000000000000', 'c3000000-0000-0000-0000-000000000000'),
  -- Subtraction
  ('c6000000-0000-0000-0000-000000000000', 'c5000000-0000-0000-0000-000000000000');
