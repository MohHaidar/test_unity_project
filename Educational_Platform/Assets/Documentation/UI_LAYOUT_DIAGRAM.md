# 🎨 UI Layout Diagram - Visual Reference

## Canvas Layout (Full Screen or 960x540)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ┌──────────────────────┐              ┌──────────────────────────────┐│
│  │ PlayerStats:         │              │ Step Info:                   ││
│  │ Player Name          │              │ Step X: Description          ││
│  │ Subject: Math        │              │ Streak: 3/5                  ││
│  │ Challenge: Addition  │              │ Mastery: 0.75                ││
│  └──────────────────────┘              │ Phase: StreakBuilding        ││
│                                        └──────────────────────────────┘│
│                                                                         │
│                    ┌────────────────────────────────┐                  │
│                    │   What is 5 + 3?               │                  │
│                    │  (QuestionText, 48pt, centered)│                  │
│                    └────────────────────────────────┘                  │
│                                                                         │
│              ┌──────────────────┐  ┌──────────────────┐                │
│              │ [OptionButton1]  │  │ [OptionButton2]  │                │
│              │      "8"         │  │      "9"         │                │
│              └──────────────────┘  └──────────────────┘                │
│                                                                         │
│              ┌──────────────────┐  ┌──────────────────┐                │
│              │ [OptionButton3]  │  │ [OptionButton4]  │                │
│              │      "7"         │  │      "10"        │                │
│              └──────────────────┘  └──────────────────┘                │
│                                                                         │
│                    ┌────────────────────────────────┐                  │
│                    │  ✓ Correct! Addition is easy!  │                  │
│                    │ (FeedbackText, changes color)  │                  │
│                    └────────────────────────────────┘                  │
│                                                                         │
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │           Status: ✓ Correct! | Streak: 1/5 | Mastery: 0.76      │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│     ┌──────────────────────┐         ┌──────────────────────┐          │
│     │ [NextQuestionButton] │         │[StepCompleteButton] │          │
│     │   "Next Question"    │         │    "Next Step"      │          │
│     │   (shown when ready) │         │ (shown at 5-streak) │          │
│     └──────────────────────┘         └──────────────────────┘          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Canvas Rect Transform Settings Quick Reference

| Element | Position | Size | Anchor | Notes |
|---------|----------|------|--------|-------|
| BackgroundPanel | 0, 0 | Fill (0,0,0,0) | Stretch | Optional, fills entire canvas |
| QuestionText | 0, 100 | 1000 x 300 | Top Center | Above buttons |
| OptionButton1 | -250, -200 | 300 x 100 | Center | Top-left of 2x2 grid |
| OptionButton2 | 250, -200 | 300 x 100 | Center | Top-right of grid |
| OptionButton3 | -250, -320 | 300 x 100 | Center | Bottom-left of grid |
| OptionButton4 | 250, -320 | 300 x 100 | Center | Bottom-right of grid |
| FeedbackText | 0, -450 | 1000 x 150 | Bottom Center | Below buttons |
| PlayerStatsText | -900, 480 | 300 x 150 | Top-Left | Top-left corner |
| StepInfoText | 900, 480 | 400 x 200 | Top-Right | Top-right corner |
| StatusText | 0, -700 | 1500 x 100 | Bottom Stretch | Bottom bar |
| NextQuestionButton | -250, -850 | 300 x 80 | Bottom-Left | Start disabled |
| StepCompleteButton | 250, -850 | 300 x 80 | Bottom-Right | Start disabled |

---

## Component Hierarchy

```
GameScene
│
├── Canvas
│   ├── BackgroundPanel (Image)
│   ├── QuestionText (TextMeshProUGUI)
│   ├── OptionButton1 (Button)
│   │   └── Text (TextMeshProUGUI)
│   ├── OptionButton2 (Button)
│   │   └── Text (TextMeshProUGUI)
│   ├── OptionButton3 (Button)
│   │   └── Text (TextMeshProUGUI)
│   ├── OptionButton4 (Button)
│   │   └── Text (TextMeshProUGUI)
│   ├── FeedbackText (TextMeshProUGUI)
│   ├── PlayerStatsText (TextMeshProUGUI)
│   ├── StepInfoText (TextMeshProUGUI)
│   ├── StatusText (TextMeshProUGUI)
│   ├── NextQuestionButton (Button)
│   │   └── Text (TextMeshProUGUI)
│   └── StepCompleteButton (Button)
│       └── Text (TextMeshProUGUI)
│
└── GameFlow (empty GameObject)
    ├── QuestionFlowManager (MonoBehaviour)
    ├── QuestionDisplay (MonoBehaviour)
    └── AnswerSubmitter (MonoBehaviour)
```

---

## Data Flow Diagram

```
Game Start
    ↓
QuestionFlowManager.Start()
    ├─ Load Player from CSV
    ├─ Load Challenge definitions
    └─ Initialize Ollama connection
         ↓
    ┌─────────────────────────────────┐
    │ Main Game Loop                  │
    └─────────────────────────────────┘
         ↓
    Generate Question
    (Ollama creates question for step)
         ↓
    Display Question
    (QuestionDisplay renders on canvas)
         ↓
    Player Clicks Option Button
         ↓
    AnswerSubmitter captures answer
         ↓
    Evaluate Answer
    (Ollama evaluates + gives mastery delta)
         ↓
    Update Step Metrics
    (streak++, mastery += delta, save)
         ↓
    Show Feedback
    (FeedbackText displays result)
         ↓
    Check if Step Complete?
    (streak >= 5?)
    ├─ NO: Show "Next Question" button
    │   └─ Loop back to "Generate Question"
    │
    └─ YES: Show "Step Complete!"
        ├─ Show "Next Step" button
        └─ Wait for player click
           └─ Advance to next step
              └─ Loop back to top of game loop
```

