using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ChallengeSelectUI: populate subjects -> challenges -> steps
/// Expects in-scene UI elements wired in the Inspector:
/// - TMP_Dropdown subjectDropdown
/// - TMP_Dropdown challengeDropdown
/// - Transform stepsContainer
/// - GameObject stepButtonPrefab (Button prefab with TMP child)
/// - Button backButton
/// </summary>
public class ChallengeSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown subjectDropdown;
    public TMP_Dropdown challengeDropdown;
    public Transform stepsContainer;
    public GameObject stepButtonPrefab;
    public Button backButton;

    private Player _player;
    private List<Challenge> _currentChallenges = new List<Challenge>();
    private Challenge _selectedChallenge;

    private void Start()
    {
        _player = PlayerDataManager.Instance.LoadPlayer(1, "Player");
        if (subjectDropdown == null || challengeDropdown == null)
        {
            Debug.LogError("[ChallengeSelectUI] Dropdown references not set in Inspector");
            return;
        }

        PopulateSubjects();

        subjectDropdown.onValueChanged.AddListener(OnSubjectChanged);
        challengeDropdown.onValueChanged.AddListener(OnChallengeChanged);

        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
    }

    private void PopulateSubjects()
    {
        var subjects = ChallengeDataManager.Instance.GetAllSubjects();
        subjectDropdown.ClearOptions();
        subjectDropdown.AddOptions(subjects);

        int idx = subjects.IndexOf(_player.CurrentSubject);
        if (idx >= 0) subjectDropdown.value = idx;
        subjectDropdown.RefreshShownValue();

        OnSubjectChanged(subjectDropdown.value);
    }

    private void OnSubjectChanged(int index)
    {
        string subject = subjectDropdown.options[index].text;
        _currentChallenges = ChallengeDataManager.Instance.GetChallengesForSubject(subject);

        List<string> names = new List<string>();
        foreach (var c in _currentChallenges) names.Add(c.Name);

        challengeDropdown.ClearOptions();
        challengeDropdown.AddOptions(names);

        int sel = 0;
        for (int i = 0; i < _currentChallenges.Count; i++)
        {
            if (_currentChallenges[i].Id.Equals(_player.CurrentChallenge, StringComparison.OrdinalIgnoreCase)) { sel = i; break; }
        }
        challengeDropdown.value = sel;
        challengeDropdown.RefreshShownValue();

        OnChallengeChanged(challengeDropdown.value);
    }

    private void OnChallengeChanged(int index)
    {
        if (index < 0 || index >= _currentChallenges.Count) return;
        _selectedChallenge = _currentChallenges[index];
        RefreshStepsUI();
    }

    private void RefreshStepsUI()
    {
        if (stepsContainer == null || stepButtonPrefab == null || _selectedChallenge == null) return;

        foreach (Transform child in stepsContainer) Destroy(child.gameObject);

        for (int i = 0; i < _selectedChallenge.Steps.Count; i++)
        {
            var step = _selectedChallenge.Steps[i];
            var go = GameObject.Instantiate(stepButtonPrefab, stepsContainer);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = $"Step {step.Number}: {step.Description}";

            string key = $"{step.Subject}:{_selectedChallenge.Id}:{step.Number}";
            bool completed = _player.CompletedSteps != null && _player.CompletedSteps.Contains(key);
            bool unlocked = false;
            if (step.Number == 1) unlocked = true;
            else
            {
                string prevKey = $"{step.Subject}:{_selectedChallenge.Id}:{step.Number - 1}";
                unlocked = _player.CompletedSteps != null && _player.CompletedSteps.Contains(prevKey);
            }

            ColorBlock colors = btn.colors;
            if (completed)
            {
                colors.normalColor = Color.green;
            }
            else if (unlocked)
            {
                colors.normalColor = Color.yellow;
            }
            else
            {
                colors.normalColor = Color.red;
                btn.interactable = false;
            }
            btn.colors = colors;

            int stepNumber = step.Number;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnStepSelected(_selectedChallenge.Id, stepNumber));
        }
    }

    private void OnStepSelected(string challengeId, int stepNumber)
    {
        Debug.Log($"[ChallengeSelectUI] Selected {challengeId} step {stepNumber}");

        // Update both subject AND challenge so lookup in GameScene is correct
        string subject = subjectDropdown.options[subjectDropdown.value].text;
        _player.CurrentSubject = subject;
        _player.SelectStep(challengeId, stepNumber);

        PlayerDataManager.Instance.SavePlayer(_player);
        SceneManager.LoadScene("GameScene");
    }

    public void OnBackPressed()
    {
        // No main menu scene exists; ChallengeSelect is the entry point.
        // Reload the selection screen (effectively a reset).
        SceneManager.LoadScene("ChallengeSelect");
    }
}
