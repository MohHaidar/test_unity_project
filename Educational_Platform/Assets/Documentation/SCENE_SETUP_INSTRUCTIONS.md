# Scene Setup Instructions - AI-Powered Question Flow

## 📋 Overview
This document explains how to set up the Unity scene for the adaptive learning game using the refactored codebase.

**Key Point:** One scene handles everything: step selection, question generation, answer evaluation, and progression.

---

## 🎮 Scene Architecture

```
Canvas
├── Panel: Background
├── Question Area
│   ├── Text: Question
│   ├── Button Grid (4 option buttons)
│   └── Text: Feedback
├── Player Stats Panel
│   ├── Text: Player Name, Subject, Challenge
│   └── Text: Step Info (Streak, Mastery, Phase)
├── Status Panel
│   └── Text: Status Messages
└── Button Panel
    ├── Button: Next Question
    └── Button: Step Complete / Next Step
```

---

## 🛠️ Step-by-Step Setup

### 1. Create New Scene
- In Unity, create a new scene: `Assets/Scenes/GameScene.unity`
- Save it

### 2. Create Canvas
- Right-click in Hierarchy → UI → Canvas
- This becomes the parent for all UI elements

### 3. Create Text Elements (TextMeshPro)
Under Canvas, create these TextMeshPro elements:

#### a. Question Text
- Right-click Canvas → TextMeshPro → Text
- Name: `QuestionText`
- Position: Upper center
- Size: 600 x 200
- Font Size: 36
- Text: "What is 5 + 3?"
- Alignment: Center

#### b. Feedback Text
- Right-click Canvas → TextMeshPro → Text
- Name: `FeedbackText`
- Position: Below question
- Size: 600 x 100
- Font Size: 24
- Text: (empty initially)
- Alignment: Center

#### c. Player Stats Text
- Right-click Canvas → TextMeshPro → Text
- Name: `PlayerStatsText`
- Position: Top-left
- Size: 300 x 150
- Font Size: 20
- Text: (empty initially)

#### d. Step Info Text
- Right-click Canvas → TextMeshPro → Text
- Name: `StepInfoText`
- Position: Top-right
- Size: 400 x 200
- Font Size: 18
- Text: (empty initially)

#### e. Status Text
- Right-click Canvas → TextMeshPro → Text
- Name: `StatusText`
- Position: Bottom center
- Size: 800 x 80
- Font Size: 22
- Text: "Loading..."
- Alignment: Center

### 4. Create Option Buttons
Create 4 buttons in a grid (2x2) for answer options:

For each button:
- Right-click Canvas → Button - TextMeshPro
- Name: `OptionButton1`, `OptionButton2`, `OptionButton3`, `OptionButton4`
- Position them in a grid below the question
- Size: ~150 x 60 each
- Spacing: ~20 pixels between buttons

**Example Layout:**
```
[OptionButton1] [OptionButton2]
[OptionButton3] [OptionButton4]
```

### 5. Create Control Buttons
Create two buttons at the bottom:

#### a. Next Question Button
- Right-click Canvas → Button - TextMeshPro
- Name: `NextQuestionButton`
- Position: Bottom-left
- Size: 200 x 60
- Text: "Next Question"
- Initially: Hidden (uncheck `Enabled` or set alpha to 0)

#### b. Step Complete Button
- Right-click Canvas → Button - TextMeshPro
- Name: `StepCompleteButton`
- Position: Bottom-right
- Size: 200 x 60
- Text: "Next Step"
- Initially: Hidden

### 6. Create Core GameObject
- Right-click in Hierarchy (not under Canvas)
- Create Empty GameObject
- Name: `GameFlow`
- Position: (0, 0, 0)

### 7. Attach Scripts to GameFlow
Add these components to the GameFlow GameObject:

1. **QuestionFlowManager** (Inspector → Add Component)
   - Serialize Fields:
     - `playerId`: 1
     - `playerName`: "Player"
     - `nextQuestionButton`: (drag NextQuestionButton)
     - `stepCompleteButton`: (drag StepCompleteButton)
     - `playerStatsText`: (drag PlayerStatsText)
     - `stepInfoText`: (drag StepInfoText)
     - `statusText`: (drag StatusText)

2. **QuestionDisplay** (Inspector → Add Component)
   - Serialize Fields:
     - `questionText`: (drag QuestionText)
     - `optionButtons`: (drag all 4 buttons in order)
     - `feedbackText`: (drag FeedbackText)

3. **AnswerSubmitter** (Inspector → Add Component)
   - No serialize fields needed

---

## 📐 Hierarchy Structure (Final)

