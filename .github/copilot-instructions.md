# Copilot Instructions for Educational Platform

## Project Overview

Educational Platform is a Unity-based educational gaming application (Unity 6000.1.2f1) delivering interactive learning activities with adaptive difficulty, performance tracking, and progression systems. The codebase has been refactored for stability and maintainability (see CLEANUP_REPORT.md for recent improvements).

## Build & Run

- **Open Project**: Open `Educational_Platform.sln` in Visual Studio or use Unity Hub with the `Educational_Platform` folder
- **Play in Editor**: Press Play in the Unity Editor (or Ctrl+P)
- **Build**: Use Unity's Build Settings menu (File > Build Settings)
- **Target Platform**: Android (configured as primary target)
- **Current Status**: ✅ Code compiles, ✅ Activities load, ⚠️ Answer validation in progress

## Architecture Overview

### Three-Layer Structure

1. **Core Layer** (`Assets/Scripts/Core/`)
   - **Data Models**: 
     - `Activity` — Tracks performance history, streak, current question, difficulty level
     - `Challenge` — Container for steps with cooldown tracking and MMR
     - `Step` — Manages activity progression with adaptive difficulty
     - `PlayerProfile` — Player stats, coins, level
   - **Question System**: 
     - `QuestionModule` (base) ← `Question` (extends) — Flexible structure supporting multiple question types via metadata
     - `QuestionPerformance` — Tracks individual answer success/timing
   - **Evaluation**: `Evaluation.cs` — Performance report generation (accuracy, time efficiency, streak stability)
   - **Data Access**: `CSVManager.cs` family (PlayerCSVManager, InventoryCSVManager, ShopCSVManager, etc.)
   - **Utilities**: `Utilities.cs` — MMRCalculator, ChallengeRewardCooldown, ActivityLoader (singleton)

2. **UI/Scene Layer** (`Assets/Scripts/UI/`)
   - **Base Architecture**: `BaseActivityManager` (abstract MonoBehaviour)
     - Handles activity lifecycle, streak display, answer submission flow
     - `LoadCurrentQuestion()` (virtual) — Override to display question type
     - `OnAnswerSubmitted()` (virtual) — Override for answer-specific logic
   - **Specific Activity Managers** (all extend BaseActivityManager):
     - `MultipleChoiceManager` — Button-based MCQ selection
     - `MultiSelectManager` — Toggle-based multiple answer selection
     - `TrueFalseMatrixManager` — True/False matrix (uses MatchPairs from Question)
     - `DragAndDropManager` — ⚠️ Placeholder DragHandler component needed
     - `SortOrderManager` — ⚠️ Order validation not fully implemented
     - `ConstructFromPartsManager` — Text input for constructed responses
   - **Scene Managers**:
     - `LoginManager` — Player authentication
     - `MainMenuManager` — Hub navigation
     - `ProfileManager` — Player stats display
     - `ShopManager` — Item purchasing (⚠️ incomplete)
     - `ChallengeManager` — Challenge selection (⚠️ incomplete)
   - **Navigation**: `SceneLoader` — Scene transition utility

3. **Scenes** (`Assets/Scenes/`)
   - `Login.unity` — Player authentication
   - `MainMenu.unity` — Hub for navigation
   - `ChallengeScene.unity` — Challenge selection and cooldown display
   - `Profile.unity` — Player stats and profile
   - `Shop.unity` — Item purchasing with currency
   - `Map.unity` — Subject/topic selection
   - `Subjects/` — Subject-specific scenes (Math, Physics)

### Data Flow

```
PlayerCSVManager (reads/writes Player Data)
    ↓
Activity.PerformanceHistory (List<QuestionPerformance>)
    ↓
Evaluation.GeneratePerformanceReport() → ActivityPerformance
    ↓
Step.NextActivity() (determines difficulty progression)
    ↓
BaseActivityManager.LoadCurrentQuestion() (displays via RenderQuestionUI)
```

### Key Design Patterns

#### Activity Lifecycle (After Recent Refactor)
```csharp
BaseActivityManager.Start()
  → currentActivity = ActivityLoader.SelectedActivity
  → ShowStartPanel()

BaseActivityManager.StartActivityLoop() [button click]
  → isActivityInProgress = true
  → LoadCurrentQuestion()

BaseActivityManager.Update() [every frame]
  → Check if PerformanceHistory.Count >= MAX_ATTEMPTS
  → If yes: HandleActivityCompleted()

OnAnswerSubmitted(answer) [user submits]
  → currentActivity.SubmitAnswer(answer)
  → LoadCurrentQuestion() [loads next question]
```

