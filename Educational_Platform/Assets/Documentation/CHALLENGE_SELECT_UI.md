# 🎮 Challenge Selection UI - Step by Step Setup Guide

## Overview
This screen lets the player pick **Subject → Challenge → Step** before entering the game.
Steps are color-coded:
- 🟢 **Green** = Completed
- 🟡 **Yellow** = Unlocked (can play)
- 🔴 **Red** = Locked (finish previous step first)

**Script:** `Assets/Scripts/UI/ChallengeSelectUI.cs`  
**Scene to create:** `ChallengeSelect.unity`

---

## 📋 What You'll Create

```
Canvas (1920x1080)
├── BackgroundPanel
├── TitleText
├── SubjectDropdown (TMP_Dropdown)
├── ChallengeDropdown (TMP_Dropdown)
├── StepsLabel
├── StepsScrollView
│   └── Viewport
│       └── Content  ← stepsContainer (step buttons go here)
└── BackButton

Prefabs/
└── StepButton_Prefab  ← Button with TMP child

ChallengeSelectController (root GameObject)
└── ChallengeSelectUI (script)
```

---

## 🚀 Step-by-Step Instructions

---

### **STEP 0: Add SupabaseClient to Scene (1 minute)**

This scene is the **entry point** — it must host the `SupabaseClient` component so Supabase is ready before any data is loaded.

1. In the Hierarchy, right-click → **Create Empty**
2. Name it: `Services`
3. Select `Services` → **Add Component** → search `SupabaseClient`
4. If `Assets/Resources/SupabaseConfig.asset` doesn't exist yet, create it:
   - Right-click `Assets/Resources` → **Create → Educational Platform → Supabase Config**
   - Fill in your `Project Url` and `Anon Key` from Supabase dashboard → Settings → API

> `SupabaseClient.Awake()` runs before `ChallengeSelectUI.Start()`, so the connection is ready when player data is requested.

---

### **STEP 1: Create New Scene (1 minute)**

1. In Project tab → `Assets/Scenes` folder
2. Right-click → **Create → Scene**
3. Name it: `ChallengeSelect`
4. Double-click to open it
5. **Ctrl+S** to save

---

### **STEP 2: Create Canvas (2 minutes)**

1. Hierarchy → Right-click → **UI → Canvas**
   - This auto-creates Canvas + EventSystem
2. Select **Canvas** → Inspector:
   - **Canvas Scaler:**
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Screen Match Mode: `Match Width Or Height`, value: `0.5`

---

### **STEP 3: Create Background Panel (1 minute)**

1. Right-click **Canvas** → **UI → Panel**
2. Name it: `BackgroundPanel`
3. Inspector → **Rect Transform:**
   - Anchor Preset: **Stretch / Stretch** (click anchor icon → hold Alt → bottom-right corner preset)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. **Image Component:**
   - Color: `(0.08, 0.08, 0.12, 1)` — dark navy background

---

### **STEP 4: Create Title Text (2 minutes)**

1. Right-click **Canvas** → **UI → Text - TextMeshPro**
2. Name it: `TitleText`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-60`
   - Width: `800`, Height: `80`
4. **TextMeshProUGUI:**
   - Text: `Select Your Challenge`
   - Font Size: `52`
   - Alignment: Center (horizontal + vertical)
   - Color: `(1, 1, 1, 1)` — white
   - Font Style: Bold

---

### **STEP 5: Create Subject Dropdown (3 minutes)**

1. Right-click **Canvas** → **UI → Dropdown - TextMeshPro**
2. Name it: `SubjectDropdown`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-180`
   - Width: `600`, Height: `70`
4. **TMP_Dropdown component:**
   - Caption Text font size: `32`
   - Leave options empty (populated by script at runtime)
5. Select the child **Label** (TextMeshPro):
   - Font Size: `32`
   - Text: `Subject` (placeholder)
   - Alignment: Left-Center
6. Style the dropdown background **Image**:
   - Color: `(0.2, 0.2, 0.3, 1)` — dark blue-grey

> 💡 **What does this do?** The script fills this with all subjects from ChallengeDataManager (Math, Physics, History).

---

### **STEP 6: Add Subject Label (1 minute)**

1. Right-click **Canvas** → **UI → Text - TextMeshPro**
2. Name it: `SubjectLabel`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-130`
   - Width: `600`, Height: `40`
4. **TextMeshProUGUI:**
   - Text: `Subject`
   - Font Size: `26`
   - Alignment: Left-Center
   - Color: `(0.8, 0.8, 0.8, 1)` — light grey

---

### **STEP 7: Create Challenge Dropdown (3 minutes)**

1. Right-click **Canvas** → **UI → Dropdown - TextMeshPro**
2. Name it: `ChallengeDropdown`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-330`
   - Width: `600`, Height: `70`
