using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Displays questions to the player.
/// Polymorphic: handles any IQuestion implementation (MultipleChoice, DragDrop, etc).
/// Currently implements MultipleChoice; others can be added by creating Display methods.
/// </summary>
public class QuestionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] optionButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI feedbackText;

    private IQuestion _currentQuestion;
    private AnswerSubmitter _answerSubmitter;

    private void Awake()
    {
        _answerSubmitter = GetComponent<AnswerSubmitter>();
        if (_answerSubmitter == null)
        {
            Debug.LogError("[QuestionDisplay] AnswerSubmitter not found on this GameObject");
        }
    }

    /// <summary>
    /// Displays a question based on its type.
    /// Polymorphic: handles different question types.
    /// </summary>
    public void DisplayQuestion(IQuestion question)
    {
        if (question == null)
        {
            Debug.LogError("[QuestionDisplay] Question is null");
            return;
        }

        _currentQuestion = question;

        // Display based on question type
        if (question is MultipleChoiceQuestion mcQuestion)
        {
            DisplayMultipleChoice(mcQuestion);
        }
        else
        {
            Debug.LogError($"[QuestionDisplay] Unknown question type: {question.QuestionType}");
        }

        Debug.Log($"[QuestionDisplay] Displayed: {question}");
    }

    /// <summary>
    /// Displays a multiple choice question with 4 option buttons.
    /// </summary>
    private void DisplayMultipleChoice(MultipleChoiceQuestion question)
    {
        // Display question text
        if (questionText != null)
        {
            questionText.text = question.QuestionText;
        }

        // Display options as buttons
        if (optionButtons.Length != 4)
        {
            Debug.LogError($"[QuestionDisplay] Expected 4 option buttons, found {optionButtons.Length}");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (optionButtons[i] == null)
            {
                Debug.LogError($"[QuestionDisplay] Option button {i} is not assigned");
                continue;
            }

            // Set button text
            TextMeshProUGUI buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = question.Options[i];
            }

            // Set button click listener
            int optionIndex = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(question.Options[optionIndex]));

            // Enable button
            optionButtons[i].interactable = true;
        }

        // Clear feedback
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// Called when a player clicks an option button.
    /// </summary>
    private void OnOptionSelected(string selectedOption)
    {
        if (_currentQuestion == null)
        {
            Debug.LogError("[QuestionDisplay] No question to answer");
            return;
        }

        // Disable all buttons
        DisableAllButtons();

        // Submit answer via AnswerSubmitter
        if (_answerSubmitter != null)
        {
            _answerSubmitter.SubmitAnswer(selectedOption);
        }
        else
        {
            Debug.LogError("[QuestionDisplay] AnswerSubmitter is not available");
        }
    }

    /// <summary>
    /// Shows feedback: Correct or Incorrect with explanation.
    /// </summary>
    public void ShowFeedback(bool isCorrect, string explanation)
    {
        if (feedbackText == null) return;

        if (isCorrect)
        {
            feedbackText.text = $"<color=green>Correct!</color>\n{explanation}";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = $"<color=red>Incorrect</color>\n{explanation}";
            feedbackText.color = Color.red;
        }

        Debug.Log($"[QuestionDisplay] Feedback: {feedbackText.text}");
    }

    /// <summary>
    /// Disables all option buttons (after answer submitted).
    /// </summary>
    public void DisableAllButtons()
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    /// <summary>
    /// Enables all option buttons (for next question).
    /// </summary>
    public void EnableAllButtons()
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.interactable = true;
            }
        }
    }

    /// <summary>
    /// Clears the display between questions.
    /// </summary>
    public void ClearDisplay()
    {
        if (questionText != null) questionText.text = "";
        if (feedbackText != null) feedbackText.text = "";

        foreach (var button in optionButtons)
        {
            if (button != null)
            {
                button.interactable = false;
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "";
            }
        }

        _currentQuestion = null;
    }
}
