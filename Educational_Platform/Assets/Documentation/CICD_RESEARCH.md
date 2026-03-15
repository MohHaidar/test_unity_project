# CI/CD Pipeline Research — Unity Scene as Code

## Overview

Research findings on automating Unity scene setup via CI/CD, replacing manual Editor clicking with code-driven scene generation that syncs to all team members via Git.

---

## Concept: Scene as Code

Unity Editor Scripts (C#) replicate exactly what the UI setup instruction files describe, running headlessly in CI to generate `.unity` scene files that are committed back to the repo.

### Flow
```
Dev edits Editor script (e.g. adds a button)
        ↓
git push → GitHub Actions triggers
        ↓
GameCI spins up Unity in Docker
        ↓
Runs SceneBuilder.cs headlessly → generates .unity files
        ↓
GitHub Actions commits updated .unity files back to repo
        ↓
Other devs git pull → Unity auto-reloads scenes ✅
```

---

## Is it Infrastructure as Code?

Follows IaC **principles** (repeatable, version-controlled, automated) but is better described as **"Scene as Code"** — not a declarative state engine like Terraform. Terraform-style drift detection and rollback must be built manually.

---

## Limitations & Mitigations

| Limitation | Mitigation |
|------------|------------|
| Not idempotent by default | Check `GameObject.Find()` before creating objects |
| No drift detection | Serialize expected state to JSON, compare before applying |
| No rollback primitive | Git history on `.unity` files serves as rollback (`git checkout`) |
| One-directional (create only) | Write a scene validator script that diffs current vs spec |
| Manual edits invisible to script | Force Text serialization + Git diff catches changes |

---

## Local Sync

Unity's Asset Database watches the project folder. When `.unity` files change on disk after `git pull`, Unity **auto-reimports** them — no manual action needed.

**Critical requirement:** Set Asset Serialization to **Force Text**
```
Edit → Project Settings → Editor → Asset Serialization → Force Text
```
This makes `.unity` files human-readable YAML, enabling Git diffs and merges.
Verify in `ProjectSettings/EditorSettings.asset`: `m_SerializationMode: 2`

**Gotcha:** Scene must be closed (or Unity will prompt reload). Never pull with unsaved local scene edits.

---

## Toolchain

| Tool | Purpose | Cost |
|------|---------|------|
| **GitHub** | Repo + Actions runner | Free (2000 min/month public, 500 min/month private) |
| **GameCI** | Unity in Docker for headless CI | Free / Open Source |
| **Unity Personal License** | Required for GameCI auth | Free |

### GitHub Actions Example
```yaml
- uses: game-ci/unity-builder@v4
  with:
    unityVersion: 6000.1.2f1
    targetPlatform: Android
    buildMethod: SceneBuilder.BuildGameScene
  env:
    UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
    UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
    UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
```

---

## Cost Summary

**$0 required** with:
- Public GitHub repo → 2,000 free Actions minutes/month (~100–200 CI runs)
- Unity Personal license (free) stored as GitHub Secrets

Private repo reduces free minutes to 500/month (~25–50 runs). Upgrade to GitHub Pro ($4/month) for 3,000 minutes if needed.

---

## Recommended Repo Structure

```
Repo/
├── Assets/Scripts/Editor/       ← Scene builder scripts (C#, source of truth)
├── Assets/Scenes/               ← Generated .unity files (committed, Force Text)
├── ProjectSettings/             ← Tracked in git (serialization mode, build settings)
└── .github/workflows/           ← GitHub Actions pipeline
```

---

## Scalability

| Team Size | Verdict |
|-----------|---------|
| Solo / small team | ✅ Works well |
| Multiple scenes | ✅ Modular scripts per scene |
| Frequent UI changes | ⚠️ Scripts must be kept in sync with actual intent |
| Large team (10+ devs) | ❌ Consider Unity Prefabs + Addressables for declarative config |

---

## Next Steps (when ready to implement)

1. Create GitHub repo, set visibility (public recommended for free minutes)
2. Enable Force Text serialization in ProjectSettings
3. Create `Assets/Scripts/Editor/SceneBuilder.cs` from UI_CREATION_GUIDE.md and SCENE_SETUP_INSTRUCTIONS.md
4. Create `.github/workflows/build-scenes.yml` using GameCI
5. Store Unity credentials as GitHub Secrets
6. Update `.gitignore` — ignore `Library/`, `Temp/`; track `Scenes/`, `ProjectSettings/`
