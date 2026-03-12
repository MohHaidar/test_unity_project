# 🎨 UI Setup Instructions - Step by Step

## Overview
This guide walks you through creating the complete game UI in Unity. The scene will have all necessary elements for the adaptive learning system to function.

**Time:** 15-20 minutes  
**Result:** Fully functional game scene ready to play

---

## 📋 What You'll Create

```
Canvas (960x540 or full screen)
├── Background Panel
├── Question Area
│   ├── QuestionText (TextMeshPro)
│   ├── OptionButtons Grid (4 buttons in 2x2)
│   └── FeedbackText
├── Player Info Panel (Top-Left)
│   ├── PlayerStatsText
│   └── StepInfoText
├── Status Area (Bottom)
│   └── StatusText
└── Control Buttons (Bottom)
    ├── NextQuestionButton
    └── StepCompleteButton

GameFlow GameObject
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

**Expected:** Gray canvas area in the Scene view

---

### **STEP 3: Create Background Panel (1 minute)**

This is optional but makes the UI look better.

1. Right-click Canvas → UI → Panel - Image
2. Name it: `BackgroundPanel`
3. In Inspector:
   - **Rect Transform:**
     - Left: 0, Right: 0, Top: 0, Bottom: 0 (fills entire canvas)
   - **Image Component:**
     - Color: Light Gray `(0.1, 0.1, 0.1, 1)` or white `(1, 1, 1, 1)`

**Expected:** Canvas now has a solid background color

---

### **STEP 4: Create Question Text (2 minutes)**

1. Right-click Canvas → TextMeshPro → Text
2. Name it: `QuestionText`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=0, Y=100 (below top edge)
     - Width: 1000, Height: 300
     - Anchor Preset: Top Center
   - **TextMeshProUGUI Component:**
     - Text: "What is 5 + 3?"
     - Font Size: 48
     - Alignment: Center (both horizontal and vertical)
     - Color: White or Black (good contrast)

**Expected:** Question text appears at top-center of canvas

---

### **STEP 5: Create Option Buttons (5 minutes)**

Create 4 buttons in a 2x2 grid below the question.

**For EACH button (repeat 4 times):**

1. Right-click Canvas → Button - TextMeshPro
2. Name them in order: `OptionButton1`, `OptionButton2`, `OptionButton3`, `OptionButton4`
3. Configure each button's **Rect Transform:**
   - Position (approximate, adjust as needed):
     - Button 1: X=-250, Y=-200 (Top-Left of grid)
     - Button 2: X=250, Y=-200 (Top-Right of grid)
     - Button 3: X=-250, Y=-320 (Bottom-Left of grid)
     - Button 4: X=250, Y=-320 (Bottom-Right of grid)
   - Size: Width=300, Height=100
   - Anchor Preset: Center

4. Configure **Text (TextMeshProUGUI)** child of each button:
   - Text: "Option 1", "Option 2", "Option 3", "Option 4"
   - Font Size: 32
   - Alignment: Center
   - Color: Black

5. Configure **Image Component** of button:
   - Color: Light Blue `(0.7, 0.9, 1, 1)` or similar

**Expected:** 4 buttons arranged in a grid, each with text

**Layout visual:**
```
    [Button 1] [Button 2]
    
    [Button 3] [Button 4]
