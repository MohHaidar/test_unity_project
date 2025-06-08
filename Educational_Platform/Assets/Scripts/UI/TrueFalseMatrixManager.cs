
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TrueFalseMatrixManager : BaseActivityManager
{
    public Transform statementContainer;
    public GameObject statementRowPrefab;

    private Dictionary<string, string> userResponses = new();

    protected override void RenderQuestionUI(Question q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;
        
        userResponses.Clear();

        foreach (Transform child in statementContainer)
            Destroy(child.gameObject);

        foreach (var pair in q.MatchPairs)
        {
            GameObject row = Instantiate(statementRowPrefab, statementContainer);
            row.GetComponentInChildren<TextMeshProUGUI>().text = pair.Key;

            Toggle trueToggle = row.transform.Find("TrueToggle").GetComponent<Toggle>();
            Toggle falseToggle = row.transform.Find("FalseToggle").GetComponent<Toggle>();

            trueToggle.isOn = false;
            falseToggle.isOn = false;

            trueToggle.onValueChanged.AddListener((val) => {
                if (val) userResponses[pair.Key] = "True";
            });
            falseToggle.onValueChanged.AddListener((val) => {
                if (val) userResponses[pair.Key] = "False";
            });
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(SubmitTrueFalseMatrixAnswer);

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.Streak = 0;
            ShowStartPanel();
        });
    }

    private void SubmitTrueFalseMatrixAnswer()
    {
        string answer = string.Join(", ", userResponses.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
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
