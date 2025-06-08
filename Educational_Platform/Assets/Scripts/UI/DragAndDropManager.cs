
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DragAndDropManager : BaseActivityManager
{
    public Transform itemContainer;
    public Transform[] dropZones;
    public GameObject draggableItemPrefab;

    private Dictionary<string, string> matchPairs;
    private Dictionary<string, string> userMatches = new();

    protected override void RenderQuestionUI(Question q)
    {
        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;

        matchPairs = q.MatchPairs;
        userMatches.Clear();

        // Clear existing
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
        foreach (Transform zone in dropZones)
            foreach (Transform child in zone)
                Destroy(child.gameObject);

        // Instantiate draggable items
        foreach (var pair in matchPairs)
        {
            GameObject item = Instantiate(draggableItemPrefab, itemContainer);
            item.GetComponentInChildren<TextMeshProUGUI>().text = pair.Key;
            // Add drag logic (requires DragHandler component)
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(SubmitDragAndDropAnswer);

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.Streak = 0;
            ShowStartPanel();
        });
    }

    private void SubmitDragAndDropAnswer()
    {
        // Assume each draggable has a component tracking which drop zone it's in
        // For now, simulate correct answer collection:
        foreach (var pair in matchPairs)
        {
            userMatches[pair.Key] = pair.Value; // Replace with actual drop zone name
        }

        string answer = string.Join(", ", userMatches.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
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