```
Canvas
├── QuestionText (TextMeshPro)
├── FeedbackText (TextMeshPro)
├── PlayerStatsText (TextMeshPro)
├── StepInfoText (TextMeshPro)
├── StatusText (TextMeshPro)
├── OptionButton1
├── OptionButton2
├── OptionButton3
├── OptionButton4
├── NextQuestionButton
└── StepCompleteButton

GameFlow (empty GameObject)
├── QuestionFlowManager (script)
├── QuestionDisplay (script)
└── AnswerSubmitter (script)
```

---

## 🎨 Recommended Visual Setup

### Colors
- **Question Background:** Light gray (0.9, 0.9, 0.9)
- **Option Buttons:** Light blue (0.7, 0.9, 1.0)
- **Correct Feedback:** Green (#00FF00)
- **Incorrect Feedback:** Red (#FF0000)
- **Status Text:** Dark gray (#333333)

### Fonts
- **Questions:** 36pt, Bold
- **Options:** 28pt, Regular
- **Stats/Step Info:** 18-20pt, Regular
- **Status:** 24pt, Regular

### Layout Tips
- Leave 50px margins from canvas edges
- 20px spacing between buttons
- Center everything for balanced look

---

## ✅ Verification Checklist

Before running the scene:

- [ ] Canvas exists with all UI elements
- [ ] QuestionText displays question content
- [ ] 4 OptionButtons are properly laid out
- [ ] FeedbackText shows correct/incorrect feedback
- [ ] PlayerStatsText shows player name, subject, challenge
- [ ] StepInfoText shows streak, mastery, phase
- [ ] StatusText shows current status
- [ ] NextQuestionButton is hidden initially
- [ ] StepCompleteButton is hidden initially
- [ ] GameFlow GameObject exists
- [ ] QuestionFlowManager is attached to GameFlow
- [ ] QuestionFlowManager has all serialize fields filled
- [ ] QuestionDisplay is attached to GameFlow
- [ ] QuestionDisplay has all serialize fields filled
- [ ] AnswerSubmitter is attached to GameFlow

---

## 🚀 Running the Scene

1. **Make sure Ollama is running:**
   ```bash
   ollama serve
   ```

2. **In Unity:**
   - Select the GameScene in the Project
   - Click Play
   - Wait for "Connecting to Ollama..." message

3. **You should see:**
   - Status: "Connecting to Ollama..."
   - First question generates and displays
   - 4 option buttons with text
   - Player stats in top-left
   - Step info in top-right

4. **Interact:**
   - Click an option button
   - See feedback (correct/incorrect)
   - Click "Next Question"
   - Loop continues until 5-streak is reached

---

## 🐛 Troubleshooting

### "ERROR: Ollama is not running"
- Solution: Start Ollama in a terminal: `ollama serve`

### Buttons don't appear
- Solution: Check Canvas Scaler settings (default is fine)
- Solution: Verify button images are assigned

### Questions don't generate
- Solution: Check console for errors
- Solution: Verify all serialize fields are filled
- Solution: Check Ollama is running and reachable

### "CSVManager is null" errors
- Solution: This is expected (old system). It's okay.
- The new system uses PlayerDataManager instead.

### Buttons don't respond
- Solution: Check EventSystem exists in scene (Canvas creates it)
- Solution: Verify buttons have Image component
- Solution: Check button interactable = true initially

---

## 📊 Scene Workflow

When you press Play:

1. **Start()** runs
   - Load player from CSV
   - Load challenge/steps
   - Initialize AI (Ollama)

2. **GameLoop()** starts
   - Get current step
   - While step not complete:
     - Generate question
     - Display question
     - Wait for answer
     - Evaluate answer
     - Update UI (streak, mastery)
     - Save to CSV
   - Show "Step Complete"
   - Advance to next step

3. **Repeat** until challenge is done

---

## 🎯 Key Properties to Adjust

In `QuestionFlowManager` (Inspector):

- `playerId`: Change to test different players
- `playerName`: Set custom player name
- `nextQuestionButton`: Must be set for UI to work
- `stepCompleteButton`: Must be set for step progression

---

## 📝 Notes

- Scene auto-loads player data from CSV on start
- Player can close and reopen game - picks up where they left off
- Streak resets if student answers wrong (by design)
- Mastery increases/decreases by 0.03-0.05 per question
- Step is "complete" when both conditions met:
  1. 5-streak reached
  2. Ultimate challenge completed (if required)

---

## 🔮 Future Extensions

Once this is working, you can add:
- **ChallengeSelectUI**: Screen to pick Math/Addition/Step 2 etc
- **Ultimate Challenge**: Show free-form input after 5-streak
- **Animations**: Transition between questions
- **Sound Effects**: Correct/incorrect audio
- **Leaderboard**: Track best students
- **Progress UI**: Visual streak counter

For now, hardcoded to start at: **Math → Addition → Step 1**

Modify in `PlayerDataManager.LoadPlayer()` to change defaults.
