using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class ChallengeManager : MonoBehaviour
{
    public GameObject moduleWindow;
    public GameObject startPanel;
    public GameObject messagePanel;

    public Button startButton;
    public Button backButton;
    public Button abandonButton;
    public Button continueButton;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI moduleNameText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI rewardAvailability;

    public Button[] optionButtons;
    public GameObject moduleCirclePrefab;
    public Transform moduleTrackerPanel;
    public GameObject streakDotPrefab;
    public Transform streakDotContainer;

    private Challenge currentChallenge;
    private PlayerProfile currentPlayer;
    private ChallengeCooldownRow cooldownRow;
    private List<GameObject> moduleCircles = new();
    private List<GameObject> streakDots = new();

    void Start()
    {
        currentChallenge = ChallengeLoader.SelectedChallenge;

        // Load player profile from database
        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        currentPlayer = PlayerCSVManager.QueryPlayerProfile(playerName);
        cooldownRow = ChallengeRewardCooldown.GetOrCreateCooldown(currentPlayer.Name, currentChallenge.ChallengeName);

        if (currentChallenge == null)
        {
            Debug.LogError("No challenge loaded.");
            return;
        }

        GenerateModuleTracker(); // Update Step tracker
        ShowStartPanel(); // Unlock first activity
    }

    void ShowRewardAvailability()
    {
        bool canClaim = ChallengeRewardCooldown.CanClaimReward(cooldownRow);

        string displayText;

        if (canClaim)
        {
            displayText = "Reward available!";
        }
        else
        {
            DateTime lastClaim = DateTime.Parse(cooldownRow.LastClaimedISO);
            DateTime nextEligible = lastClaim.AddDays(cooldownRow.WaitDays);

            TimeSpan remaining = nextEligible - DateTime.Now;
            displayText = $"Reward in {remaining.Days}d {remaining.Hours}h {remaining.Minutes}m";
        }
        rewardAvailability.text = displayText;

    }
    void GenerateModuleTracker()
    {
        for (int i = 0; i < currentChallenge.Modules.Count; i++)
        {
            GameObject circle = Instantiate(moduleCirclePrefab, moduleTrackerPanel);
            TextMeshProUGUI label = circle.GetComponentInChildren<TextMeshProUGUI>();
            label.text = (i + 1).ToString();
            moduleCircles.Add(circle);
        }
        UpdateModuleTracker();
    }

    void UpdateModuleTracker()
    {
        for (int i = 0; i < moduleCircles.Count; i++)
        {
            var image = moduleCircles[i].GetComponent<Image>();
            image.color = i == currentChallenge.CurrentIndex ? Color.yellow : Color.white;
        }
    }

    void ShowStartPanel()
    {
        startPanel.SetActive(true);
        moduleWindow.SetActive(false);
        messagePanel.SetActive(false);
        Title.text = currentChallenge.ChallengeName;
        UpdateModuleTracker();
        ShowRewardAvailability();

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            currentChallenge.CurrentQuestion = null;
            LoadCurrentQuestion();
        });

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Arithmetics");
        });
    }

    void LoadCurrentQuestion()
    {
        startPanel.SetActive(false);
        messagePanel.SetActive(false);
        moduleWindow.SetActive(true);

        if (currentChallenge.ChallengeCompleted)
        {
            int score = currentChallenge.CalculateScore();
            int oldMMR = currentPlayer.MMR;
            int newMMR = MMRCalculator.CalculateNewMMR(oldMMR, currentChallenge.ChallengeMMR, true);

            currentPlayer.TotalScore += score;
            currentPlayer.MMR = newMMR;

            PlayerPrefs.SetInt("MMR", currentPlayer.MMR);
            PlayerPrefs.SetInt("TotalScore", currentPlayer.TotalScore);

            messagePanel.SetActive(true);

            if (ChallengeRewardCooldown.CanClaimReward(cooldownRow))
            {
                ChallengeRewardCooldown.MarkClaimed(cooldownRow);
                CooldownCSVManager.SaveOrUpdateCooldown(cooldownRow);

                int reward = score / 10;
                currentPlayer.PlayerCoins += reward;
                PlayerPrefs.SetInt("PlayerCoins", currentPlayer.PlayerCoins);
                messageText.text = $"Challenge Complete!\nReward claimed: +{reward} coins";
            }
            else
            {
                messageText.text = " Challenge Complete!\nReward cooldown active. No coins awarded.";
            }

            ChallengeLogger.LogResult(currentPlayer.Name, currentChallenge, score, oldMMR, newMMR);

            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                currentChallenge.RefreshChallenge();
                ShowStartPanel();
            });

            return;
        }

        var q = currentChallenge.GetCurrentQuestion();
        if (q == null) return;

        promptText.text = q.Prompt;
        moduleNameText.text = q.Type;
        UpdateModuleTracker();
        UpdateStreakDots();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = q.Options[i];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                if (currentChallenge.UltimateUnlocked)
                    currentChallenge.SubmitUltimateAnswer(q.Options[idx]);
                else
                    currentChallenge.SubmitAnswer(q.Options[idx]);

                if (currentChallenge.UltimateUnlocked || currentChallenge.Streak >= 5)
                {
                    messagePanel.SetActive(true);
                    messageText.text = currentChallenge.UltimateUnlocked ? "Final Challenge Unlocked!" : "Module Complete!";
                    continueButton.gameObject.SetActive(true);
                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(() =>
                    {
                        currentChallenge.Streak = 0;
                        LoadCurrentQuestion();
                    });
                }
                else
                    LoadCurrentQuestion();
            });
        }

        abandonButton.onClick.RemoveAllListeners();
        abandonButton.onClick.AddListener(() =>
        {
            // Onlt refresh Streak, so the player is able to resume the Module (TODO: pause timer)
            currentChallenge.Streak = 0;
            ShowStartPanel();
        });
    }

    void UpdateStreakDots()
    {
        foreach (var dot in streakDots) Destroy(dot);
        streakDots.Clear();

        for (int i = 0; i < currentChallenge.Streak; i++)
        {
            GameObject dot = Instantiate(streakDotPrefab, streakDotContainer);
            streakDots.Add(dot);
        }
    }
    
    void OnDestroy()
    {
        // Save from Memory to database
        currentPlayer = PlayerProfile.GetFromMemory();
        PlayerCSVManager.UpdatePlayerProfile(currentPlayer);
        Debug.Log("Scene is being unloaded or object destroyed.");
    }
}
