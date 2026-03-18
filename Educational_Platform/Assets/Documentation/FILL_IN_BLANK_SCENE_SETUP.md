# Fill-In-Blank UI — Scene Setup Instructions

## 📋 Overview

This guide adds the **FillInBlankPanel** to the existing **GameScene** so the game can display
typed-answer and (future) drag-and-drop questions alongside the existing multiple-choice UI.

**Pre-condition:** GameScene already has the objects from `UI_CREATION_GUIDE.md` (BackgroundPanel,
QuestionText, OptionButton1–4, FeedbackText, PlayerStatsText, StepInfoText, StatusText,
NextQuestionButton, StepCompletePanel, BackButton, GameFlow with its three scripts).

**What this guide adds:**
1. Wrap the existing MC elements in a `MultipleChoicePanel` container
2. Create a sibling `FillInBlankPanel` container with input fields, labels, and a submit button
3. Re-wire `QuestionDisplay` serialized fields in the Inspector

**Time:** ~25 minutes  
**Result:** GameScene ready to display both question types; the code switches panels automatically.

---

## 🗺️ Target Hierarchy (after this guide)

```
Canvas
├── BackgroundPanel
├── PlayerStatsText
├── StepInfoText
├── StatusText
├── BackButton
├── ExpPopText
├── MultipleChoicePanel          ← NEW wrapper around existing MC elements
│   ├── QuestionText             ← MOVED in (was direct Canvas child)
│   ├── OptionButton1            ← MOVED in
│   ├── OptionButton2            ← MOVED in
│   ├── OptionButton3            ← MOVED in
│   └── OptionButton4            ← MOVED in
├── FillInBlankPanel             ← NEW (inactive by default)
│   ├── FIB_QuestionText         ← NEW
│   ├── BlankRow_0               ← NEW
│   │   ├── FIB_Label_0          ← NEW
│   │   └── FIB_Input_0          ← NEW
│   ├── BlankRow_1               ← NEW
│   │   ├── FIB_Label_1          ← NEW
│   │   └── FIB_Input_1          ← NEW
│   ├── BlankRow_2               ← NEW
│   │   ├── FIB_Label_2          ← NEW
│   │   └── FIB_Input_2          ← NEW
│   ├── BlankRow_3               ← NEW
│   │   ├── FIB_Label_3          ← NEW
│   │   └── FIB_Input_3          ← NEW
│   └── FIB_SubmitButton         ← NEW
├── FeedbackText                 (stays as direct Canvas child — shared by both panels)
├── NextQuestionButton
└── StepCompletePanel
    ├── StepCompleteText
    └── StepCompleteButton
```

---

## 🛠️ Step-by-Step Instructions

---

### STEP 1 — Create the MultipleChoicePanel wrapper (3 minutes)

We need a container so `QuestionDisplay` can hide all MC elements at once.

1. Right-click **Canvas** → **UI → Panel**
2. Name it: `MultipleChoicePanel`
3. **Rect Transform:**
   - Click the Anchor Preset box → hold **Alt** → click **Stretch / Stretch** (bottom-right of the preset grid)
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
4. **Image Component:**
   - Color: `(0, 0, 0, 0)` — fully transparent (invisible logic container)
   - Uncheck **Raycast Target**

5. **Move existing children into MultipleChoicePanel:**
   In the Hierarchy, drag each of the following onto `MultipleChoicePanel`:
   - `QuestionText`
   - `OptionButton1`
   - `OptionButton2`
   - `OptionButton3`
   - `OptionButton4`

   > ⚠️ After reparenting, all Pos X / Pos Y values **stay the same** — `MultipleChoicePanel`
   > is full-screen with the same origin as Canvas, so no position adjustments are needed.

6. **Verify:** Press Play — MC questions must still display correctly before continuing.

---

### STEP 2 — Create the FillInBlankPanel container (2 minutes)