#### Question System
- Questions created via `Activity.QuestionGenerator` (delegate)
- `Question` inherits from `QuestionModule` for type consistency
- Flexible via metadata: MCQ (Options list), Matching (MatchPairs dict), Custom (Metadata dict)

#### CSV-Based Persistence
- **Files**: PlayerProfiles.csv, ChallengeCooldowns.csv, InventoryCSVManager, ShopItems.csv
- **Pattern**: Static manager classes (PlayerCSVManager, etc.) with simple comma-split parsing
- **⚠️ Fragile**: No header validation, no quoted-string handling, no concurrent write protection
- **Recommended**: Use proper CSV library in future refactors

### Dependencies

Key packages:
- **Rendering**: Universal Render Pipeline (URP) 17.1.0
- **UI**: UGUI 2.0.0, TextMesh Pro
- **Input**: New Input System 1.14.0
- **Editor Tools**: Visual Studio IDE integration, Rider support
- **Visual Scripting**: 1.9.6 (for non-code logic)

## Code Conventions

### Naming
- **Managers**: `[ActivityType]Manager.cs` (MultipleChoiceManager, DragAndDropManager, etc.)
- **Data Access**: `[DataType]CSVManager` (PlayerCSVManager, InventoryCSVManager, etc.)
- **Serialized Fields**: `camelCase` with `[SerializeField]`
- **Static Access**: `ActivityLoader.SelectedActivity`, `ChallengeLoader.SelectedChallenge`
- **Enums**: `Difficulty { Easy = 0, Medium = 1, Hard = 2 }`

### Code Style
- **C# Version**: 9.0+ features supported (.NET Standard 2.1)
- **MonoBehaviour Pattern**: Initialize references in Start(), use OnDestroy() for cleanup
- **Property Initialization**: Always initialize collections in constructors (prevents null refs)
- **Null Safety**: Use null coalescing (`??`) and null conditional (`?.`) operators

### Recent Refactoring Standards
1. All managers must initialize `Activity.PerformanceHistory` before use
2. Use `BaseActivityManager.OnAnswerSubmitted()` for answer processing (don't call SubmitAnswer directly)
3. Use `LoadCurrentQuestion()` virtual method instead of inline question loading
4. Check `isActivityInProgress` before loading questions

## Common Tasks

### Add a New Activity Type
1. Create `New[ActivityType]Manager.cs` extending `BaseActivityManager`
2. Override `RenderQuestionUI(Question q)` to display your question type
3. Register manager in activity loader (currently requires manual setup)
4. Create scene in `Assets/Scenes/Subjects/` and assign manager component
5. Test with sample questions from corresponding challenge file

### Debug Activity Execution
- Check `ActivityLoader.SelectedActivity` before starting scene
- Inspect `currentActivity.PerformanceHistory.Count` during activity
- Verify `currentActivity.QuestionGenerator != null` before loading questions
- Use Debug.Log in `OnAnswerSubmitted()` to trace answer flow

### Add New Question Type
1. Add structure to `Question.Metadata` dictionary (e.g., `Metadata["matching_pairs"] = ...`)
2. Update challenge's `QuestionGenerator` to populate metadata
3. Create new manager that reads metadata and displays appropriately
4. Or extend existing manager's `RenderQuestionUI()` to handle new metadata

### Persist Player Changes
```csharp
// In any manager:
currentPlayer.PlayerCoins += reward;
PlayerCSVManager.UpdatePlayerProfile(currentPlayer);
```

## Known Issues & Todos

### ⚠️ Incomplete Features
- **DragAndDropManager**: Placeholder DragHandler component reference (line 36) not implemented
- **DragAndDropManager**: Answer validation uses placeholder (line 57) — replace with actual drop zone matching
- **SortOrderManager**: Order validation not implemented — currently accepts any order
- **ChallengeManager**: `GenerateModuleTracker()` method undefined
- **ShopManager**: `GenerateShopItems()` method incomplete

### 🐛 CSV System Fragility
- No header row support; assumes data rows only
- Naive comma splitting fails with commas in data
- No encoding specification (potential unicode issues)
- Silent failures on write operations
- No concurrent access protection

### 📝 Code Quality
- Hardcoded scene names in scene loading (SceneManager.LoadScene("ChallengeScene"))
- Hardcoded CSV file paths throughout CSVManager classes
- Physics subject folder completely empty (no implementation)
- Geometry_Challenges folder empty (no implementation)

## Recommended MCP Servers

Configure these servers to enhance Copilot's capabilities:

- **Git** — Query commit history, understand refactoring changes, check diffs
- **Filesystem** — Navigate large Unity project structure efficiently
- **Brave Search** — Research Unity API docs, URP features, adaptive learning algorithms

