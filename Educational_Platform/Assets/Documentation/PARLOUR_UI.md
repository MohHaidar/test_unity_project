# Parlour UI Setup Guide

How to build the Conversation panel for parlour challenges in the GameScene.
Parlour questions use a **two-panel layout**: a character panel on top, the existing
Multiple Choice panel below. Both are active at the same time.

---

## What Gets Added

Parlour mode adds one new panel to the existing GameScene canvas:

| Panel | When active | Content |
|-------|------------|---------|
| `ConversationPanel` | Only for parlour challenges | Character portrait + name + dialogue bubble |
| `MultipleChoicePanel` | Always (for options + question prompt) | Reused as-is |

The `ConversationPanel` sits **above** the `MultipleChoicePanel` in the canvas hierarchy.
When a parlour step is playing, both panels are visible. For math/other steps only the
`MultipleChoicePanel` is visible.

---

## Target Layout

```
┌────────────────────────────────────────────────────────────┐
│  [PlayerStats TL]                    [StepInfo TR]         │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  ConversationPanel                                  │  │
│  │                                                     │  │
│  │  ┌──────────┐   ┌─────────────────────────────┐    │  │
│  │  │          │   │ Maya                        │    │  │
│  │  │  Portrait│   │─────────────────────────────│    │  │
│  │  │  (Image) │   │ "Hey! Great to see you      │    │  │
│  │  │          │   │  again. I was just thinking │    │  │
│  │  │          │   │  about what you said last   │    │  │
│  │  └──────────┘   │  time..."                   │    │  │
│  │                 └─────────────────────────────┘    │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  MultipleChoicePanel                                │  │
│  │                                                     │  │
│  │           How do you respond?                       │  │
│  │   (QuestionText — smaller than math questions)      │  │
│  │                                                     │  │
│  │    ┌──────────────────┐  ┌──────────────────┐      │  │
│  │    │  "That's great,  │  │  "Hey, whatever" │      │  │
│  │    │   thanks Maya!"  │  │                  │      │  │
│  │    └──────────────────┘  └──────────────────┘      │  │
│  │    ┌──────────────────┐                            │  │
│  │    │  "I do not       │                            │  │
│  │    │   recollect."    │                            │  │
│  │    └──────────────────┘                            │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                            │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  FreeResponseRow                                    │  │
│  │                                                     │  │
│  │  Or write your own:                                 │  │
│  │  ┌──────────────────────────────────┐  ┌────────┐  │  │
│  │  │ TMP_InputField (multiline)       │  │ Send ✉ │  │  │
│  │  └──────────────────────────────────┘  └────────┘  │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                            │
│           ✓ Great choice! That matches Maya's tone.        │
│                                                            │
│  [← Back]                           [Next Question →]     │
└────────────────────────────────────────────────────────────┘
```

---

## Step 1 — Add ConversationPanel to the Hierarchy

In the **GameScene**, find your existing `Canvas → QuestionArea` (or wherever
`MultipleChoicePanel` lives). Add the new panel **above** it:

```
Canvas
└── QuestionArea
    ├── ConversationPanel           ← ADD THIS (new)
    │   ├── CharacterPortrait       (Image)
    │   ├── DialogueBubble          (Panel/Image with background)
    │   │   ├── CharacterNameText   (TextMeshProUGUI)
    │   │   └── CharacterDialogueText (TextMeshProUGUI)
    │   └── (optional) NameBadge    (Image behind CharacterNameText)
    │
    ├── MultipleChoicePanel         ← existing (no changes needed)
    │   ├── QuestionText
    │   ├── OptionButton_0
    │   ├── OptionButton_1
    │   └── OptionButton_2          ← only 3 buttons used for parlour
    │
    ├── FreeResponseRow             ← ADD THIS (new, below MC panel)
    │   ├── FreeResponseLabel       (TextMeshProUGUI — "Or write your own:")
    │   ├── FreeResponseInput       (TMP_InputField)
    │   └── FreeResponseSubmitButton (Button — label "Send ✉")
    │
    └── FillInBlankPanel            ← existing (no changes needed)
```

