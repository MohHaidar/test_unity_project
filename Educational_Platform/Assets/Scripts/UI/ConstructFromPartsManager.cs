
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ConstructFromPartsManager : BaseActivityManager
{
    public TMP_InputField inputField;

    protected override void RenderQuestionUI(Question q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;

        inputField.text = "";
        inputField.gameObject.SetActive(true);

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(SubmitConstructedAnswer);

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.Streak = 0;
            ShowStartPanel();
        });
    }

    private void SubmitConstructedAnswer()
    {
        string answer = inputField.text.Trim();
        currentActivity.SubmitAnswer(answer);

        if (currentActivity.Streak >= 5)
        {
            messagePanel.SetActive(true);
            messageText.text = "Activity Complete!";
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                currentActivity.Streak = 0;
                LoadCurrentQuestion();
            });
        }
        else
        {
            LoadCurrentQuestion();
        }
    }
}
