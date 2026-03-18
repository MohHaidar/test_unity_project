# Fill-In-Blank & Drag-And-Drop Question UI

This document describes how to wire the `FillInBlankPanel` in Unity's GameScene for both the **typed** (FillInBlank) and **drag-and-drop** modes.

---

## Overview

`QuestionDisplay` now manages two mutually exclusive panels:

| Panel | Active for | Key serialized fields |
|-------|-----------|----------------------|
| `MultipleChoicePanel` | `MultipleChoiceQuestion` | `questionText`, `optionButtons[4]` |
| `FillInBlankPanel` | `FillInBlankQuestion` (typed or drag) | `fibQuestionText`, `fibInputFields[4]`, `fibInputLabels[4]`, `fibSubmitButton` |

Only one panel is visible at a time; `QuestionDisplay.DisplayQuestion()` calls `SetPanelActive()` before rendering.

---

## GameScene Hierarchy Changes

```
Canvas
└── QuestionArea          (existing container)
    ├── MultipleChoicePanel       ← NEW: wrap existing MC elements in this GameObject
    │   ├── QuestionText          (TextMeshProUGUI) — existing
    │   ├── OptionButton_0        (Button + TMP child)
    │   ├── OptionButton_1
    │   ├── OptionButton_2
    │   └── OptionButton_3
    │
    └── FillInBlankPanel          ← NEW
        ├── FIB_QuestionText      (TextMeshProUGUI) — question with "_____ " blanks
        ├── BlankRow_0            (HorizontalLayoutGroup)
        │   ├── FIB_Label_0       (TextMeshProUGUI) — e.g. "answer =" or "x ="
        │   └── FIB_Input_0       (TMP_InputField)
        ├── BlankRow_1
        │   ├── FIB_Label_1
        │   └── FIB_Input_1
        ├── BlankRow_2
        │   ├── FIB_Label_2
        │   └── FIB_Input_2
        ├── BlankRow_3
        │   ├── FIB_Label_3
        │   └── FIB_Input_3
        └── FIB_SubmitButton      (Button + TMP child — label "Check")
```

> **Tip**: Set the `FillInBlankPanel` to `inactive` by default in the Inspector. `DisplayQuestion()` will activate it when needed.

---

## Inspector Wiring (QuestionDisplay component)

After restructuring the hierarchy, wire these fields on the `QuestionDisplay` component:

### Multiple Choice
| Field | Assign |
|-------|--------|
| `Multiple Choice Panel` | `MultipleChoicePanel` GameObject |
| `Question Text` | `QuestionText` (TextMeshProUGUI inside MultipleChoicePanel) |
| `Option Buttons[0–3]` | `OptionButton_0` … `OptionButton_3` |

### Fill In Blank
| Field | Assign |
|-------|--------|
| `Fill In Blank Panel` | `FillInBlankPanel` GameObject |
| `Fib Question Text` | `FIB_QuestionText` |
| `Fib Input Fields[0–3]` | `FIB_Input_0` … `FIB_Input_3` |
| `Fib Input Labels[0–3]` | `FIB_Label_0` … `FIB_Label_3` |
| `Fib Submit Button` | `FIB_SubmitButton` |

### Shared
| Field | Assign |
|-------|--------|
| `Feedback Text` | existing `FeedbackText` (TextMeshProUGUI, outside both panels) |

---

## TMP_InputField Setup (per FIB_Input_N)

- **Content Type**: Standard
- **Line Type**: Single Line
- **Character Limit**: 10
- **Font Size**: 28–32 (match option buttons)
- **Caret Color**: white or accent colour
- **Width**: ~120 px; height: ~50 px (match button height)

For mobile: set **Keyboard Type** to `NumberPad` on steps where answers are always numeric.

---

## Submit Button

- Label text: **"Check ✓"**
- Size: same height as option buttons, full width of FillInBlankPanel
- The button is disabled after the player submits (re-enabled on next question via `DisplayFillInBlank()`).

---

## Answer Encoding

`QuestionDisplay.OnFIBSubmit()` collects all active `TMP_InputField.text` values, trims whitespace, and joins them with `|` before calling `AnswerSubmitter.SubmitAnswer(answer)`.

`FillInBlankQuestion.CheckAnswer(string)` splits the incoming string on `|` and compares each part to `Blanks[i].CorrectAnswer` case-insensitively.

Single-blank steps produce answers like `"9"`.  
Multi-blank steps (e.g. Systems of Equations) produce `"3|5"`.

---

## Phase 2: Drag-And-Drop (planned)

When `FillInBlankQuestion.DragOptions.Count > 0`, the question type returns `"drag_and_drop"`. The planned UI for this mode:

```
FillInBlankPanel (drag variant)
├── FIB_QuestionText
├── DropZone_Row              (HorizontalLayoutGroup — one DropZone per blank)
│   ├── DropZone_0            (Image + DropZoneHandler script) — shows label + accepted token
│   └── DropZone_1
└── TokenTray                 (HorizontalLayoutGroup — draggable tokens)
    ├── Token_0               (DraggableToken script — TMP text child)
    ├── Token_1
    └── ...
```

**Not yet implemented in `QuestionDisplay`.**  
Current behaviour: a `DragAndDrop`-mode step falls back to the typed FillInBlank UI (tokens are ignored, player types). This is intentional — implement the drag UI as a separate visual layer once the typed path is proven.

---

## QuestionMode Assignment Rules

Steps are assigned a `QuestionMode` in `ChallengeDataManager.MakeStep(... mode: QuestionMode.FillInBlank)`:

| QuestionMode | Used for |
|---|---|
| `Any` (default) | Addition, Multiplication, Division, Order of Operations, Arithmetic Review — multiple choice works naturally |
| `FillInBlank` | All Subtraction steps, Expressions with Variables, One-Step Equations, Two-Step Equations, Systems of Equations — typed answer is more natural and pedagogically appropriate |
| `DragAndDrop` | Reserved for future steps where visual token matching adds value (fractions, factoring) |

`QuestionMode.Any` resolves to `MultipleChoice` in `OllamaQuestionGenerator.ResolveMode()`, preserving full backward compatibility.