1. Right-click **Canvas** → **UI → Panel**
2. Name it: `FillInBlankPanel`
3. Drag it to be a sibling of `MultipleChoicePanel` (just below it in the Hierarchy)
4. **Rect Transform:**
   - Anchor Preset → hold **Alt** → click **Stretch / Stretch**
   - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
5. **Image Component:**
   - Color: `(0, 0, 0, 0)` — transparent
   - Uncheck **Raycast Target**
6. **Deactivate the GameObject:** Uncheck the checkbox next to the name `FillInBlankPanel` at the
   top of the Inspector. The panel starts hidden; the code activates it when a FIB question arrives.

---

### STEP 3 — Create FIB_QuestionText (2 minutes)

1. Right-click **FillInBlankPanel** → **UI → Text - TextMeshPro**
2. Name it: `FIB_QuestionText`
3. **Rect Transform:**
   - Anchor Preset: **Top / Center**
   - Pos X: `0`
   - Pos Y: `-130`
   - Width: `1400`
   - Height: `200`
4. **TextMeshProUGUI:**
   - Text: `What is 16 − 9?  _____` (placeholder)
   - Font Size: `52`
   - Alignment: Center / Center
   - Color: `(1, 1, 1, 1)` — white
   - Overflow: **Overflow**

> Identical position to MC `QuestionText` so the question area always appears in the same spot.

---

### STEP 4 — Create BlankRow_0 (label + input field) (5 minutes)

#### 4a. Row container

1. Right-click **FillInBlankPanel** → **Create Empty**
2. Name it: `BlankRow_0`
3. **Rect Transform:**
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `110`  *(same vertical slot as the top button row)*
   - Width: `700`
   - Height: `90`
4. Add Component → **Horizontal Layout Group**
   - Child Alignment: **Middle Center**
   - Control Child Size: Width ☑, Height ☑
   - Child Force Expand: Width ☐, Height ☐
   - Spacing: `16`
   - Padding: all `0`

#### 4b. FIB_Label_0

1. Right-click **BlankRow_0** → **UI → Text - TextMeshPro**
2. Name it: `FIB_Label_0`
3. **TextMeshProUGUI:**
   - Text: `answer =` (placeholder; code replaces at runtime)
   - Font Size: `30`
   - Alignment: Middle Right
   - Color: `(0.75, 0.85, 1, 1)` — `#BFD9FF` light blue-white
   - Overflow: **Overflow**
4. Add Component → **Layout Element**
   - Preferred Width: `160`
   - Min Width: `100`
   - Flexible Width: `0`

#### 4c. FIB_Input_0

1. Right-click **BlankRow_0** → **UI → Input Field - TextMeshPro**
2. Name it: `FIB_Input_0`
3. **TMP_InputField component:**
   - Content Type: **Integer Number**
   - Line Type: **Single Line**
   - Character Limit: `6`
   - Caret Blink Rate: `0.85`
   - Caret Width: `2`
   - Caret Color: `(1, 1, 1, 1)` — white
   - Selection Color: `(0.2, 0.5, 1, 0.5)` — semi-transparent blue
4. **Image component** (input background):
   - Color: `(0.15, 0.18, 0.28, 1)` — `#262E47` dark blue-grey
5. **Child → Text Area → Text (TextMeshProUGUI):**
   - Font Size: `34`
   - Alignment: Middle Center
   - Color: `(1, 1, 1, 1)` — white
6. **Child → Text Area → Placeholder (TextMeshProUGUI):**
   - Text: `?`
   - Font Style: Italic
   - Color: `(0.5, 0.5, 0.6, 0.8)` — grey
7. Add Component → **Layout Element**
   - Preferred Width: `220`
   - Min Width: `150`
   - Flexible Width: `0`

> **Mobile:** On each `FIB_Input_N`, set **Keyboard Type → Number Pad** for numeric-answer steps.
> For steps where the answer could be negative (algebra), use **Number and Punctuation**.

