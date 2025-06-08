using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public abstract class BaseActivityManager : MonoBehaviour
{
    public GameObject moduleWindow, startPanel, messagePanel;
    public Button startButton, backButton, abandonButton, continueButton;
    public TextMeshProUGUI Title, promptText, moduleNameText, messageText;

    public GameObject streakDotPrefab;
    public Transform streakDotContainer;

    protected Activity currentActivity;
    protected PlayerProfile currentPlayer;
    // protected ChallengeCooldownRow cooldownRow;
    protected List<GameObject> streakDots = new();
    public int MAX_ATTEMPTS = 20;

    protected virtual void Start()
    {
        currentActivity = ActivityLoader.SelectedActivity;
        currentPlayer = PlayerCSVManager.QueryPlayerProfile(PlayerPrefs.GetString("PlayerName", "Guest"));

        ShowStartPanel();
    }

    protected void ShowStartPanel()
    {
        startPanel.SetActive(true);
        moduleWindow.SetActive(false);
        messagePanel.SetActive(false);

        // Set Assets
        Title.text = currentActivity.ActivityId;
        startButton.onClick.AddListener(() => MainLoop());
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Arithmetics"));
    }

    protected void MainLoop()
    {
        startPanel.SetActive(false);
        messagePanel.SetActive(false);
        moduleWindow.SetActive(true);

        while (PerformanceHistory.Count <= MAX_ATTEMPTS)
        {
            UpdateStreakDots();
            CheckStreak();

            // Load Question
            var q = currentActivity.GetCurrentQuestion();

            RenderQuestionUI(q);
            RenderAbandonButton();
        }
    }

    private void RenderAbandonButton()
    {
        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            currentActivity.RefreshActivity();
            SceneManager.LoadScene("ChallengeScene");
        });
    }

    private void CheckStreak()
    {
        if (currentActivity.Streak >= 5)
        {
            HandleActivityCompleted();
        }
        else
            return;
    }

    protected void HandleActivityCompleted()
    {
        ActivityPerformance report = Evaluation.GeneratePerformanceReport(PerformanceHistory);
        // Log the report with player name, activityId, 

        // int oldMMR = currentPlayer.MMR;
        // int newMMR = MMRCalculator.CalculateNewMMR(oldMMR, currentActivity.ActivityMMR, true);
        // currentPlayer.MMR = newMMR;
        // PlayerPrefs.SetInt("MMR", newMMR);

        // messagePanel.SetActive(true);

        // int reward = score / 10;
        // currentPlayer.PlayerCoins += reward;
        // PlayerPrefs.SetInt("PlayerCoins", currentPlayer.PlayerCoins);
        // messageText.text = $"Challenge Complete!\nReward claimed: +{reward} coins";
        // Rewards in Step completion

        messagePanel.SetActive(true);
        messageText.text = "Activity Complete!";
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            currentActivity.RefreshActivity();
            SceneManager.LoadScene("ChallengeScene");
        });
    }

    protected void UpdateStreakDots()
    {
        foreach (var dot in streakDots) Destroy(dot);
        streakDots.Clear();

        for (int i = 0; i < currentChallenge.Streak; i++)
        {
            GameObject dot = Instantiate(streakDotPrefab, streakDotContainer);
            streakDots.Add(dot);
        }
    }

    protected void OnDestroy()
    {
        currentPlayer = PlayerProfile.GetFromMemory();
        PlayerCSVManager.UpdatePlayerProfile(currentPlayer);
    }

    // 🧩 Abstract methods to implement per scene
    protected abstract void RenderQuestionUI(QuestionModule q);
}