4. **TMP_Dropdown:**
   - Caption Text font size: `32`
   - Leave options empty (filled by script when subject changes)
5. **Image** color: `(0.2, 0.2, 0.3, 1)`

---

### **STEP 8: Add Challenge Label (1 minute)**

1. Right-click **Canvas** → **UI → Text - TextMeshPro**
2. Name it: `ChallengeLabel`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-275`
   - Width: `600`, Height: `40`
4. **TextMeshProUGUI:**
   - Text: `Challenge`
   - Font Size: `26`
   - Alignment: Left-Center
   - Color: `(0.8, 0.8, 0.8, 1)`

---

### **STEP 9: Add Steps Label (1 minute)**

1. Right-click **Canvas** → **UI → Text - TextMeshPro**
2. Name it: `StepsLabel`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-430`
   - Width: `600`, Height: `40`
4. **TextMeshProUGUI:**
   - Text: `Steps`
   - Font Size: `26`
   - Alignment: Left-Center
   - Color: `(0.8, 0.8, 0.8, 1)`

---

### **STEP 10: Create Steps Scroll View (4 minutes)**

This is the scrollable list where step buttons appear.

1. Right-click **Canvas** → **UI → Scroll View**
2. Name it: `StepsScrollView`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`, Pos Y: `-680`
   - Width: `620`, Height: `400`
4. **Scroll Rect component:**
   - Horizontal: ❌ unchecked
   - Vertical: ✅ checked
   - Scroll Sensitivity: `10`
5. **Image** (background): Color `(0.12, 0.12, 0.18, 1)` — dark panel

Now configure the **Content** child (the transform where buttons go):

1. In Hierarchy expand `StepsScrollView → Viewport → Content`
2. Select **Content**
3. **Rect Transform:**
   - Anchor: Top-Left (0, 1)
   - Pivot: (0.5, 1)
   - Width: `620`, Height: `200` (auto-grows)
4. Add Component → **Vertical Layout Group:**
   - Padding: Left `10`, Right `10`, Top `10`, Bottom `10`
   - Spacing: `8`
   - Child Alignment: Upper Center
   - Control Child Size: ✅ Width, ❌ Height
   - Use Child Scale: ❌ unchecked
5. Add Component → **Content Size Fitter:**
   - Vertical Fit: `Preferred Size`

> 💡 **This Content object is what you drag into the StepsContainer field** of the ChallengeSelectUI script.

---

### **STEP 11: Create StepButton Prefab (4 minutes)**

This is the reusable button template for each step.

1. Right-click **Canvas** → **UI → Button - TextMeshPro**
2. Name it: `StepButton_Template`
3. **Rect Transform:**
   - Width: `580`, Height: `70`
   *(The Vertical Layout Group in Content will size the width automatically)*
4. **Button component** → **Colors:**
   - Normal Color: `(1, 1, 1, 1)` — white (script will override per step status)
   - Highlighted Color: `(0.9, 0.9, 0.9, 1)`
   - Pressed Color: `(0.7, 0.7, 0.7, 1)`
5. **Image component:**
   - Color: `(1, 1, 1, 1)` — white (overridden by script)
6. Select **Text (TMP)** child:
   - Font Size: `26`
   - Alignment: Left-Center
   - Color: `(0.1, 0.1, 0.1, 1)` — dark text
   - Overflow: Overflow

**Save as Prefab:**
1. In Project → `Assets/Prefabs` (create folder if missing)
2. Drag `StepButton_Template` from Hierarchy → into `Assets/Prefabs`
3. Name it: `StepButton_Prefab`
4. Delete `StepButton_Template` from the Hierarchy (it's now saved as a prefab)

---

### **STEP 12: Create Back Button (2 minutes)**

1. Right-click **Canvas** → **UI → Button - TextMeshPro**
2. Name it: `BackButton`
3. **Rect Transform:**
   - Anchor Preset: **Bottom / Left**
   - Pos X: `120`, Pos Y: `60`
   - Width: `200`, Height: `60`
4. **Image:** Color `(0.3, 0.3, 0.4, 1)` — grey-blue
5. **Text (TMP)** child:
   - Text: `← Back`
   - Font Size: `28`
   - Alignment: Center
   - Color: White

---

### **STEP 13: Create ChallengeSelectController (3 minutes)**

1. Hierarchy → Right-click (**NOT under Canvas**) → **Create Empty**
2. Name it: `ChallengeSelectController`
3. Inspector → Transform: Position `(0, 0, 0)`

**Attach the script:**
1. Select `ChallengeSelectController`
2. Inspector → **Add Component** → search `ChallengeSelectUI` → click to add

**Wire Inspector fields:**

| Field | Drag From Hierarchy |
|---|---|
| Subject Dropdown | `SubjectDropdown` |
| Challenge Dropdown | `ChallengeDropdown` |
| Steps Container | `StepsScrollView → Viewport → Content` |
| Step Button Prefab | Drag `StepButton_Prefab` from Project/Prefabs |
| Back Button | `BackButton` |

---

### **STEP 14: Add ChallengeSelect Scene to Build Settings**

1. **File → Build Settings**
2. Click **Add Open Scenes** (adds ChallengeSelect)
3. Also ensure `GameScene` and `MainMenu` are in the list
4. Close Build Settings

---

## ✅ Verification Checklist

### Hierarchy Structure
- [ ] Canvas with all UI elements inside
- [ ] ChallengeSelectController at root (not inside Canvas)
- [ ] ChallengeSelectUI script attached to ChallengeSelectController

### Canvas Elements
- [ ] BackgroundPanel (dark navy, fills screen)
- [ ] TitleText at top-center
- [ ] SubjectLabel + SubjectDropdown
- [ ] ChallengeLabel + ChallengeDropdown
- [ ] StepsLabel above scroll view
- [ ] StepsScrollView with Content child
- [ ] Content has Vertical Layout Group + Content Size Fitter
- [ ] BackButton at bottom-left

### Inspector Fields (ChallengeSelectUI)
- [ ] Subject Dropdown: `SubjectDropdown`
- [ ] Challenge Dropdown: `ChallengeDropdown`
- [ ] Steps Container: `Content` (inside StepsScrollView/Viewport/Content)
- [ ] Step Button Prefab: `StepButton_Prefab` from Prefabs folder
- [ ] Back Button: `BackButton`

### Prefab
- [ ] `StepButton_Prefab` exists in `Assets/Prefabs`
- [ ] Has Button + Image + TextMeshProUGUI child

---

## 🎮 Testing

1. Press **Play**
2. **Expected:** SubjectDropdown shows `Math`, `Physics`, `History`
3. Select **Math** → ChallengeDropdown shows all 9 Math challenges (Addition through Systems of Equations)
4. Select **Addition** → Step buttons appear in the scroll view:
   - Step 1: Yellow (unlocked, no completed steps yet)
   - Steps 2–4: Red (locked)
5. Click **Step 1 (yellow)** → GameScene loads
6. Complete Step 1 (5-streak) → return to ChallengeSelect
7. Step 1 now shows **Green**, Step 2 shows **Yellow**

---

## 🔁 Returning to ChallengeSelect After Step Complete

To return to ChallengeSelect after a step is completed, add this to `QuestionFlowManager.cs` inside `OnStepCompleteButtonClicked`:

```csharp
private void OnStepCompleteButtonClicked()
{
    if (stepCompleteButton != null)
        stepCompleteButton.gameObject.SetActive(false);

    // Return to challenge selection
    UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeSelect");
}
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|---|---|
| Dropdowns are empty | ChallengeDataManager not initializing — check console |
| Step buttons don't appear | Confirm `Steps Container` field points to the `Content` object, not the ScrollView |
| Buttons are wrong size | Verify `Vertical Layout Group` has Control Child Size Width checked |
| Clicking step doesn't load scene | Ensure `GameScene` is in Build Settings (File → Build Settings) |
| All steps are red | Player has no completed steps yet — only step 1 should be yellow on first run |
| TMP import error | Window → TextMeshPro → Import TMP Essential Resources |