---

### STEP 5 — Create BlankRow_1, BlankRow_2, BlankRow_3

Repeat Step 4 three more times with only the **Pos Y** changing:

| Row | Pos Y | Notes |
|-----|-------|-------|
| `BlankRow_1` | `-30` | Same Y as the lower button row |
| `BlankRow_2` | `-170` | Below the button grid |
| `BlankRow_3` | `-310` | Rarely used; 4-blank steps only |

Name the children `FIB_Label_1/2/3` and `FIB_Input_1/2/3` accordingly. All other settings are
identical to row 0.

> Rows 1–3 start active inside the inactive panel. `QuestionDisplay.DisplayFillInBlank()` calls
> `SetActive(false)` on any row beyond the blank count, so there is no need to deactivate them
> manually — they will be hidden at runtime.

---

### STEP 6 — Create FIB_SubmitButton (3 minutes)

1. Right-click **FillInBlankPanel** → **UI → Button - TextMeshPro**
2. Name it: `FIB_SubmitButton`
3. **Rect Transform:**
   - Anchor Preset: **Center / Center**
   - Pos X: `0`
   - Pos Y: `-460`
   - Width: `420`
   - Height: `100`
4. **Image Component:**
   - Color: `(0.18, 0.68, 0.38, 1)` — `#2EAD61` green
5. **Button Component → Color Tint transitions:**

   | State | Color (R, G, B, A) | Hex |
   |-------|--------------------|-----|
   | Normal | (0.18, 0.68, 0.38, 1) | #2EAD61 |
   | Highlighted | (0.24, 0.80, 0.46, 1) | #3DCC75 |
   | Pressed | (0.12, 0.50, 0.28, 1) | #1E8047 |
   | Selected | (0.18, 0.68, 0.38, 1) | #2EAD61 |
   | Disabled | (0.4, 0.4, 0.4, 0.5) | #66666680 |
   | Fade Duration | 0.1 | — |

6. **Button → On Click ():** leave this list **empty** — the code wires it at runtime via
   `AddListener`. Any entry you add here will fire twice.

7. **Child TextMeshProUGUI:**
   - Text: `Check ✓`
   - Font Size: `36`
   - Font Style: **Bold**
   - Alignment: Center / Center
   - Color: `(0.05, 0.05, 0.1, 1)` — `#0D0D1A` near-black

---

### STEP 7 — Visual layout check

**Temporarily** activate `FillInBlankPanel` and deactivate `MultipleChoicePanel` in the Inspector
to preview the layout in Scene view:

```
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│  [PlayerStats TL]                           [StepInfo TR]            │
│                                                                      │
│              What is 16 − 9?  _____                                  │
│              (FIB_QuestionText, 52pt, Top/Center, Y=-130)            │
│                                                                      │
│         ┌─────────────────────────────────────┐                      │
│         │  answer =   [  ?  ]                 │  ← BlankRow_0 Y=110  │
│         └─────────────────────────────────────┘                      │
│                                                                      │
│         (BlankRow_1 Y=-30, active but hidden at runtime)             │
│         (BlankRow_2 Y=-170, same)                                    │
│         (BlankRow_3 Y=-310, same)                                    │
│                                                                      │
│                   ┌───────────────┐                                  │
│                   │   Check ✓     │  ← FIB_SubmitButton Y=-460       │
│                   └───────────────┘                                  │
│                                                                      │
│         [FeedbackText — direct Canvas child, unchanged]              │
│                                                                      │
│  [StatusText bottom bar]                                             │
└──────────────────────────────────────────────────────────────────────┘
```

After verifying the layout, **reactivate `MultipleChoicePanel`** and **deactivate
`FillInBlankPanel`** before saving.

---

### STEP 8 — Re-wire QuestionDisplay in the Inspector (5 minutes)

Select **GameFlow** → find the **QuestionDisplay** component.

The component now has three header groups. Wire every field:

