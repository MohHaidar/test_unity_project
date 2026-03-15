-- Players table (replaces player_data.csv)
CREATE TABLE players (
    id          SERIAL PRIMARY KEY,
    name        TEXT NOT NULL UNIQUE,
    subject     TEXT NOT NULL DEFAULT 'Math',
    challenge   TEXT NOT NULL DEFAULT 'addition',
    step        INTEGER NOT NULL DEFAULT 1,
    streak      INTEGER NOT NULL DEFAULT 0,
    questions_count INTEGER NOT NULL DEFAULT 0,
    coins       INTEGER NOT NULL DEFAULT 0,
    total_exp   INTEGER NOT NULL DEFAULT 0,
    last_updated TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Per-step mastery tracking (replaces MasteryByStep dictionary)
-- Key format: "{subject}:{challenge}:{step}"
CREATE TABLE player_step_mastery (
    id          SERIAL PRIMARY KEY,
    player_id   INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    step_key    TEXT NOT NULL,
    mastery     FLOAT NOT NULL DEFAULT 0.0,
    UNIQUE(player_id, step_key)
);

-- Completed steps list (replaces CompletedSteps list)
CREATE TABLE player_completed_steps (
    id          SERIAL PRIMARY KEY,
    player_id   INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    step_key    TEXT NOT NULL,
    completed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(player_id, step_key)
);

-- Question history / play logs (replaces QuestionHistory list)
CREATE TABLE question_history (
    id              SERIAL PRIMARY KEY,
    player_id       INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    question_text   TEXT,
    student_answer  TEXT,
    correct_answer  TEXT,
    is_correct      BOOLEAN NOT NULL DEFAULT FALSE,
    time_taken_sec  FLOAT,
    difficulty      FLOAT,
    error_type      TEXT,
    answered_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Challenges catalog (replaces ChallengeDataManager CSV)
CREATE TABLE challenges (
    id          TEXT PRIMARY KEY,
    name        TEXT NOT NULL,
    description TEXT,
    subject     TEXT NOT NULL
);

-- Steps catalog (replaces step definitions)
CREATE TABLE steps (
    id                  TEXT PRIMARY KEY,
    number              INTEGER NOT NULL,
    description         TEXT,
    subject             TEXT NOT NULL,
    challenge_id        TEXT NOT NULL REFERENCES challenges(id) ON DELETE CASCADE,
    streak_goal         INTEGER NOT NULL DEFAULT 5,
    mastery_target      FLOAT NOT NULL DEFAULT 0.80,
    require_ultimate    BOOLEAN NOT NULL DEFAULT FALSE
);