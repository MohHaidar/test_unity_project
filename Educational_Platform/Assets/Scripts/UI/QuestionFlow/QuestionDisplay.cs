using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Displays questions to the player.
/// Polymorphic: handles MultipleChoiceQuestion, FillInBlankQuestion, and ConversationQuestion.
///
/// Unity Inspector wiring:
///   Multiple Choice Panel  — GameObject wrapping questionText + optionButtons
///   Fill In Blank Panel    — GameObject wrapping fibQuestionText, fibInputFields, fibInputLabels, fibSubmitButton
///   Conversation Panel     — GameObject wrapping characterPortrait, characterNameText, characterDialogueText
///                            (shown together with Multiple Choice Panel for ConversationQuestion)
///
/// See FILL_IN_BLANK_UI.md and PARLOUR_UI.md for hierarchy and Inspector setup guides.
/// </summary>
public class QuestionDisplay : MonoBehaviour
{
    [Header("Multiple Choice")]
    [SerializeField] private GameObject multipleChoicePanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons = new Button[4];

    [Header("Fill In Blank")]
    [SerializeField] private GameObject fillInBlankPanel;
    [SerializeField] private TextMeshProUGUI fibQuestionText;
    [SerializeField] private TMP_InputField[] fibInputFields = new TMP_InputField[4];
    [SerializeField] private TextMeshProUGUI[] fibInputLabels = new TextMeshProUGUI[4];
    [SerializeField] private Button fibSubmitButton;

    [Header("Conversation (Parlour)")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDialogueText;

    [Header("Shared")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    private IQuestion _currentQuestion;
    private AnswerSubmitter _answerSubmitter;

    private void Awake()
    {
        _answerSubmitter = GetComponent<AnswerSubmitter>();
        if (_answerSubmitter == null)
            Debug.LogError("[QuestionDisplay] AnswerSubmitter not found on this GameObject");
    }

    /// <summary>
    /// Displays a question based on its type; shows/hides the correct panel.
    /// </summary>
    public void DisplayQuestion(IQuestion question)
    {
        if (question == null)
        {
            Debug.LogError("[QuestionDisplay] Question is null");
            return;
        }

        _currentQuestion = question;

        if (question is ConversationQuestion convQuestion)
        {
            SetPanelActive(showMC: true, showConversation: true);
            DisplayConversation(convQuestion);
        }
        else if (question is MultipleChoiceQuestion mcQuestion)
        {
            SetPanelActive(showMC: true, showConversation: false);
            DisplayMultipleChoice(mcQuestion);
        }
        else if (question is FillInBlankQuestion fibQuestion)
        {
            SetPanelActive(showMC: false, showConversation: false);
            DisplayFillInBlank(fibQuestion);
        }
        else
        {
            Debug.LogError($"[QuestionDisplay] Unknown question type: {question.QuestionType}");
        }

        Debug.Log($"[QuestionDisplay] Displayed: {question}");
    }

    private void SetPanelActive(bool showMC, bool showConversation = false)
    {
        if (multipleChoicePanel != null) multipleChoicePanel.SetActive(showMC);
        if (fillInBlankPanel    != null) fillInBlankPanel.SetActive(!showMC && !showConversation);
        if (conversationPanel   != null) conversationPanel.SetActive(showConversation);
    }

    // ─── Conversation (Parlour) ───────────────────────────────────────────────

    private void DisplayConversation(ConversationQuestion question)
    {
        // Load character portrait from Resources/Characters/<AvatarKey>
        if (characterPortrait != null)
        {
            Sprite portrait = Resources.Load<Sprite>($"Characters/{question.CharacterName}");
            if (portrait != null)
                characterPortrait.sprite = portrait;
            else
                characterPortrait.sprite = Resources.Load<Sprite>("Characters/character_placeholder");
        }

        if (characterNameText     != null) characterNameText.text     = question.CharacterName;
        if (characterDialogueText != null) characterDialogueText.text = question.CharacterDialogue;

        // Reuse the MC panel for question text + options (it's already active)
        DisplayMultipleChoiceForConversation(question);
    }

    private void DisplayMultipleChoiceForConversation(ConversationQuestion question)
    {
        if (questionText != null)
            questionText.text = question.QuestionText;

        var shuffledOptions = new List<string>(question.Options);
        var rng = new System.Random();
        int n = shuffledOptions.Count;
        while (n > 1) { n--; int k = rng.Next(n + 1); var t = shuffledOptions[k]; shuffledOptions[k] = shuffledOptions[n]; shuffledOptions[n] = t; }
        question.Options = shuffledOptions;

        int buttonCount = Mathf.Min(optionButtons.Length, shuffledOptions.Count);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;
            bool active = i < buttonCount;
            optionButtons[i].gameObject.SetActive(active);
            if (!active) continue;

            var buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = shuffledOptions[i];

            int idx = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(question.Options[idx]));
            optionButtons[i].interactable = true;
        }

        if (feedbackText != null) feedbackText.text = "";
    }

