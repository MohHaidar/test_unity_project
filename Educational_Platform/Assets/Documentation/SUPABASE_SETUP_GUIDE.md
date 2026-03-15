# Supabase Setup Guide — Educational Platform

## Overview

This guide walks you through creating a free Supabase account, setting up the database project, and connecting it to the Unity game. Supabase replaces the fragile CSV persistence system with a proper cloud database accessible from any device.

Schema is managed via **Supabase Migrations** — versioned SQL files tracked in Git. Every schema change is reproducible, diffable, and deployable via CI/CD. This is the IaC approach for database schema.

```
supabase/migrations/
├── 20260315_001_initial_schema.sql   ← creates all tables
├── 20260315_002_add_shop_items.sql   ← future changes go here
└── ...
```

**Time:** ~20 minutes  
**Cost:** Free (Supabase free tier + Supabase CLI)  
**Result:** Cloud database with version-controlled schema, ready for Unity integration

---

## Part 1 — Create Supabase Account & Project

### Step 1: Sign Up
1. Go to [https://supabase.com](https://supabase.com)
2. Click **Start your project** → **Sign Up**
3. Sign up with GitHub (recommended) or email

### Step 2: Create a New Project
1. Once logged in, click **New Project**
2. Fill in the form:
   - **Organization:** your personal org (auto-created)
   - **Project Name:** `educational-platform`
   - **Database Password:** create a strong password — **save this somewhere safe**
   - **Region:** pick the closest to your location
3. Click **Create new project**
4. Wait ~2 minutes while Supabase provisions your database

---

## Part 2 — Get Your API Credentials

These are the two values Unity needs to talk to your database.

### Step 3: Find Your Credentials
1. In your project dashboard, click **Settings** (gear icon, bottom left)
2. Click **API** in the left sidebar
3. You will see two important values:

| Value | Where to find it | Example format |
|-------|-----------------|----------------|
| **Project URL** | Under "Project URL" | `https://abcdefgh.supabase.co` |
| **Anon Public Key** | Under "Project API keys" → `anon public` | `eyJhbGciOi...` (long string) |

4. Copy and **save both values** — you will need them in Part 4

> ⚠️ The `anon public` key is safe to use in client apps (it respects Row Level Security).  
> Never use the `service_role` key in Unity — it bypasses all security.

---

## Part 3 — Install Supabase CLI

The CLI manages migrations locally and pushes them to your cloud project.

### Step 4: Install Supabase CLI

**Windows (via Scoop):**
```powershell
# Install Scoop if you don't have it
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
irm get.scoop.sh | iex

# Install Supabase CLI
scoop bucket add supabase https://github.com/supabase/scoop-bucket.git
scoop install supabase
```

**Windows (via direct download):**
Download the latest release from [github.com/supabase/cli/releases](https://github.com/supabase/cli/releases) → `supabase_windows_amd64.tar.gz`, extract and add to PATH.

**Verify installation:**
```powershell
supabase --version
```

### Step 5: Log In to Supabase CLI
```powershell
supabase login
```
This opens a browser — authorize with your Supabase account.

---

## Part 4 — Initialize Migrations in the Repo

### Step 6: Initialize Supabase in the Project
Run this from the **repository root** (not the Unity Assets folder):
```powershell
cd C:\Users\User\Documents\python_scripts\test_unity_project
supabase init
```
This creates:
```
supabase/
├── config.toml      ← project config
└── migrations/      ← all schema migrations live here
```

### Step 7: Link to Your Cloud Project
```powershell
supabase link --project-ref YOUR_PROJECT_REF
```
Find `YOUR_PROJECT_REF` in your Supabase dashboard URL:  
`https://supabase.com/dashboard/project/YOUR_PROJECT_REF`  
It's the short alphanumeric string after `/project/`.

### Step 8: Create the Initial Migration
```powershell
supabase migration new initial_schema
```
This creates `supabase/migrations/YYYYMMDDHHMMSS_initial_schema.sql`.

Open that file and paste the following schema:

```sql
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
```

### Step 9: Apply the Migration to Cloud
```powershell
supabase db push
```
You should see each migration listed as applied. Verify in the Supabase dashboard → **Table Editor** — all tables should now exist.

---

## Making Future Schema Changes

**Never edit old migration files.** Always create a new one:
```powershell
supabase migration new add_shop_items
# edit the new file
supabase db push
```

This keeps the full history of every schema change in Git — reviewable, reversible, and CI/CD-ready.

---

## Part 5 — CI/CD: Auto-Apply Migrations on Push

Add this to `.github/workflows/db-migrate.yml` so schema changes deploy automatically when merged to `main`:

```yaml
name: Apply DB Migrations

on:
  push:
    branches: [main]
    paths:
      - 'supabase/migrations/**'

jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: supabase/setup-cli@v1
        with:
          version: latest

      - name: Apply migrations
        run: supabase db push --project-ref ${{ secrets.SUPABASE_PROJECT_REF }}
        env:
          SUPABASE_ACCESS_TOKEN: ${{ secrets.SUPABASE_ACCESS_TOKEN }}
```

**Required GitHub Secrets** (Settings → Secrets → Actions):

| Secret | Where to find it |
|--------|-----------------|
| `SUPABASE_PROJECT_REF` | Dashboard URL after `/project/` |
| `SUPABASE_ACCESS_TOKEN` | supabase.com → Account → Access Tokens |

---

## Part 6 — Configure Unity

### Step 10: Create the Config File in Unity

The repo contains `SupabaseConfig.cs` (the class definition) but **not** `SupabaseConfig.asset` (your credentials — it is gitignored). Every developer must create their own asset file locally.

**In the Unity Editor:**
1. In the Project tab, navigate to `Assets/Resources/`
2. Right-click → **Create → Educational Platform → Supabase Config**
3. Unity creates `SupabaseConfig.asset` in that folder
4. With the file selected, fill in the two fields in the **Inspector**:
   - **Project Url:** `https://your-project-id.supabase.co`
   - **Anon Key:** `eyJhbGciOi...` (your anon public key)
5. Save (Ctrl+S)

> This file is loaded at runtime via `Resources.Load<SupabaseConfig>("SupabaseConfig")`.  
> It is gitignored — credentials never leave your machine.

### Step 11: Add to .gitignore
Make sure these lines exist in your `.gitignore`:
```
Assets/Resources/SupabaseConfig.asset
Assets/Resources/SupabaseConfig.asset.meta
```

---

## Part 7 — Verify the Connection

### Step 12: Test in Unity Editor
1. Press **Play** in the Unity Editor
2. Open the **Console** window
3. Look for:
   ```
   [SupabaseClient] Connected to Supabase: https://your-project-id.supabase.co
   [PlayerDataManager] Player loaded from Supabase: PlayerName
   ```
4. In Supabase dashboard → **Table Editor** → `players` table — your player should appear

### Step 13: Verify Data in Supabase Dashboard
1. Go to your Supabase project
2. Click **Table Editor** (left sidebar)
3. Click the `players` table
4. You should see a row with your player's name, subject, and step

---

## Troubleshooting

| Problem | Likely cause | Fix |
|---------|-------------|-----|
| `401 Unauthorized` | Wrong anon key | Re-copy from Settings → API |
| `Connection refused` | Wrong project URL | Re-copy from Settings → API |
| `relation does not exist` | Schema not applied | Run `supabase db push` |
| `supabase: command not found` | CLI not installed | Re-run Step 4 |
| Data not saving | Config file missing | Check SupabaseConfig.asset exists in Resources/ |
| Console shows CSV path | Old PlayerDataManager | Ensure new scripts are compiled |
| Migration already applied | Re-running old migration | Create a new migration file instead |

---

## Free Tier Limits (reference)

| Resource | Free Limit |
|----------|-----------|
| Database size | 500 MB |
| API requests | Unlimited |
| Monthly active users | 50,000 |
| Project pause (inactivity) | Paused after 1 week inactive (unpause is free, instant) |

> The free project **pauses after 1 week of inactivity** but resumes instantly when you send a request. No data is lost.

---

## Next Steps

Once connected:
- All player data saves to Supabase automatically
- Multiple devices share the same player profile
- You can view/edit all data in the Supabase Table Editor dashboard
- Query play metrics directly with SQL in the Supabase SQL Editor
