# 🎨 UI Setup Instructions - Step by Step

## Overview
This guide walks you through creating the complete game UI in Unity. The scene will have all necessary elements for the adaptive learning system to function.

**Time:** 20-25 minutes  
**Result:** Fully functional game scene ready to play

---

## 📋 What You'll Create

```
Canvas (1920x1080)
├── BackgroundPanel
├── QuestionText
├── OptionButton1
├── OptionButton2
├── OptionButton3
├── OptionButton4
├── FeedbackText
├── ExpPopText            ← NEW: shows "+7 EXP" briefly after each answer
├── PlayerStatsText       (shows Name, EXP, Coins, Completed Steps)
├── StepInfoText          (shows Streak, Mastery, Phase)
├── StatusText
├── BackButton            ← NEW: abandons step → returns to ChallengeSelect
├── NextQuestionButton    (hidden initially)
├── StepCompleteButton    (hidden initially, placed INSIDE StepCompletePanel)
└── StepCompletePanel     ← NEW: overlay shown when step finishes
    ├── StepCompleteText  ← NEW: shows rewards earned
    └── StepCompleteButton

GameFlow GameObject (root, NOT under Canvas)
├── QuestionFlowManager (script)
├── QuestionDisplay (script)
└── AnswerSubmitter (script)
```

---

## 🚀 Step-by-Step Instructions

### **STEP 1: Create New Scene (1 minute)**

