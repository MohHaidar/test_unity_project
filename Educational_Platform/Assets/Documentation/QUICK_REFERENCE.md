# ⚡ Quick Reference - UI Creation Essentials

## 📖 Two Core Documents

### 1. UI_LAYOUT_DIAGRAM.md (Read First - 5 min)
- Visual layout of the UI
- Rect Transform settings table
- Element positions and sizes
- Color and font reference
- Component hierarchy

📍 Location: `Assets/Documentation/UI_LAYOUT_DIAGRAM.md`

### 2. UI_CREATION_GUIDE.md (Follow Along - 15-20 min)
- 15 step-by-step instructions
- Complete configuration for each element
- Exact names and positions
- Script attachment and field assignment
- Verification checklist
- Troubleshooting

📍 Location: `Assets/Documentation/UI_CREATION_GUIDE.md`

---

## 🎨 What You're Creating

### GameScene UI Elements (14 total)
```
Canvas (parent)
├── QuestionText ..................... TextMeshPro
├── OptionButton1 .................... Button (top-left)
├── OptionButton2 .................... Button (top-right)
├── OptionButton3 .................... Button (bottom-left)
├── OptionButton4 .................... Button (bottom-right)
├── FeedbackText ..................... TextMeshPro
├── PlayerStatsText .................. TextMeshPro (shows Name, EXP, Coins, Completed Steps)
├── StepInfoText ..................... TextMeshPro (shows Streak, Mastery, Phase)
├── StatusText ....................... TextMeshPro
├── NextQuestionButton ............... Button (start hidden)
└── StepCompleteButton ............... Button (start hidden)

GameFlow (root)
├── QuestionFlowManager .............. Script
├── QuestionDisplay .................. Script
└── AnswerSubmitter .................. Script
```

### ChallengeSelect Scene Elements (new)
```
Canvas (parent)
├── BackgroundPanel .................. Image (dark navy)
├── TitleText ........................ TextMeshPro
├── SubjectLabel + SubjectDropdown ... TMP_Dropdown (auto-filled from ChallengeDataManager)
├── ChallengeLabel + ChallengeDropdown TMP_Dropdown (auto-filled on subject change)
├── StepsLabel ....................... TextMeshPro
├── StepsScrollView .................. Scroll View
│   └── Viewport/Content ............. stepsContainer (VerticalLayoutGroup)
└── BackButton ....................... Button

ChallengeSelectController (root)
└── ChallengeSelectUI ................ Script

Assets/Prefabs/
└── StepButton_Prefab ................ Button + TMP child (580x70)
```

### 3 Scripts to Attach (to GameFlow)
1. **QuestionFlowManager** - Main game loop
2. **QuestionDisplay** - Render questions
3. **AnswerSubmitter** - Get player input

---

## 🚀 Timeline

| Phase | Time | What |
|-------|------|------|
| Read Layout | 5 min | UI_LAYOUT_DIAGRAM.md |
| Create UI | 15-20 min | Follow UI_CREATION_GUIDE.md |
| Test Game | 5 min | Press Play in Unity |
| **Total** | **25-30 min** | **Playable game!** |

---

## ✅ Quick Checklist

### Before You Start
- [ ] Read UI_LAYOUT_DIAGRAM.md
- [ ] Have UI_CREATION_GUIDE.md open
- [ ] Ollama installed (`ollama serve` working)
- [ ] Unity project open

### While Creating (15 steps)
1. [ ] Create Scene: GameScene
2. [ ] Create Canvas
3. [ ] Create QuestionText
4. [ ] Create 4 OptionButtons
5. [ ] Create FeedbackText
6. [ ] Create PlayerStatsText
7. [ ] Create StepInfoText
8. [ ] Create StatusText
9. [ ] Create NextQuestionButton
10. [ ] Create StepCompleteButton
11. [ ] Create GameFlow GameObject
12. [ ] Attach QuestionFlowManager + fill fields
13. [ ] Attach QuestionDisplay + fill fields
14. [ ] Attach AnswerSubmitter
15. [ ] Verify all elements and scripts

### After Creation
- [ ] Press Play
- [ ] See "Connecting to Ollama..."
- [ ] See first question appear
- [ ] Click option button
- [ ] See feedback
- [ ] Test full game flow

---

## 🎯 Key Values (Copy-Paste Ready)

### Common Positions
```
Top-Left:     Position (-900, 480)
Top-Center:   Position (0, 100)
Top-Right:    Position (900, 480)
Center:       Position (0, 0)
Bottom-Left:  Position (-250, -850)
Bottom-Center: Position (0, -700)
Bottom-Right: Position (250, -850)
```