---

## Color Reference

### Recommended Colors

| Element | Color | RGB | Hex | Notes |
|---------|-------|-----|-----|-------|
| Background | Dark Gray | (0.1, 0.1, 0.1, 1) | #1A1A1A | Professional look |
| Question Text | White | (1, 1, 1, 1) | #FFFFFF | High contrast |
| Option Buttons | Light Blue | (0.7, 0.9, 1, 1) | #B3E5FC | Friendly, clickable |
| Correct Feedback | Green | (0, 1, 0, 1) | #00FF00 | Success indicator |
| Incorrect Feedback | Red | (1, 0, 0, 1) | #FF0000 | Error indicator |
| Status Text | Yellow | (1, 1, 0, 1) | #FFFF00 | Attention grabber |
| Streak Counter | Gold | (1, 0.84, 0, 1) | #FFD700 | Achievement |
| Next Button | Green | (0.5, 0.9, 0.5, 1) | #80E680 | Positive action |
| Next Step Button | Gold | (1, 0.84, 0, 1) | #FFD700 | Special achievement |

---

## Font Size Reference

| Element | Size | Notes |
|---------|------|-------|
| Question | 48pt | Large, prominent |
| Option Buttons | 32pt | Easy to read |
| Feedback | 32pt | Clear feedback |
| Player Stats | 24pt | Secondary info |
| Step Info | 22pt | Secondary info |
| Status | 28pt | Important messages |
| Button Text | 28pt | Easy to read |

---

## Layout Grid (Using Anchor Presets)

```
┌─────────────────────────────┐
│ TL    TC    TR              │  TL = Top-Left
│                             │  TC = Top-Center
│                             │  TR = Top-Right
│  ────────────────────────   │
│  │                      │   │  ML = Middle-Left
│  │    CENTER            │   │  MC = Middle-Center
│  │                      │   │  MR = Middle-Right
│  ────────────────────────   │
│                             │  BL = Bottom-Left
│ BL    BC    BR              │  BC = Bottom-Center
└─────────────────────────────┘  BR = Bottom-Right
```

**Our Layout:**
- PlayerStatsText: **TL**
- StepInfoText: **TR**
- QuestionText: **TC** (upper)
- OptionButtons: **MC** (center)
- FeedbackText: **BC** (upper)
- StatusText: **BC** (middle)
- NextQuestionButton: **BL**
- StepCompleteButton: **BR**

---

## UI State Management

### During Question Display
```
☐ NextQuestionButton (HIDDEN)
☐ StepCompleteButton (HIDDEN)
✓ All option buttons VISIBLE and INTERACTABLE
✓ StatusText showing "Generating question..."
```

### After Answer Submitted
```
☐ NextQuestionButton (VISIBLE and INTERACTABLE)
☐ StepCompleteButton (HIDDEN)
✓ FeedbackText showing result (green/red)
✓ StatusText showing streak count
```

### At Step Complete
```
☐ NextQuestionButton (HIDDEN)
✓ StepCompleteButton (VISIBLE and INTERACTABLE)
✓ StatusText showing "Step X Complete!"
```

### Waiting for Next Step
```
☐ NextQuestionButton (HIDDEN)
✓ StepCompleteButton (VISIBLE, waiting for click)
```

---

## Quick Setup Checklist (Copy & Paste)

### Create these in order:
- [ ] Scene: GameScene
- [ ] Canvas (with CanvasScaler)
- [ ] BackgroundPanel (Image)
- [ ] QuestionText (TextMeshPro)
- [ ] OptionButton1-4 (Buttons with Text children)
- [ ] FeedbackText (TextMeshPro)
- [ ] PlayerStatsText (TextMeshPro)
- [ ] StepInfoText (TextMeshPro)
- [ ] StatusText (TextMeshPro)
- [ ] NextQuestionButton (Button with Text)
- [ ] StepCompleteButton (Button with Text)
- [ ] GameFlow (empty GameObject)

### Attach to GameFlow:
- [ ] QuestionFlowManager (fill fields)
- [ ] QuestionDisplay (fill fields)
- [ ] AnswerSubmitter (no fields)

### Verify:
- [ ] All buttons disabled initially
- [ ] All text fields properly named
- [ ] All references filled in scripts
- [ ] No missing script errors

---

## Camera Setup

If you have a Main Camera:
- **Canvas Render Mode:** Screen Space - Overlay
  - Best for UI-only scenes
  - No need to configure camera

OR

- **Canvas Render Mode:** Screen Space - Camera
  - Need to assign Main Camera
  - Adjust Camera clipping/position

Recommendation: Use **Screen Space - Overlay** for simplicity

---

## Testing the Layout

Before attaching scripts:

1. Create scene with all UI elements
2. Preview it
3. Check alignment and spacing look good
4. Then attach scripts and test functionality

This separates UI creation from script testing.

---

## Done! 🎉

You now have a complete layout diagram to reference while building your UI.

**Follow the UI_CREATION_GUIDE.md for step-by-step instructions.**

Next step: Create the scene in Unity!