> **Important:** Set `ConversationPanel` to **inactive** by default in the Inspector.
> `QuestionDisplay.SetPanelActive()` will enable/disable it automatically.

---

## Step 2 — Build the ConversationPanel

### 2a. ConversationPanel root

| Property | Value |
|----------|-------|
| Component | `Image` (optional background) or just `RectTransform` |
| Anchor | Top-stretch (full width, top half of QuestionArea) |
| Height | ~220–260 px |
| Color | Slightly lighter than background (e.g. `#1E2030` if bg is `#1A1A1A`) |
| Corner radius | Optional — add a `UI Rounded Corners` shader or use a Sprite with 9-slice |

---

### 2b. CharacterPortrait

| Property | Value |
|----------|-------|
| Component | `Image` |
| Source Image | Placeholder sprite (solid colour circle or silhouette) |
| Preserve Aspect | ✅ On |
| Position | Left side of `ConversationPanel`, vertically centered |
| Size | ~120 × 120 px (square) |
| Anchor | Middle-Left |

**Placeholder sprite:** Right-click `Assets/Resources/Characters/` →
*Create → UI Toolkit → (or import a 128×128 solid colour PNG)* and name it
`character_placeholder`. The code will try to load `Resources/Characters/<CharacterName>`
first, then fall back to `Resources/Characters/character_placeholder`.

---

### 2c. DialogueBubble

| Property | Value |
|----------|-------|
| Component | `Image` (speech bubble background) |
| Color | `#2A2D40` or any contrasting panel colour |
| Layout | `Vertical Layout Group` — auto-sizes to text content |
| Padding | 12–16 px on all sides |
| Position | Right of portrait, fills remaining width |
| Anchor | Stretch-right, vertically centered |

---

### 2d. CharacterNameText

| Property | Value |
|----------|-------|
| Component | `TextMeshProUGUI` |
| Font Size | 20–22 pt |
| Font Style | **Bold** |
| Color | Accent colour matching character (e.g. warm gold for Maya) |
| Text content | Leave empty — filled by code |
| Overflow | `Overflow` (single line) |
| Position | Top of `DialogueBubble` |

---

### 2e. CharacterDialogueText

| Property | Value |
|----------|-------|
| Component | `TextMeshProUGUI` |
| Font Size | 18–20 pt |
| Font Style | Italic (optional, gives a "speaking" feel) |
| Color | White or near-white (`#E8E8E8`) |
| Text content | Leave empty — filled by code |
| Overflow | `Overflow` |
| Word Wrap | ✅ On |
| Position | Below `CharacterNameText`, fills bubble height |

---

### 2f. FreeResponseRow

This is a separate GameObject **outside** `ConversationPanel`, placed below `MultipleChoicePanel`.

| Property | Value |
|----------|-------|
| Layout | `Horizontal Layout Group` (or manual placement) |
| Height | ~60–70 px |
| Start state | **Inactive** — code calls `freeResponseRow.SetActive(true/false)` |

#### FreeResponseLabel
| Property | Value |
|----------|-------|
| Component | `TextMeshProUGUI` |
| Text | `"Or write your own:"` |
| Font Size | 16–18 pt |
| Color | Muted white (`#AAAAAA`) |

#### FreeResponseInput
| Property | Value |
|----------|-------|
| Component | `TMP_InputField` |
| Line Type | `Multi Line Newline` (so the player can type a sentence) |
| Character Limit | 200 |
| Placeholder text | `"Type your response here..."` |
| Flex width | Expand to fill remaining row width |

#### FreeResponseSubmitButton
| Property | Value |
|----------|-------|
| Component | `Button + TextMeshProUGUI child` |
| Label | `"Send ✉"` |
| Width | ~80–90 px |
| Color | Accent colour (e.g. teal `#3ECFCF`) |

---

## Step 3 — Inspector Wiring

Select the `GameFlow` GameObject (the one with `QuestionDisplay` attached) and find
the **Conversation (Parlour)** and **Free Response (Parlour)** headers in the Inspector:

| Field | Drag in |
|-------|---------|
| `Conversation Panel` | `ConversationPanel` GameObject |
| `Character Portrait` | `CharacterPortrait` (Image component) |
| `Character Name Text` | `CharacterNameText` (TextMeshProUGUI) |
| `Character Dialogue Text` | `CharacterDialogueText` (TextMeshProUGUI) |
| `Free Response Row` | `FreeResponseRow` GameObject |
| `Free Response Input` | `FreeResponseInput` (TMP_InputField) |
| `Free Response Submit Button` | `FreeResponseSubmitButton` (Button) |

The existing **Multiple Choice** and **Shared** fields stay as they are — no changes.

---

## Step 4 — Portrait Assets

Portraits are loaded at runtime from `Assets/Resources/Characters/`.

### To add a real portrait for Maya:
1. Import your sprite into `Assets/Resources/Characters/`
2. Rename it exactly `Maya` (file: `Maya.png` or `Maya.asset`)
3. Set **Texture Type** → `Sprite (2D and UI)` in the Inspector
4. That's it — the code calls `Resources.Load<Sprite>("Characters/Maya")` automatically

### Placeholder setup (no art yet):
1. Create a folder: `Assets/Resources/Characters/`
2. Create a simple coloured square sprite named `character_placeholder`
3. Each character will show this placeholder until their portrait is added

### Per-character placeholder colours (suggestion):

| Character | Suggested colour |
|-----------|-----------------|
| Maya | Warm amber `#F5A623` |
| Victor | Cool steel `#5B6E82` |
| Zoe | Bright teal `#3ECFCF` |
| Dr. Chen | Sage green `#6AAB6A` |
| Alex | Charcoal `#4A4A4A` |

Create one coloured square PNG per character, named exactly after the character
(`Maya.png`, `Victor.png`, etc.) in `Assets/Resources/Characters/`.

---

## Step 5 — Suggested Rect Transform Values

These are for a 1080×1920 portrait (mobile) canvas. Adjust for your target resolution.

### ConversationPanel
```
Anchor Min: (0, 0.55)  Anchor Max: (1, 1)
Left: 0   Right: 0   Top: -80   Bottom: 0
```

### CharacterPortrait (inside ConversationPanel)
```
Anchor: Middle-Left
Pos X: 20   Pos Y: 0
Width: 130   Height: 130
```

### DialogueBubble (inside ConversationPanel)
```
Anchor: Stretch (left=160, right=0, top=10, bottom=10)
```

### CharacterNameText (inside DialogueBubble)
```
Anchor: Top-Stretch
Height: 32
```

### CharacterDialogueText (inside DialogueBubble)
```
Anchor: Stretch (below name)
```

### MultipleChoicePanel (existing, unchanged)
```
Anchor Min: (0, 0)  Anchor Max: (1, 0.52)
```

> Adjust the vertical split (0.52 / 0.55 boundary) depending on how much text
> the dialogue typically uses. If characters speak longer, give more height to ConversationPanel.

---

## Step 6 — Visual Polish (Optional)

### Speech bubble tail
Add a rotated `Image` (triangle sprite) at the bottom-left of `DialogueBubble`,
pointing toward the portrait. Position it at the left edge, overlapping the bubble bottom.

### Animated text reveal
In `QuestionDisplay.DisplayConversation()`, you can replace the direct text assignment:
```csharp
characterDialogueText.text = question.CharacterDialogue;
```
with a coroutine that types out each character one at a time (typewriter effect).
This is a purely cosmetic enhancement — the MC options should only appear after the
reveal is complete.

### Character name accent colours
Create a lookup in `QuestionDisplay` or a `CharacterTheme` ScriptableObject:
```csharp
// Example: tint name text by character
characterNameText.color = question.CharacterName switch {
    "Maya"    => new Color(0.96f, 0.65f, 0.14f),  // amber
    "Victor"  => new Color(0.36f, 0.43f, 0.51f),  // steel blue
    "Zoe"     => new Color(0.24f, 0.81f, 0.81f),  // teal
    "Dr. Chen"=> new Color(0.42f, 0.67f, 0.42f),  // sage
    "Alex"    => new Color(0.70f, 0.70f, 0.70f),  // grey
    _         => Color.white
};
```