    // ─── Multiple Choice ──────────────────────────────────────────────────────

    private void DisplayMultipleChoice(MultipleChoiceQuestion question)
    {
        if (questionText != null)
            questionText.text = question.QuestionText;

        if (optionButtons.Length != 4)
        {
            Debug.LogError($"[QuestionDisplay] Expected 4 option buttons, found {optionButtons.Length}");
            return;
        }

        var shuffledOptions = new List<string>(question.Options);
        var rng = new System.Random();
        int n = shuffledOptions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var tmp = shuffledOptions[k];
            shuffledOptions[k] = shuffledOptions[n];
            shuffledOptions[n] = tmp;
        }
        question.Options = shuffledOptions;

        for (int i = 0; i < 4; i++)
        {
            if (optionButtons[i] == null) { Debug.LogError($"[QuestionDisplay] Option button {i} not assigned"); continue; }

            TextMeshProUGUI buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = question.Options[i];

            int optionIndex = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(question.Options[optionIndex]));
            optionButtons[i].interactable = true;
        }

        if (feedbackText != null) feedbackText.text = "";
    }

    private void OnOptionSelected(string selectedOption)
    {
        if (_currentQuestion == null) { Debug.LogError("[QuestionDisplay] No question to answer"); return; }
        DisableAllButtons();
        _answerSubmitter?.SubmitAnswer(selectedOption);
    }

    // ─── Fill In Blank ────────────────────────────────────────────────────────

    /// <summary>
    /// Renders a FillInBlankQuestion: sets question text, activates one input field per blank,
    /// sets labels, and wires the submit button.
    /// </summary>
    private void DisplayFillInBlank(FillInBlankQuestion question)
    {
        if (fibQuestionText != null)
            fibQuestionText.text = question.QuestionText;

        int blankCount = question.Blanks != null ? question.Blanks.Count : 0;

        for (int i = 0; i < fibInputFields.Length; i++)
        {
            bool active = i < blankCount;

            if (fibInputFields[i] != null)
            {
                fibInputFields[i].gameObject.SetActive(active);
                if (active)
                {
                    fibInputFields[i].text = "";
                    fibInputFields[i].interactable = true;
                }
            }

            if (fibInputLabels != null && i < fibInputLabels.Length && fibInputLabels[i] != null)
            {
                fibInputLabels[i].gameObject.SetActive(active);
                if (active)
                    fibInputLabels[i].text = question.Blanks[i].Label;
            }
        }

        if (fibSubmitButton != null)
        {
            fibSubmitButton.onClick.RemoveAllListeners();
            fibSubmitButton.onClick.AddListener(OnFIBSubmit);
            fibSubmitButton.interactable = true;
        }

        if (feedbackText != null) feedbackText.text = "";
    }

    private void OnFIBSubmit()
    {
        if (_currentQuestion == null) { Debug.LogError("[QuestionDisplay] No FIB question active"); return; }

        var parts = new List<string>();
        foreach (var field in fibInputFields)
            if (field != null && field.gameObject.activeSelf)
                parts.Add(field.text.Trim());

        string answer = string.Join("|", parts);
        DisableFIBInput();
        _answerSubmitter?.SubmitAnswer(answer);
    }

    private void DisableFIBInput()
    {
        foreach (var field in fibInputFields)
            if (field != null) field.interactable = false;
        if (fibSubmitButton != null) fibSubmitButton.interactable = false;
    }

    // ─── Shared helpers ───────────────────────────────────────────────────────

    /// <summary>Shows feedback message (rich text tags handled by the message itself).</summary>
    public void ShowFeedback(bool isCorrect, string richMessage)
    {
        if (feedbackText == null) return;
        feedbackText.color = Color.white;
        feedbackText.text = richMessage;
        Debug.Log($"[QuestionDisplay] Feedback shown (correct={isCorrect})");
    }

    public void DisableAllButtons()
    {
        foreach (var button in optionButtons)
            if (button != null) button.interactable = false;
    }

    public void EnableAllButtons()
    {
        foreach (var button in optionButtons)
            if (button != null) button.interactable = true;
    }

    public void ClearDisplay()
    {
        if (questionText          != null) questionText.text          = "";
        if (fibQuestionText       != null) fibQuestionText.text       = "";
        if (feedbackText          != null) feedbackText.text          = "";
        if (characterNameText     != null) characterNameText.text     = "";
        if (characterDialogueText != null) characterDialogueText.text = "";

        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.interactable = false;
                var t = button.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = "";
            }
        }

        foreach (var field in fibInputFields)
            if (field != null) { field.text = ""; field.interactable = false; }

        DisableFIBInput();
        _currentQuestion = null;
    }
}