#### Multiple Choice

| Inspector Field | Assign |
|----------------|--------|
| Multiple Choice Panel | `MultipleChoicePanel` |
| Question Text | `MultipleChoicePanel / QuestionText` |
| Option Buttons → Element 0 | `MultipleChoicePanel / OptionButton1` |
| Option Buttons → Element 1 | `MultipleChoicePanel / OptionButton2` |
| Option Buttons → Element 2 | `MultipleChoicePanel / OptionButton3` |
| Option Buttons → Element 3 | `MultipleChoicePanel / OptionButton4` |

#### Fill In Blank

| Inspector Field | Assign |
|----------------|--------|
| Fill In Blank Panel | `FillInBlankPanel` |
| Fib Question Text | `FillInBlankPanel / FIB_QuestionText` |
| Fib Input Fields → Element 0 | `FillInBlankPanel / BlankRow_0 / FIB_Input_0` |
| Fib Input Fields → Element 1 | `FillInBlankPanel / BlankRow_1 / FIB_Input_1` |
| Fib Input Fields → Element 2 | `FillInBlankPanel / BlankRow_2 / FIB_Input_2` |
| Fib Input Fields → Element 3 | `FillInBlankPanel / BlankRow_3 / FIB_Input_3` |
| Fib Input Labels → Element 0 | `FillInBlankPanel / BlankRow_0 / FIB_Label_0` |
| Fib Input Labels → Element 1 | `FillInBlankPanel / BlankRow_1 / FIB_Label_1` |
| Fib Input Labels → Element 2 | `FillInBlankPanel / BlankRow_2 / FIB_Label_2` |
| Fib Input Labels → Element 3 | `FillInBlankPanel / BlankRow_3 / FIB_Label_3` |
| Fib Submit Button | `FillInBlankPanel / FIB_SubmitButton` |

#### Shared

| Inspector Field | Assign |
|----------------|--------|
| Feedback Text | `FeedbackText` (direct Canvas child — unchanged) |

> **Array tip:** Click the lock icon 🔒 on the Inspector to keep GameFlow pinned.
> Set each array's **Size** to `4` before dragging elements in.

---

## 📐 Rect Transform Quick Reference

Reference resolution: **1920 × 1080** · Scale With Screen Size · Match Width Or Height `0.5`

| Object | Anchor | Pos X | Pos Y | Width | Height |
|--------|--------|------:|------:|------:|-------:|
| MultipleChoicePanel | Stretch/Stretch | — | — | fill | fill |
| FillInBlankPanel | Stretch/Stretch | — | — | fill | fill |
| FIB_QuestionText | Top/Center | 0 | -130 | 1400 | 200 |
| BlankRow_0 | Center/Center | 0 | 110 | 700 | 90 |
| BlankRow_1 | Center/Center | 0 | -30 | 700 | 90 |
| BlankRow_2 | Center/Center | 0 | -170 | 700 | 90 |
| BlankRow_3 | Center/Center | 0 | -310 | 700 | 90 |
| FIB_Label_N *(inside HLG row)* | HLG-managed | — | — | pref 160 | pref 80 |
| FIB_Input_N *(inside HLG row)* | HLG-managed | — | — | pref 220 | pref 80 |
| FIB_SubmitButton | Center/Center | 0 | -460 | 420 | 100 |

---

## 🎨 Color Reference

