using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ChallengeSelectUI: populate subjects -> challenges -> steps.
/// Expects in-scene UI elements wired in the Inspector:
/// - TMP_Dropdown subjectDropdown
/// - TMP_Dropdown challengeDropdown
/// - Transform stepsContainer
/// - GameObject stepButtonPrefab (Button prefab with TMP child)
/// - Button backButton
/// Optional stage display (wire in Inspector for the stage panel):
/// - TMP_Text stageLabel     → shows "Stage 2 · Arithmetic Mastery"
/// - TMP_Text stageProgress  → shows "1 / 5 challenges complete"
/// - Slider stageProgressBar → filled 0–1 within the current stage
/// </summary>
public class ChallengeSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown subjectDropdown;
    public TMP_Dropdown challengeDropdown;
    public Transform stepsContainer;
    public GameObject stepButtonPrefab;
    public Button backButton;

    [Header("Stage Display (optional)")]
    public TextMeshProUGUI stageLabel;
    public TextMeshProUGUI stageProgress;
    public Slider stageProgressBar;

    private Player _player;
    private List<Challenge> _currentChallenges = new List<Challenge>();
    private Challenge _selectedChallenge;

    private async void Start()
    {
        // SupabaseClient must be in this scene as a component on a GameObject.
        // It initializes in Awake() before Start() runs, so it's ready here.
        if (subjectDropdown == null || challengeDropdown == null)
        {
            Debug.LogError("[ChallengeSelectUI] Dropdown references not set in Inspector");
            return;
        }

        // Load player and challenges from Supabase (falls back gracefully if unavailable)
        _player = await PlayerDataManager.Instance.LoadPlayerAsync(1, "Player");
        await ChallengeDataManager.Instance.LoadFromSupabaseAsync();

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
        foreach (var c in _currentChallenges)
        {
            bool unlocked = ChallengeDataManager.Instance.IsChallengeUnlocked(c.Id, _player);
            string lockIcon = unlocked ? "" : "🔒 ";
            names.Add($"{lockIcon}{c.Name} [S{c.StageNumber}]");
        }

        challengeDropdown.ClearOptions();
        challengeDropdown.AddOptions(names);

        int sel = 0;
        for (int i = 0; i < _currentChallenges.Count; i++)
        {
            var c = _currentChallenges[i];
            if (c.Slug.Equals(_player.CurrentChallenge, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals(_player.CurrentChallenge,  StringComparison.OrdinalIgnoreCase))
            { sel = i; break; }
        }
        challengeDropdown.value = sel;
        challengeDropdown.RefreshShownValue();

        OnChallengeChanged(challengeDropdown.value);
    }

    private void OnChallengeChanged(int index)
    {
        if (index < 0 || index >= _currentChallenges.Count) return;
        _selectedChallenge = _currentChallenges[index];
        RefreshStageDisplay();
        RefreshStepsUI();
    }

    private void RefreshStageDisplay()
    {
        if (_selectedChallenge == null) return;

        string subject = subjectDropdown.options[subjectDropdown.value].text;
        var allChallenges = ChallengeDataManager.Instance.GetChallengesForSubject(subject);
        int stageNum = _selectedChallenge.StageNumber;
        string stageName = _selectedChallenge.StageName;

        if (stageLabel != null)
            stageLabel.text = $"Stage {stageNum}  ·  {stageName}";

        var (done, total) = _player.GetStageProgress(subject, stageNum, allChallenges);

        if (stageProgress != null)
            stageProgress.text = total > 0 ? $"{done} / {total} challenges complete" : "";

        if (stageProgressBar != null)
            stageProgressBar.value = total > 0 ? (float)done / total : 0f;
    }

    private void RefreshStepsUI()
    {
        if (stepsContainer == null || stepButtonPrefab == null || _selectedChallenge == null) return;

        foreach (Transform child in stepsContainer) Destroy(child.gameObject);

        bool challengeUnlocked = ChallengeDataManager.Instance.IsChallengeUnlocked(_selectedChallenge.Id, _player);

        for (int i = 0; i < _selectedChallenge.Steps.Count; i++)
        {
            var step = _selectedChallenge.Steps[i];
            var go = GameObject.Instantiate(stepButtonPrefab, stepsContainer);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = $"Step {step.Number}: {step.Description}";

            bool completed = _player.CompletedSteps != null && _player.CompletedSteps.Contains(step.Id);
            bool unlocked  = challengeUnlocked && ChallengeDataManager.Instance.IsStepUnlocked(step.Id, _player);

            ColorBlock colors = btn.colors;
            if (completed)       colors.normalColor = Color.green;
            else if (unlocked)   colors.normalColor = Color.yellow;
            else                { colors.normalColor = Color.red; btn.interactable = false; }
            btn.colors = colors;

            int stepNumber = step.Number;
            string stepId  = step.Id;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnStepSelected(_selectedChallenge, stepNumber, stepId));
        }
    }

    private void OnStepSelected(Challenge challenge, int stepNumber, string stepId)
    {
        Debug.Log($"[ChallengeSelectUI] Selected {challenge.Slug} step {stepNumber} ({stepId})");

        string subject = subjectDropdown.options[subjectDropdown.value].text;
        _player.CurrentSubject = subject;
        _player.SelectStep(challenge.Slug, stepNumber, stepId);

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