```

---

### **STEP 6: Create Feedback Text (2 minutes)**

1. Right-click Canvas → TextMeshPro → Text
2. Name it: `FeedbackText`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=0, Y=-450 (below buttons)
     - Width: 1000, Height: 150
     - Anchor Preset: Bottom Center
   - **TextMeshProUGUI Component:**
     - Text: (leave empty initially)
     - Font Size: 32
     - Alignment: Center
     - Color: Green (for correct) or Red (for incorrect)
       - Use White for now, it will change dynamically

**Expected:** Empty text area below buttons

---

### **STEP 7: Create Player Stats Text (2 minutes)**

1. Right-click Canvas → TextMeshPro → Text
2. Name it: `PlayerStatsText`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=-900, Y=480 (top-left)
     - Width: 300, Height: 150
     - Anchor Preset: Top-Left
   - **TextMeshProUGUI Component:**
     - Text: (leave empty, filled by script)
     - Font Size: 24
     - Alignment: Top-Left
     - Color: White

**Expected:** Text in top-left corner (empty initially)

---

### **STEP 8: Create Step Info Text (2 minutes)**

1. Right-click Canvas → TextMeshPro → Text
2. Name it: `StepInfoText`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=900, Y=480 (top-right)
     - Width: 400, Height: 200
     - Anchor Preset: Top-Right
   - **TextMeshProUGUI Component:**
     - Text: (leave empty, filled by script)
     - Font Size: 22
     - Alignment: Top-Right
     - Color: White

**Expected:** Text in top-right corner (empty initially)

---

### **STEP 9: Create Status Text (2 minutes)**

1. Right-click Canvas → TextMeshPro → Text
2. Name it: `StatusText`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=0, Y=-700 (very bottom)
     - Width: 1500, Height: 100
     - Anchor Preset: Bottom Stretch
   - **TextMeshProUGUI Component:**
     - Text: "Connecting to Ollama..."
     - Font Size: 28
     - Alignment: Center
     - Color: Yellow or White
     - Bold: Yes

**Expected:** Status message at bottom center

---

### **STEP 10: Create Next Question Button (2 minutes)**

1. Right-click Canvas → Button - TextMeshPro
2. Name it: `NextQuestionButton`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=-250, Y=-850 (bottom-left)
     - Size: Width=300, Height=80
     - Anchor Preset: Bottom-Left
   - **Button Component:**
     - Navigation: Automatic
   - **TextMeshProUGUI (child):**
     - Text: "Next Question"
     - Font Size: 28
     - Color: Black
   - **Image Component:**
     - Color: Green `(0.5, 0.9, 0.5, 1)` or light green
   - **CanvasGroup Component:** (Add if missing)
     - Set `Interactable: false` initially (we'll enable it dynamically)

4. IMPORTANT: Disable this button initially
   - Uncheck `Enabled` in the Inspector
   - OR set the image alpha to 0

**Expected:** Button at bottom-left, hidden initially

---

### **STEP 11: Create Step Complete Button (2 minutes)**

1. Right-click Canvas → Button - TextMeshPro
2. Name it: `StepCompleteButton`
3. In Inspector:
   - **Rect Transform:**
     - Position: X=250, Y=-850 (bottom-right)
     - Size: Width=300, Height=80
     - Anchor Preset: Bottom-Right
   - **Button Component:**
     - Navigation: Automatic
   - **TextMeshProUGUI (child):**
     - Text: "Next Step"
     - Font Size: 28
     - Color: Black
   - **Image Component:**
     - Color: Gold `(1, 0.84, 0, 1)` or light gold
   - **CanvasGroup Component:** (Add if missing)

4. IMPORTANT: Disable this button initially
   - Uncheck `Enabled` in the Inspector

**Expected:** Button at bottom-right, hidden initially

---

### **STEP 12: Create GameFlow GameObject (2 minutes)**

This holds all the game logic scripts.

1. Right-click in Hierarchy (NOT under Canvas) → Create Empty
2. Name it: `GameFlow`
3. In Inspector:
   - Position: X=0, Y=0, Z=0
   - No components yet (will add next)

**Expected:** Empty GameObject in scene root

---

### **STEP 13: Attach QuestionFlowManager Script (3 minutes)**

1. Select `GameFlow` in Hierarchy
2. Inspector → Add Component → QuestionFlowManager
3. Configure Serialized Fields:
   - **Player ID:** 1
   - **Player Name:** "Player"
   - **Next Question Button:** Drag `NextQuestionButton` from Hierarchy
   - **Step Complete Button:** Drag `StepCompleteButton` from Hierarchy
   - **Player Stats Text:** Drag `PlayerStatsText` from Hierarchy
   - **Step Info Text:** Drag `StepInfoText` from Hierarchy
   - **Status Text:** Drag `StatusText` from Hierarchy

**Expected:** All fields filled with correct references

---

### **STEP 14: Attach QuestionDisplay Script (2 minutes)**

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

### **STEP 15: Attach AnswerSubmitter Script (1 minute)**

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

### Canvas Elements
- [ ] QuestionText at top-center (shows question)
- [ ] 4 OptionButtons in 2x2 grid (below question)
- [ ] FeedbackText below buttons (for feedback)
- [ ] PlayerStatsText in top-left (player info)
- [ ] StepInfoText in top-right (step info)
- [ ] StatusText at bottom-center (status messages)
- [ ] NextQuestionButton at bottom-left (hidden initially)
- [ ] StepCompleteButton at bottom-right (hidden initially)

### Component Configuration
- [ ] QuestionFlowManager has all 5 button/text fields filled
- [ ] QuestionDisplay has question/feedback texts and 4 buttons filled
- [ ] AnswerSubmitter added (no config needed)
- [ ] All buttons disabled initially (NextQuestionButton, StepCompleteButton)

### Script Assignment
- [ ] No missing script references (would show as "missing" in red)
- [ ] No null reference exceptions in console

---

## 🎮 First Test Run

1. Make sure Ollama is running:
   ```bash
   ollama serve
   ```

2. In Unity, press **Play** (or Ctrl+P)

3. Watch the Status text:
   - **"Connecting to Ollama..."** (2-3 seconds)
   - **"Generating question..."** (3-4 seconds)
   - Then you should see a question appear

4. Click an Option button

5. You should see:
   - Feedback message (correct/incorrect)
   - Status updates with streak count
   - "Next Question" button appears

6. Click "Next Question" to continue

7. After 5 correct answers in a row:
   - Status: "Step 1 Complete!"
   - "Next Step" button appears

---

## 🐛 Troubleshooting

### "ERROR: Ollama is not running"
- **Solution:** Open terminal, run `ollama serve`, keep it running

### No question appears (blank screen)
- **Solution:** Check console for errors
- **Solution:** Verify all QuestionFlowManager fields are filled (not null)
- **Solution:** Verify AnswerSubmitter is attached

### Buttons don't work
- **Solution:** Verify EventSystem exists (Canvas creates it)
- **Solution:** Check button is set to `Interactable: true`
- **Solution:** Verify Image component on button

### Text overlaps or looks bad
- **Solution:** Adjust Rect Transform positions and sizes
- **Solution:** Use Anchor Presets to position elements correctly
- **Solution:** Adjust Font Sizes to fit

### Script says "CSVManager is null"
- **Solution:** This is expected (old system)
- **Solution:** It's okay, new system uses PlayerDataManager
- **Solution:** Check console - may see other errors too

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
├── Canvas (960x540 or full screen)
│   ├── BackgroundPanel (light gray)
│   ├── QuestionText (top-center, 48pt, white)
│   ├── OptionButton1 (top-left of grid)
│   ├── OptionButton2 (top-right of grid)
│   ├── OptionButton3 (bottom-left of grid)
│   ├── OptionButton4 (bottom-right of grid)
│   ├── FeedbackText (below buttons)
│   ├── PlayerStatsText (top-left corner)
│   ├── StepInfoText (top-right corner)
│   ├── StatusText (bottom-center)
│   ├── NextQuestionButton (bottom-left, hidden)
│   └── StepCompleteButton (bottom-right, hidden)
│
└── GameFlow (root)
    ├── QuestionFlowManager (script)
    ├── QuestionDisplay (script)
    └── AnswerSubmitter (script)
```

---

## ✨ You're Done!

Your UI is complete and ready to test! 🎉

Press **Play** and you should see:
1. Status: "Connecting to Ollama..."
2. Status: "Generating question..."
3. A math question appears with 4 options
4. Click an option → see feedback
5. Click "Next Question" → next question
6. Repeat until 5-streak → step completes

---

## 🎯 Next Steps

After confirming it works:
- [ ] Test with several questions
- [ ] Verify streak increments on correct answers
- [ ] Verify step advances at 5-streak
- [ ] Test closing and reopening (resume feature)
- [ ] Close Ollama, re-open to test error handling

Enjoy your AI-powered learning system! 🚀