| Element | R, G, B, A | Hex |
|---------|-----------|-----|
| Panel containers | 0, 0, 0, 0 | transparent |
| FIB_QuestionText | 1, 1, 1, 1 | #FFFFFF |
| FIB_Label text | 0.75, 0.85, 1, 1 | #BFD9FF |
| FIB_Input background | 0.15, 0.18, 0.28, 1 | #262E47 |
| FIB_Input typed text | 1, 1, 1, 1 | #FFFFFF |
| FIB_Input placeholder | 0.5, 0.5, 0.6, 0.8 | #808099CC |
| FIB_Input caret | 1, 1, 1, 1 | #FFFFFF |
| FIB_Input selection | 0.2, 0.5, 1, 0.5 | #3380FF80 |
| Submit normal | 0.18, 0.68, 0.38, 1 | #2EAD61 |
| Submit highlighted | 0.24, 0.80, 0.46, 1 | #3DCC75 |
| Submit pressed | 0.12, 0.50, 0.28, 1 | #1E8047 |
| Submit disabled | 0.4, 0.4, 0.4, 0.5 | #66666680 |
| Submit label | 0.05, 0.05, 0.1, 1 | #0D0D1A |

---

## ✅ Verification Checklist

### Hierarchy
- [ ] `MultipleChoicePanel` exists under Canvas, transparent, stretch
- [ ] `QuestionText` and `OptionButton1–4` are inside `MultipleChoicePanel`
- [ ] `FillInBlankPanel` is a sibling of `MultipleChoicePanel`, inactive by default
- [ ] `FIB_QuestionText` is child of `FillInBlankPanel`
- [ ] `BlankRow_0–3` each contain exactly `FIB_Label_N` + `FIB_Input_N`
- [ ] Each `BlankRow` has a **Horizontal Layout Group**
- [ ] `FIB_SubmitButton` is child of `FillInBlankPanel`
- [ ] `FeedbackText` is still a direct Canvas child

### Inspector — QuestionDisplay on GameFlow
- [ ] Multiple Choice Panel → `MultipleChoicePanel`
- [ ] Question Text → `QuestionText` (inside MC panel)
- [ ] Option Buttons [0–3] → `OptionButton1–4`
- [ ] Fill In Blank Panel → `FillInBlankPanel`
- [ ] Fib Question Text → `FIB_QuestionText`
- [ ] Fib Input Fields [0–3] → `FIB_Input_0–3`
- [ ] Fib Input Labels [0–3] → `FIB_Label_0–3`
- [ ] Fib Submit Button → `FIB_SubmitButton`
- [ ] Feedback Text → `FeedbackText`
- [ ] **FIB_SubmitButton On Click () list is EMPTY** (code wires it at runtime)

### Runtime
- [ ] Addition Step 1 (MC) → `MultipleChoicePanel` shown, `FillInBlankPanel` hidden
- [ ] Subtraction Step 1 (FIB) → `FillInBlankPanel` shown, `MultipleChoicePanel` hidden
- [ ] Only `BlankRow_0` is active for single-blank questions
- [ ] Typing an answer and pressing **Check ✓** evaluates and shows feedback
- [ ] Systems of Equations step → `BlankRow_0` (x) and `BlankRow_1` (y) both active
- [ ] Pressing **Check ✓** with an empty field does not crash (it submits an empty string which evaluates as incorrect)

---

## 🐛 Troubleshooting

**MC questions no longer display**  
→ `Multiple Choice Panel` field on `QuestionDisplay` is empty. Assign `MultipleChoicePanel`.

**FIB panel never appears**  
→ `Fill In Blank Panel` field is empty, OR the step has `QuestionMode.Any`. Subtraction steps
use `FillInBlank`. Select Subtraction in ChallengeSelectUI to test.

**Input field is too narrow / gets squashed**  
→ Add a `Layout Element` to `FIB_Input_N` with **Min Width: 150**, **Preferred Width: 220**.

**Submit button fires twice**  
→ There is a pre-assigned entry in `FIB_SubmitButton → On Click ()`. Remove it.

**Positions shift after reparenting into MultipleChoicePanel**  
→ They should not change (same origin), but if they do: re-enter the same Pos X / Pos Y values
from the Quick Reference table.

**Keyboard doesn't open on device**  
→ Unity requires `TouchScreenKeyboard.isSupported`. On Android this works by default. In Editor,
type directly — the on-screen keyboard will appear on device.