### Common Sizes
```
Question Area:      1000 x 300
Button (2x2 grid):  300 x 100
Feedback Area:      1000 x 150
Status Bar:         1500 x 100
Text Panels:        300-400 x 150-200
```

### Common Font Sizes
```
Question:      48pt
Options:       32pt
Feedback:      32pt
Status:        28pt
Buttons:       28pt
Stats/Step:    20-24pt
```

### Common Colors
```
Background:    (0.1, 0.1, 0.1, 1)   [Dark Gray]
Question:      (1, 1, 1, 1)         [White]
Options:       (0.7, 0.9, 1, 1)     [Light Blue]
Feedback OK:   (0, 1, 0, 1)         [Green]
Feedback Bad:  (1, 0, 0, 1)         [Red]
Status:        (1, 1, 0, 1)         [Yellow]
Button OK:     (0.5, 0.9, 0.5, 1)   [Light Green]
```

---

## 🔗 File Structure Reference

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Step.cs
│   │   ├── Challenge.cs
│   │   ├── Player.cs .............. + EXP, Coins, CompletedSteps, AddExp, AddCoins, MarkStepCompleted
│   │   ├── IQuestion.cs
│   │   ├── MultipleChoiceQuestion.cs
│   │   ├── ChallengeDataManager.cs
│   │   └── PlayerDataManager.cs ... + coins, total_exp, completed_steps_json columns
│   ├── AI/
│   │   ├── OllamaAPI.cs
│   │   ├── OllamaQuestionGenerator.cs  (retries up to 4x before fallback)
│   │   └── OllamaPerformanceEvaluator.cs
│   └── UI/
│       ├── QuestionFlow/
│       │   ├── QuestionDisplay.cs .. (options shuffled each question)
│       │   ├── QuestionFlowManager.cs (awards EXP/Coins, tracks CompletedSteps)
│       │   └── AnswerSubmitter.cs
│       └── ChallengeSelectUI.cs .... (NEW — subject/challenge/step selector)
│
├── Documentation/
│   ├── UI_LAYOUT_DIAGRAM.md ← Read first (GameScene layout)
│   ├── UI_CREATION_GUIDE.md ← GameScene step-by-step
│   ├── CHALLENGE_SELECT_UI.md ← NEW: ChallengeSelect scene step-by-step
│   ├── README.md
│   ├── QUICK_START.md
│   ├── CLEANUP_COMPLETE.md
│   ├── REFACTORING_COMPLETE.md
│   ├── SCENE_SETUP_INSTRUCTIONS.md
│   └── WHATS_READY.md
│
├── Prefabs/
│   └── StepButton_Prefab (create during ChallengeSelect setup)
│
└── Scenes/
    ├── ChallengeSelect.unity (create this first — entry point)
    └── GameScene.unity (create this — game loop)
```

---

## ⚡ Pro Tips

1. **Copy values from tables** - Exact Rect Transform values provided
2. **Follow the order** - Steps 1-15 must be in sequence
3. **Fill ALL fields** - Missing references cause errors
4. **Button names matter** - Scripts reference buttons by exact name
5. **Disable buttons initially** - NextQuestion and StepComplete start hidden
6. **Use Anchor Presets** - Simplifies positioning

---

## 🐛 Common Issues (Solutions)

| Issue | Solution |
|-------|----------|
| "Ollama not running" | Run `ollama serve` in terminal |
| No question appears | Check console for errors, verify fields filled |
| Buttons don't work | Verify EventSystem exists, button is Interactable |
| Text overlaps | Adjust Rect Transform positions/sizes |
| Script missing | Check script is in correct folder |
| Null reference error | Fill all serialized fields in Inspector |
| Question text blank | QuestionFlowManager not filling QuestionText field |

→ Full troubleshooting in **UI_CREATION_GUIDE.md**

---

## ✨ You're Ready!

**Next Action:**
1. Open: `Assets/Documentation/UI_LAYOUT_DIAGRAM.md` (5 min)
2. Open: `Assets/Documentation/UI_CREATION_GUIDE.md`
3. Follow steps 1-15 (15-20 min)
4. Press Play (5 min)
5. Enjoy your AI game! 🚀

**Total time: 25-30 minutes to playable game**

---

## 📞 Need Help?

- **Layout questions:** See UI_LAYOUT_DIAGRAM.md
- **Step-by-step help:** See UI_CREATION_GUIDE.md
- **Troubleshooting:** See UI_CREATION_GUIDE.md → Troubleshooting
- **General questions:** See README.md
- **Code questions:** See script comments (well-documented)

All answers are in the guides! ✓