1. In Project tab, navigate to `Assets` → create folder `Scenes` (if doesn't exist)
2. Right-click in `Scenes` folder → Create → Scene
3. Name it: `GameScene`
4. Double-click to open it
5. Save the scene: Ctrl+S

**Expected:** Empty scene with just default camera

---

### **STEP 2: Create Canvas (2 minutes)**

1. Right-click in Hierarchy → UI → Canvas
2. This creates Canvas + GraphicsRaycaster + EventSystem
3. Select Canvas and configure in Inspector:
   - **Canvas Scaler:**
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Screen Match Mode: `Match Width Or Height`, value `0.5`

**Expected:** Gray canvas area in the Scene view

---

### **STEP 3: Create Background Panel (1 minute)**

1. Right-click **Canvas** → UI → Panel
2. Name it: `BackgroundPanel`
3. **Rect Transform:**
   - Click the Anchor Preset box → hold **Alt** → click **Stretch / Stretch** (bottom-right corner of the preset grid)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
4. **Image Component:**
   - Color: `(0.08, 0.08, 0.12, 1)` — dark navy

> 💡 Holding **Alt** while clicking an Anchor Preset also resets the position/size to fill the parent automatically.

**Expected:** Canvas has a solid dark background

---

### **STEP 4: Create Question Text (2 minutes)**

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `QuestionText`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`
   - Pos Y: `-130`  *(130 px below the top edge → pivot lands at vertical center of the box)*
   - Width: `1400`
   - Height: `200`
4. **TextMeshProUGUI:**
   - Text: `What is 5 + 3?` (placeholder)
   - Font Size: `52`
   - Alignment: Center / Center
   - Color: `(1, 1, 1, 1)` — white
   - Overflow: **Overflow** (so long questions don't clip)

> 💡 With **Top / Center** anchor, Pos Y is the distance from the top edge to the pivot of the rect. A value of `-130` with height `200` means the box occupies Y from `-30` to `-230` below the top edge — well inside the canvas.

**Expected:** Question text at the top-center, spanning most of the screen width

---

### **STEP 5: Create Option Buttons (5 minutes)**

Create 4 buttons in a 2×2 grid. All use **Center** anchor so positions are relative to the canvas center (0, 0).

**For EACH button (repeat 4 times):**

1. Right-click **Canvas** → UI → Button - TextMeshPro
2. Name them: `OptionButton1`, `OptionButton2`, `OptionButton3`, `OptionButton4`
3. **Rect Transform** — Anchor Preset: **Center / Center**, then set:

   | Button | Pos X | Pos Y | Width | Height |
   |--------|-------|-------|-------|--------|
   | OptionButton1 | `-310` | `110` | `560` | `110` |
   | OptionButton2 | `310` | `110` | `560` | `110` |
   | OptionButton3 | `-310` | `-30` | `560` | `110` |
   | OptionButton4 | `310` | `-30` | `560` | `110` |

   > Column gap (between columns): 620 − 560 = **60 px**  
   > Row gap (between rows): 140 − 110 = **30 px**

4. **TextMeshProUGUI** child of each button:
   - Text: `A`, `B`, `C`, `D` (placeholder — script fills this at runtime)
   - Font Size: `34`
   - Alignment: Center / Center
   - Color: `(0.05, 0.05, 0.1, 1)` — near-black
   - Overflow: **Overflow**

5. **Image Component:**
   - Color: `(0.2, 0.45, 0.8, 1)` — medium blue

**Layout visual (on 1920×1080):**
```
        [-310, 110] OptionButton1     [310, 110] OptionButton2

        [-310, -30] OptionButton3     [310, -30] OptionButton4
```

**Expected:** 4 evenly-spaced buttons centered on screen, below the question area

---

### **STEP 6: Create Feedback Text (2 minutes)**

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `FeedbackText`
3. **Rect Transform:**
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `-200`  *(below the bottom row of buttons)*
   - Width: `1400`
   - Height: `90`
4. **TextMeshProUGUI:**
   - Text: *(leave empty — filled by script)*
   - Font Size: `32`
   - Alignment: Center / Center
   - Color: `(1, 1, 1, 1)` — white (script changes to green/red at runtime)
   - Overflow: **Overflow**

**Expected:** Empty text area below the button grid

---

### **STEP 7: Create Player Stats Text (2 minutes)**

Shows the player's name, EXP, Coins, and completed step count in the top-left corner.

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `PlayerStatsText`
3. **Rect Transform:**
   - Anchor Preset: **Top / Left**
   - Pos X: `200`  *(center of the box is 200 px from the left edge → left edge of box at 200−190 = 10 px from canvas left)*
   - Pos Y: `-120`  *(center of box is 120 px below the top → top of box at −120+110 = −10 px from top = 10 px inside canvas)*
   - Width: `380`
   - Height: `220`
4. **TextMeshProUGUI:**
   - Text: *(leave empty — filled by script)*
   - Font Size: `22`
   - Alignment: Top / Left
   - Color: `(1, 1, 1, 1)` — white

> 💡 With **Top / Left** anchor and default pivot (0.5, 0.5), Pos X is measured from the left edge to the **center** of the rect, and Pos Y is measured from the top edge downward (negative). This is why Pos X needs to be half the width + margin.

**Expected:** Player stats block tightly in the top-left corner

---

### **STEP 8: Create Step Info Text (2 minutes)**

Shows streak, mastery, and step phase in the top-right corner.

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `StepInfoText`
3. **Rect Transform:**
   - Anchor Preset: **Top / Right**
   - Pos X: `-200`  *(center of the box is 200 px from the right edge → right edge of box at −200+190 = −10 px from canvas right = 10 px inside canvas)*
   - Pos Y: `-120`
   - Width: `380`
   - Height: `220`
4. **TextMeshProUGUI:**
   - Text: *(leave empty — filled by script)*
   - Font Size: `22`
   - Alignment: Top / Right
   - Color: `(1, 1, 1, 1)` — white

**Expected:** Step info block in the top-right corner

---

### **STEP 9: Create Status Text (2 minutes)**

A full-width bar at the very bottom showing connection and evaluation status.

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `StatusText`
3. **Rect Transform:**
   - Click the Anchor Preset box → hold **Alt** → click **Bottom / Stretch** (bottom row, center-stretch column)
   - Left: `0`, Right: `0`
   - Bottom: `10`
   - Height: `64`
4. **TextMeshProUGUI:**
   - Text: `Connecting to Ollama...`
   - Font Size: `26`
   - Font Style: Bold
   - Alignment: Center / Center
   - Color: `(1, 0.9, 0.2, 1)` — warm yellow

> 💡 **Bottom / Stretch** sets the anchor to span the full width. You set **Left**, **Right** (margins), **Bottom** (distance from canvas bottom), and **Height** — not Pos X/Y.

**Expected:** Yellow status bar pinned to the bottom of the screen, full width

---

### **STEP 10: Create Next Question Button (2 minutes)**

1. Right-click **Canvas** → UI → Button - TextMeshPro
2. Name it: `NextQuestionButton`
3. **Rect Transform:**
   - Anchor Preset: **Bottom / Center**
   - Pos X: `-310`  *(left of center, mirroring one option button column)*
   - Pos Y: `110`  *(110 px above the bottom edge)*
   - Width: `540`
   - Height: `80`
4. **TextMeshProUGUI (child):**
   - Text: `Next Question →`
   - Font Size: `28`
   - Alignment: Center / Center
   - Color: `(0.05, 0.05, 0.1, 1)` — near-black
5. **Image Component:**
   - Color: `(0.2, 0.75, 0.3, 1)` — medium green

6. **Disable initially:**
   - Uncheck the checkbox next to the object name in the Inspector

> 💡 With **Bottom / Center** anchor and default pivot, Pos Y is the distance from the bottom edge to the **center** of the rect. `110` with height `80` means the button bottom is at 110−40 = **70 px above the canvas bottom** — just above the StatusText bar.

**Expected:** Green button left-of-center near the bottom, hidden initially

---

### **STEP 11: Create Step Complete Button (2 minutes)**

This button will live **inside the StepCompletePanel** (created in Step 14), but create it here first so you can reparent it later.

1. Right-click **Canvas** → UI → Button - TextMeshPro
2. Name it: `StepCompleteButton`
3. **Rect Transform:**
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `-200`  *(will be repositioned inside the panel in Step 14)*
   - Width: `400`
   - Height: `80`
4. **TextMeshProUGUI (child):**
   - Text: `Continue →`
   - Font Size: `28`
   - Alignment: Center / Center
   - Color: `(0.05, 0.05, 0.1, 1)` — near-black
5. **Image Component:**
   - Color: `(1, 0.84, 0, 1)` — gold

6. **Disable initially:**
   - Uncheck the checkbox next to the object name in the Inspector

**Expected:** Gold Continue button, hidden initially (will be reparented into the overlay panel in Step 14)

---

### **STEP 12: Create Back Button (2 minutes)**

This button lets the player abandon the current step and return to the Challenge Select screen at any time. Progress (streak) is lost but mastery is saved.

1. Right-click **Canvas** → UI → Button - TextMeshPro
2. Name it: `BackButton`
3. **Rect Transform:**
   - Anchor Preset: **Bottom / Left**
   - Pos X: `120` 
   - Pos Y: `120` 
   - Width: `180`
   - Height: `60`
4. **TextMeshProUGUI (child):**
   - Text: `← Back`
   - Font Size: `24`
   - Alignment: Center / Center
   - Color: White
5. **Image Component:**
   - Color: `(0.15, 0.15, 0.25, 0.9)` — dark navy, slightly transparent

6. Leave the button **always active** (do NOT disable it)

7. **⚠️ Important — Hierarchy order:**  
   In the Hierarchy, drag `BackButton` to be the **last child** inside Canvas (below all other elements).  
   Unity renders later siblings on top — this ensures the button appears above PlayerStatsText in the same corner.

> 💡 The button occupies **x=10–120px, y=5–55px** from the canvas corner. It sits in the top-left corner of PlayerStatsText but renders on top of it because of its hierarchy position.

**Expected:** Small dark back button in the top-left corner, always visible

---

### **STEP 13: Create EXP Pop Text (2 minutes)**

This label briefly appears after each answer showing how much EXP was earned (e.g. "+7 EXP").

1. Right-click **Canvas** → UI → Text - TextMeshPro
2. Name it: `ExpPopText`
3. **Rect Transform:**
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `-300`  *(center at 840px from top — below FeedbackText at 740px, above NextQuestionButton at 930px)*
   - Width: `300`
   - Height: `55`
4. **TextMeshProUGUI:**
   - Text: `+7 EXP` (placeholder — filled by script)
   - Font Size: `36`
   - Font Style: Bold
   - Alignment: Center / Center
   - Color: `(1, 0.9, 0, 1)` — bright yellow

5. **Disable the GameObject initially:**
   - Uncheck the checkbox next to the object name in the Inspector top

> 💡 This sits in a clear gap: FeedbackText bottom is at **785px** from top, ExpPopText top is at **812px**, NextQuestionButton top is at **930px** — 45px gap above and 118px gap below.

**Expected:** Hidden yellow label that pops up briefly after each answer, centred below the feedback

---

### **STEP 14: Create Step Complete Panel (4 minutes)**

This full-screen overlay appears when the player finishes a step, showing a congratulations message and the rewards earned.

**Create the Panel:**
1. Right-click **Canvas** → UI → Panel
2. Name it: `StepCompletePanel`
3. **Rect Transform:**
   - Click the Anchor Preset box → hold **Alt** → click **Stretch / Stretch** (bottom-right corner of the preset grid)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
4. **Image Component:**
   - Color: `(0, 0, 0, 0.88)` — dark semi-transparent overlay

**Create the Reward Text inside the Panel:**
1. Right-click `StepCompletePanel` → UI → Text - TextMeshPro
2. Name it: `StepCompleteText`
3. **Rect Transform** (child of StepCompletePanel, so positions are relative to panel center):
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `80`   *(80px above panel center → upper half of screen)*
   - Width: `900`
   - Height: `480`
4. **TextMeshProUGUI:**
   - Text: *(filled by script at runtime)*
   - Font Size: `44`
   - Alignment: Center / Center
   - Color: White
   - Overflow: **Overflow**

**Move StepCompleteButton inside the Panel:**
1. In the Hierarchy, drag `StepCompleteButton` onto `StepCompletePanel` to reparent it
2. With `StepCompleteButton` selected, set its **Rect Transform** (now relative to panel):
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `-210`  *(210px below panel center — below the reward text)*
   - Width: `360`
   - Height: `80`
3. Its text should already be `Continue →` (set in Step 11)

> 💡 StepCompletePanel covers the whole canvas, so Pos Y=−210 here means 210px below the canvas center (y=750px from top), which is in the lower half of the screen — clear of everything since the panel hides all other UI when visible.

**Disable the Panel initially:**
- Select `StepCompletePanel` → uncheck the checkbox next to its name in the Inspector

**Expected:** When a step is completed, this dark overlay covers the whole screen showing the step name, EXP and Coins earned, and a gold Continue button

---

### **STEP 15: Create GameFlow GameObject (2 minutes)**

This holds all the game logic scripts.

1. Right-click in Hierarchy (NOT under Canvas) → Create Empty
2. Name it: `GameFlow`
3. In Inspector:
   - Position: X=0, Y=0, Z=0

**Expected:** Empty GameObject in scene root

---

### **STEP 16: Attach QuestionFlowManager Script (3 minutes)**

1. Select `GameFlow` in Hierarchy
2. Inspector → Add Component → QuestionFlowManager
3. Configure ALL Serialized Fields:

| Field | Drag from Hierarchy |
|---|---|
| Player ID | `1` (type directly) |
| Player Name | `"Player"` (type directly) |
| Next Question Button | `NextQuestionButton` |
| Step Complete Button | `StepCompleteButton` (the one inside StepCompletePanel) |
| **Back Button** | `BackButton` ← NEW |
| Player Stats Text | `PlayerStatsText` |
| Step Info Text | `StepInfoText` |
| Status Text | `StatusText` |
| **Exp Pop Text** | `ExpPopText` ← NEW |
| **Step Complete Panel** | `StepCompletePanel` ← NEW |
| **Step Complete Text** | `StepCompleteText` ← NEW |

**Expected:** All 11 fields filled with correct references

---

### **STEP 17: Attach QuestionDisplay Script (2 minutes)**

1. Select `GameFlow` in Hierarchy
2. Inspector → Add Component → QuestionDisplay
3. Configure Serialized Fields:
   - **Question Text:** Drag `QuestionText` from Hierarchy
   - **Feedback Text:** Drag `FeedbackText` from Hierarchy
   - **Option Buttons:** 
     - Size: 4
     - Element 0: Drag `OptionButton1`
     - Element 1: Drag `OptionButton2`
     - Element 2: Drag `OptionButton3`
     - Element 3: Drag `OptionButton4`

**Expected:** All fields filled with correct references

---

### **STEP 18: Attach AnswerSubmitter Script (1 minute)**

1. Select `GameFlow` in Hierarchy
2. Inspector → Add Component → AnswerSubmitter
3. No serialized fields needed for this component

**Expected:** Component added to GameFlow

---

## ✅ Verification Checklist

Before pressing Play, verify:

### Hierarchy Structure
- [ ] Canvas exists with all UI elements
- [ ] GameFlow is in scene root (not under Canvas)
- [ ] GameFlow has 3 components: QuestionFlowManager, QuestionDisplay, AnswerSubmitter
- [ ] StepCompleteButton is **inside** StepCompletePanel

### Canvas Elements
- [ ] QuestionText at top-center (shows question)
- [ ] 4 OptionButtons in 2x2 grid (below question)
- [ ] FeedbackText below buttons
- [ ] ExpPopText near FeedbackText — **disabled initially** ← NEW
- [ ] PlayerStatsText in top-left (shows Name, EXP, Coins, Completed Steps)
- [ ] StepInfoText in top-right (shows Streak, Mastery, Phase)
- [ ] StatusText at bottom-center
- [ ] BackButton top-left corner — **always visible** ← NEW
- [ ] NextQuestionButton bottom-left — **disabled initially**
- [ ] StepCompletePanel — **disabled initially**, fullscreen overlay ← NEW
  - [ ] StepCompleteText inside panel
  - [ ] StepCompleteButton inside panel — **disabled initially**

### Component Configuration (QuestionFlowManager — 11 fields)
- [ ] Player ID: 1
- [ ] Player Name: "Player"
- [ ] Next Question Button → `NextQuestionButton`
- [ ] Step Complete Button → `StepCompleteButton`
- [ ] Back Button → `BackButton` ← NEW
- [ ] Player Stats Text → `PlayerStatsText`
- [ ] Step Info Text → `StepInfoText`
- [ ] Status Text → `StatusText`
- [ ] Exp Pop Text → `ExpPopText` ← NEW
- [ ] Step Complete Panel → `StepCompletePanel` ← NEW
- [ ] Step Complete Text → `StepCompleteText` ← NEW

### QuestionDisplay fields
- [ ] Question Text, Feedback Text, and 4 Option Buttons filled

### Script Assignment
- [ ] No missing script references (shown in red in Inspector)
- [ ] No null reference exceptions in console

---

## 🎮 First Test Run

1. Make sure Ollama is running:
   ```bash
   ollama serve
   ```

2. Press **Play** (Ctrl+P)

3. Watch the Status text:
   - **"Connecting to Ollama..."** → **"Generating question..."** → question appears

4. Click an option button → you should see:
   - Correct/Incorrect feedback in FeedbackText
   - **"+5 EXP"** (or "+7 EXP" if answered fast) pops up briefly in yellow ← NEW
   - Streak count updates in StepInfoText
   - EXP counter updates in PlayerStatsText ← NEW
   - "Next Question" button appears

5. Click **← Back** at any time → returns to ChallengeSelect, mastery is saved ← NEW

6. After 5 correct answers in a row:
   - **Step Complete overlay appears** showing the step name and rewards ← NEW
   - e.g. "🎉 Step Complete! +50 EXP +50 Coins"
   - Click **Continue →** to advance to the next step

---

## 🐛 Troubleshooting

| Issue | Solution |
|---|---|
| "ERROR: Ollama is not running" | Run `ollama serve` in terminal |
| No question appears | Check console errors; verify all QuestionFlowManager fields filled |
| Buttons don't work | Verify EventSystem exists; button must be Interactable |
| ExpPopText never shows | Confirm it is assigned in QuestionFlowManager Inspector field |
| StepCompletePanel doesn't appear | Confirm Panel is assigned in Inspector; check it starts disabled |
| Back button crashes | Confirm `ChallengeSelect` scene is in Build Settings |
| Text overlaps | Adjust Rect Transform positions/sizes |

---

## 🎨 Optional: Better Styling

Once it works, you can improve the look:

### Button Styling
- Add rounded corners using Border Radius
- Add hover effects (Button component → On Hover Color Change)
- Use gradient backgrounds

### Text Styling
- Use outline effect (TextMeshPro → Outline settings)
- Use shadow effect (TextMeshPro → Shadow settings)
- Use color gradients

### Layout
- Add spacing between elements
- Use Layout Groups (Horizontal/Vertical Layout Group)
- Add padding and margins

---

## 📊 Final Scene Structure

```
Scene: GameScene
│
├── Canvas (1920x1080)
│   ├── BackgroundPanel (dark, fullscreen)
│   ├── QuestionText (top-center, 48pt, white)
│   ├── OptionButton1 (grid top-left)
│   ├── OptionButton2 (grid top-right)
│   ├── OptionButton3 (grid bottom-left)
│   ├── OptionButton4 (grid bottom-right)
│   ├── FeedbackText (below buttons)
│   ├── ExpPopText (next to feedback, hidden) ← NEW
│   ├── PlayerStatsText (top-left, shows EXP/Coins)
│   ├── StepInfoText (top-right, shows Streak/Mastery)
│   ├── StatusText (bottom-center)
│   ├── BackButton (top-left corner, always visible) ← NEW
│   ├── NextQuestionButton (bottom-left, hidden)
│   └── StepCompletePanel (fullscreen overlay, hidden) ← NEW
│       ├── StepCompleteText (center, reward summary)
│       └── StepCompleteButton (center-bottom, "Continue →")
│
└── GameFlow (root)
    ├── QuestionFlowManager (script) — 11 inspector fields
    ├── QuestionDisplay (script)
    └── AnswerSubmitter (script)
```

---

## ✨ You're Done!

Your UI is complete and ready to test! 🎉

Press **Play** and you should see:
1. Status: "Connecting to Ollama..."
2. Status: "Generating question..."
3. A question appears with 4 shuffled options
4. Click an option → feedback + **"+N EXP"** pop
5. Click "Next Question" → next question
6. Use **← Back** to return to Challenge Select at any time
7. Reach 5-streak → **Step Complete overlay** with rewards

---

## 🔗 Related Guides

- **CHALLENGE_SELECT_UI.md** — Setup the subject/challenge/step selector screen
- **QUICK_REFERENCE.md** — All key values (positions, sizes, colors) at a glance
- **README.md** — Full architecture overview

---

## 🎯 Next Steps

After confirming it works:
- [ ] Test Back button — verify mastery is saved and you return to ChallengeSelect
- [ ] Complete a step — verify overlay shows with correct EXP/Coins
- [ ] Reopen scene after closing — verify EXP, Coins, and completed steps persist
- [ ] Test Ollama offline — verify retry logic shows fallback question

Enjoy your AI-powered learning system! 🚀