---

## Step 7 — Testing

### Quick smoke test (no Ollama required)

In the Unity Editor, temporarily hardcode a `ConversationQuestion` in
`QuestionFlowManager.Start()` or use the **Debug Win Question** button to
skip generation and verify the layout renders correctly:

1. Open `GameScene`
2. Set `Player.CurrentChallenge` → `"parlour_coffee_shop"` on the player in Supabase
   (or temporarily hardcode in `QuestionFlowManager.Start()`)
3. Press Play
4. The `ConversationPanel` should appear with the portrait placeholder, character name,
   and dialogue text
5. The MC panel below should show "How do you respond?" + 4 option buttons

### Checklist before going live

- [ ] `ConversationPanel` starts **inactive** in Inspector
- [ ] `MultipleChoicePanel` starts **inactive** in Inspector  
- [ ] `FillInBlankPanel` starts **inactive** in Inspector
- [ ] `FreeResponseRow` starts **inactive** in Inspector
- [ ] All three `QuestionDisplay` panel fields are wired in Inspector
- [ ] `CharacterPortrait` field is wired (Image component)
- [ ] `CharacterNameText` and `CharacterDialogueText` fields wired
- [ ] `FreeResponseInput` and `FreeResponseSubmitButton` fields wired
- [ ] `Assets/Resources/Characters/` folder exists with at least `character_placeholder`
- [ ] Canvas Scaler is set to **Scale With Screen Size** (reference 1080×1920 or your target)

---

## How the Code Drives the UI

For reference — this is what `QuestionDisplay` does automatically, no manual steps needed:

```
DisplayQuestion(ConversationQuestion)
   │
   ├─ SetPanelActive(showMC: true, showConversation: true)
   │     • ConversationPanel.SetActive(true)
   │     • MultipleChoicePanel.SetActive(true)
   │     • FillInBlankPanel.SetActive(false)
   │
   ├─ Load portrait: Resources.Load<Sprite>("Characters/" + CharacterName)
   │     fallback:   Resources.Load<Sprite>("Characters/character_placeholder")
   │
   ├─ characterNameText.text     = question.CharacterName
   ├─ characterDialogueText.text = question.CharacterDialogue
   │
   └─ DisplayMultipleChoiceForConversation(question)
         • questionText.text = question.QuestionText  ("How do you respond?")
         • Shuffles 3 options → wires OptionButton_0/1/2
         • freeResponseRow.SetActive(question.AllowFreeResponse)  ← always true
         • Wires FreeResponseSubmitButton → OnFreeResponseSubmit()
```

When the player **picks an option button**: standard `OnOptionSelected()` flow.  
When the player **types and taps Send**: `OnFreeResponseSubmit()` submits the typed text.
The evaluator detects that the answer isn't in `question.Options` → uses AI verbal evaluation instead of `CheckAnswer()`.

When the challenge is **not** a parlour challenge:
```
SetPanelActive(showMC: true, showConversation: false)
   • ConversationPanel hidden
   • MultipleChoicePanel shown as normal
```

---

## Summary of New Inspector Fields

On the `QuestionDisplay` component:

**`[Header("Conversation (Parlour)")]`**

| Field name in Inspector | Type | What to drag in |
|------------------------|------|----------------|
| `Conversation Panel` | GameObject | `ConversationPanel` root |
| `Character Portrait` | Image | `CharacterPortrait` Image component |
| `Character Name Text` | TextMeshProUGUI | `CharacterNameText` |
| `Character Dialogue Text` | TextMeshProUGUI | `CharacterDialogueText` |

**`[Header("Free Response (Parlour)")]`**

| Field name in Inspector | Type | What to drag in |
|------------------------|------|----------------|
| `Free Response Row` | GameObject | `FreeResponseRow` root |
| `Free Response Input` | TMP_InputField | `FreeResponseInput` |
| `Free Response Submit Button` | Button | `FreeResponseSubmitButton` |

All seven fields are **null-safe** — if not wired, parlour questions still play using just the MC option buttons, without the portrait/dialogue/free-text UI. Wire them progressively as you build the scene.