---

## 🎨 Color Reference

| Status | normalColor (RGBA) | Visual |
|---|---|---|
| Completed | `(0.2, 0.8, 0.2, 1)` | 🟢 Green |
| Unlocked | `(0.9, 0.8, 0.1, 1)` | 🟡 Yellow |
| Locked | `(0.8, 0.2, 0.2, 1)` | 🔴 Red |

---

## 📊 Final Scene Structure

```
Scene: ChallengeSelect
│
├── Canvas (1920x1080)
│   ├── BackgroundPanel (dark navy, fullscreen)
│   ├── TitleText (top-center, 52pt, white, bold)
│   ├── SubjectLabel (26pt, light grey)
│   ├── SubjectDropdown (TMP, 600x70)
│   ├── ChallengeLabel (26pt, light grey)
│   ├── ChallengeDropdown (TMP, 600x70)
│   ├── StepsLabel (26pt, light grey)
│   ├── StepsScrollView (620x400)
│   │   └── Viewport
│   │       └── Content  ← StepsContainer (VerticalLayoutGroup)
│   └── BackButton (bottom-left, 200x60)
│
└── ChallengeSelectController (root)
    └── ChallengeSelectUI (script)

Assets/Prefabs/
└── StepButton_Prefab (580x70, Button + TMP child)
```

