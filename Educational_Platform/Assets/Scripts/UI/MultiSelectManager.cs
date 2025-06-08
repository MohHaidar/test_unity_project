
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MultiSelectManager : BaseActivityManager
{
    public Toggle[] optionToggles;

    protected override void RenderQuestionUI(Questione q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;
        
        for (int i = 0; i < optionToggles.Length; i++)
        {
            optionToggles[i].gameObject.SetActive(true);
            optionToggles[i].isOn = false;
            optionToggles[i].GetComponentInChildren<TextMeshProUGUI>().text = q.Options[i];
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(SubmitMultiSelectAnswer);

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.Streak = 0;
            ShowStartPanel();
        });
    }

    private void SubmitMultiSelectAnswer()
    {
        List<string> selected = new();
        foreach (var toggle in optionToggles)
        {
            if (toggle.isOn)
                selected.Add(toggle.GetComponentInChildren<TextMeshProUGUI>().text);
        }

        string answer = string.Join(", ", selected);
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
