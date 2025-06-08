
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SortOrderManager : BaseActivityManager
{
    public Transform sortableListPanel;
    public GameObject optionItemPrefab;

    private List<GameObject> sortableItems = new();

    protected override void RenderQuestionUI(Question q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;
        
        // Clear previous
        foreach (Transform child in sortableListPanel)
            GameObject.Destroy(child.gameObject);
        sortableItems.Clear();

        // Instantiate new sortable items
        foreach (string option in q.Options)
        {
            GameObject item = GameObject.Instantiate(optionItemPrefab, sortableListPanel);
            item.GetComponentInChildren<TextMeshProUGUI>().text = option;
            sortableItems.Add(item);
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(SubmitSortedAnswer);

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.Streak = 0;
            ShowStartPanel();
        });
    }

    private void SubmitSortedAnswer()
    {
        List<string> orderedAnswers = new();
        foreach (Transform item in sortableListPanel)
        {
            string answer = item.GetComponentInChildren<TextMeshProUGUI>().text;
            orderedAnswers.Add(answer);
        }

        string answerString = string.Join(", ", orderedAnswers);
        currentActivity.SubmitAnswer(answerString);

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
