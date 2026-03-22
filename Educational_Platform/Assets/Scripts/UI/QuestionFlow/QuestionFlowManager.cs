using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Orchestrates the main question-answer flow with step-based progression.
/// Manages: step selection → question generation → answer submission → evaluation → step completion.
/// Supports: resume, replay, and multi-step progression.
/// </summary>
public class QuestionFlowManager : MonoBehaviour
{
    [SerializeField] private int playerId = 1;
    [SerializeField] private string playerName = "Player";
    [SerializeField] private Button nextQuestionButton;
    [SerializeField] private Button stepCompleteButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI playerStatsText;
    [SerializeField] private TextMeshProUGUI stepInfoText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI expPopText;
    [SerializeField] private GameObject stepCompletePanel;
    [SerializeField] private TextMeshProUGUI stepCompleteText;
    [SerializeField] private GameObject challengeCompletePanel;
    [SerializeField] private TextMeshProUGUI challengeCompleteText;
    [SerializeField] private Button challengeCompleteButton;

    [Header("Debug Mode (disable in production)")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI debugLogText;
    [SerializeField] private Button debugWinQuestionButton;
    [SerializeField] private Button debugWinStepButton;
    [SerializeField] private Button debugWinChallengeButton;

    private Player _player;
    private Challenge _currentChallenge;
    private Step _currentStep;
    private IQuestion _currentQuestion;
    private float _questionStartTime;

    // Session tracking
    private string _sessionId;
    private int _sessionQuestions;
    private int _sessionCorrect;
    private int _sessionMaxStreak;
    private int _sessionExpEarned;
    private int _sessionCoinsEarned;
    private float _sessionMasteryStart;
    private float _lastQuestionDifficulty = -1f;

    private OllamaQuestionGenerator _questionGenerator;
    private ParlourQuestionGenerator _parlourGenerator;
    private OllamaPerformanceEvaluator _performanceEvaluator;
    private QuestionDisplay _questionDisplay;
    private AnswerSubmitter _answerSubmitter;

    private bool _isParlourChallenge = false;

    private bool _isWaitingForAnswer = false;
    private bool _isProcessingEvaluation = false;

    // Debug insta-win flags (set by button callbacks, consumed by GameLoop)
    private bool _debugForceCorrect = false;
    private bool _debugForceCompleteStep = false;
    private bool _debugForceCompleteChallenge = false;

    private void Start()
    {
        Debug.Log("[QuestionFlowManager] Starting game...");

        // Get components
        _questionDisplay = GetComponent<QuestionDisplay>();
        _answerSubmitter = GetComponent<AnswerSubmitter>();

        if (_questionDisplay == null || _answerSubmitter == null)
        {
            Debug.LogError("[QuestionFlowManager] Required components not found");
            return;
        }

        // Initialize AI
        _questionGenerator = new OllamaQuestionGenerator("gpt-oss:20b-cloud");
        _performanceEvaluator = new OllamaPerformanceEvaluator("gpt-oss:20b-cloud");

        // Check Ollama is available
        if (!new OllamaAPI().IsOllamaAvailable())
        {
            ShowStatus("ERROR: Ollama is not running! Please start Ollama first.");
            Debug.LogError("[QuestionFlowManager] Ollama is not available");
            return;
        }

        ShowStatus("Connecting to Ollama...");

        // Load player
        _player = PlayerDataManager.Instance.LoadPlayer(playerId, playerName);
        if (_player == null)
        {
            ShowStatus("ERROR: Could not load player data");
            return;
        }

        Debug.Log($"[QuestionFlowManager] Loaded player: {_player}");

        // Load challenge
        _currentChallenge = ChallengeDataManager.Instance.GetChallenge(_player.CurrentSubject, _player.CurrentChallenge);
        if (_currentChallenge == null)
        {
            ShowStatus($"ERROR: Challenge {_player.CurrentChallenge} not found");
            return;
        }

        // Detect parlour challenges (slug starts with "parlour_") — must be after challenge load
        _isParlourChallenge = _currentChallenge.Slug.StartsWith("parlour_", System.StringComparison.OrdinalIgnoreCase);
        if (_isParlourChallenge)
        {
            _parlourGenerator = new ParlourQuestionGenerator("gpt-oss:20b-cloud");
            Debug.Log($"[QuestionFlowManager] Parlour mode active for challenge: {_currentChallenge.Slug}");
        }

        // Setup buttons
        if (nextQuestionButton != null)
            nextQuestionButton.onClick.AddListener(OnNextQuestionButtonClicked);

        if (stepCompleteButton != null)
            stepCompleteButton.onClick.AddListener(OnStepCompleteButtonClicked);

        if (challengeCompleteButton != null)
            challengeCompleteButton.onClick.AddListener(OnChallengeCompleteButtonClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        // Debug panel
        if (debugPanel != null) debugPanel.SetActive(debugMode);
        if (debugMode)
        {
            if (debugWinQuestionButton  != null) debugWinQuestionButton.onClick.AddListener(OnDebugWinQuestion);
            if (debugWinStepButton      != null) debugWinStepButton.onClick.AddListener(OnDebugWinStep);
            if (debugWinChallengeButton != null) debugWinChallengeButton.onClick.AddListener(OnDebugWinChallenge);
            Debug.LogWarning("[QuestionFlowManager] ⚠ DEBUG MODE ENABLED — insta-win buttons active");
        }

        // Hide overlays/pops initially
        if (expPopText != null) expPopText.gameObject.SetActive(false);
        if (stepCompletePanel != null) stepCompletePanel.SetActive(false);
        if (challengeCompletePanel != null) challengeCompletePanel.SetActive(false);

        // Ensure current step UUID is set on player before starting
        if (_player.CurrentStepId == null)
            _player.CurrentStepId = ChallengeDataManager.STEP_ADDITION_1_ID;

        // Subscribe to answer submission
        _answerSubmitter.OnAnswerSubmitted += OnAnswerSubmitted;

        // Start the flow
        StartCoroutine(GameLoop());
    }

    /// <summary>
    /// Main game loop: step progression with question loop inside each step.
    /// </summary>
    private IEnumerator GameLoop()
    {
        while (true)
        {
            // Debug: insta-complete the whole challenge (skip all remaining steps)
            if (_debugForceCompleteChallenge)
            {
                _debugForceCompleteChallenge = false;
                // Mark all remaining steps complete
                foreach (var s in _currentChallenge.Steps)
                {
                    if (!_player.CompletedSteps.Contains(s.Id))
                    {
                        s.Status = StepStatus.Completed;
                        s.StreakCurrent = s.StreakGoal;
                        _player.MarkStepCompleted(s.Id);
                    }
                }
                Debug.LogWarning("[QuestionFlowManager] DEBUG: insta-completed all steps in challenge");
                PlayerDataManager.Instance.SavePlayer(_player);
                yield return StartCoroutine(HandleChallengeComplete());
                break;
            }

            // Get current step
            _currentStep = _currentChallenge.GetStep(_player.CurrentStep);
            if (_currentStep == null)
            {
                // All steps done — handle challenge completion then return to selection
                yield return StartCoroutine(HandleChallengeComplete());
                break;
            }

            // Ensure step is marked in progress
            _currentStep.Status = StepStatus.InProgress;

            // Sync player's CurrentStepId with the step we're actually working on
            _player.CurrentStepId = _currentStep.Id;

            // Initialize mastery for this step if not yet present
            float existingMastery = _player.GetStepMastery(_currentStep.Id);
            if (existingMastery <= 0f)
            {
                float initialMastery = Mathf.Clamp01((_currentStep.MasteryTarget + 0.30f) / 2.0f);
                _player.UpdateStepMastery(initialMastery);
                _currentStep.MasteryCurrent = initialMastery;
                PlayerDataManager.Instance.SavePlayer(_player);
            }
            else
            {
                _currentStep.MasteryCurrent = existingMastery;
            }

            // Open a session for this step
            _sessionMasteryStart = _currentStep.MasteryCurrent;
            _sessionQuestions = _sessionCorrect = _sessionMaxStreak = _sessionExpEarned = _sessionCoinsEarned = 0;
            _sessionId = null;
            _lastQuestionDifficulty = -1f;
            if (!string.IsNullOrEmpty(_player.Id))
            {
                StartCoroutine(StartSessionCoroutine());
                StartCoroutine(HeartbeatCoroutine());
            }

            UpdateStepInfo();

            // Question loop: keep asking until step is fully complete (5-streak + optional ultimate challenge)
            while (!_currentStep.IsFullyComplete)
            {
                // Debug: insta-complete the whole challenge
                if (_debugForceCompleteChallenge)
                {
                    _debugForceCompleteChallenge = false;
                    _currentStep.StreakCurrent = _currentStep.StreakGoal;
                    break;
                }

                // Ultimate Challenge is not yet implemented — auto-complete so the streak alone finishes the step
                if (_currentStep.IsStreakComplete && _currentStep.RequireUltimateChallenge && !_currentStep.UltimateChallengeCompleted)
                {
                    _currentStep.UltimateChallengeCompleted = true;
                    break;
                }

                // Debug: insta-complete this step
                if (_debugForceCompleteStep)
                {
                    _debugForceCompleteStep = false;
                    _currentStep.StreakCurrent = _currentStep.StreakGoal;
                    break;
                }

                // Generate question (route to parlour generator for parlour challenges)
                ShowStatus($"Generating question... (Streak: {_currentStep.StreakCurrent}/{_currentStep.StreakGoal})");
                _currentQuestion = _isParlourChallenge && _parlourGenerator != null
                    ? _parlourGenerator.GenerateQuestion(_player, _currentStep)
                    : _questionGenerator.GenerateQuestion(_player, _currentStep);

                // Show debug log after generation
                if (debugMode && debugLogText != null)
                    debugLogText.text = _isParlourChallenge && _parlourGenerator != null
                        ? _parlourGenerator.LastGenerationDebugLog
                        : _questionGenerator.LastGenerationDebugLog;

                if (_currentQuestion == null)
                {
                    ShowStatus("ERROR: Failed to generate question. Retrying...");
                    yield return new WaitForSeconds(3f);
                    continue;
                }

                // Display question
                _questionDisplay.DisplayQuestion(_currentQuestion);
                _questionStartTime = Time.time;
                _isWaitingForAnswer = true;
                _debugForceCorrect = false;

                UpdatePlayerStats();

                // Wait for answer or debug override
                yield return new WaitUntil(() => _answerSubmitter.IsAnswerReady || !_isWaitingForAnswer || _debugForceCorrect);

                if (!_isWaitingForAnswer && !_debugForceCorrect) continue;

                _isWaitingForAnswer = false;
                _isProcessingEvaluation = true;

                // Debug: inject the correct answer
                string studentAnswer;
                float timeTaken;
                if (_debugForceCorrect)
                {
                    _debugForceCorrect = false;
                    studentAnswer = GetCorrectAnswer(_currentQuestion);
                    timeTaken = 1f;
                    Debug.LogWarning($"[QuestionFlowManager] DEBUG: insta-win question, injecting correct answer: {studentAnswer}");
                }
                else
                {
                    studentAnswer = _answerSubmitter.SelectedAnswer;
                    timeTaken = Time.time - _questionStartTime;
                }

                Debug.Log($"[QuestionFlowManager] Answer: {studentAnswer} (Time: {timeTaken:F1}s)");

                // Evaluate answer
                ShowStatus("Evaluating answer...");
                EvaluationResult evaluation = _performanceEvaluator.Evaluate(_player, _currentStep, _currentQuestion, studentAnswer, timeTaken);

                if (evaluation == null)
                {
                    ShowStatus("ERROR: Failed to evaluate answer. Retrying...");
                    yield return new WaitForSeconds(2f);
                    _answerSubmitter.ResetForNextQuestion();
                    _isProcessingEvaluation = false;
                    continue;
                }

                // Record answer
                QuestionResult result = new QuestionResult
                {
                    QuestionText      = _currentQuestion.QuestionText,
                    StudentAnswer     = studentAnswer,
                    CorrectAnswer     = GetCorrectAnswer(_currentQuestion),
                    IsCorrect         = evaluation.IsCorrect,
                    TimeTakenSeconds  = timeTaken,
                    Difficulty        = _currentQuestion.Difficulty,
                    ErrorType         = evaluation.ErrorType,
                    AnsweredAt        = System.DateTime.Now,
                    SubjectName       = _player.CurrentSubject,
                    ChallengeSlug     = _currentChallenge?.Slug,
                    StepDescription   = _currentStep?.Description
                };

                _player.RecordAnswer(result);
                _currentStep.QuestionsCompleted++;

                // Update streak and mastery
                if (evaluation.IsCorrect)
                {
                    _currentStep.StreakCurrent++;
                }
                else
                {
                    _currentStep.StreakCurrent = 0;
                }

                float newMastery = Mathf.Clamp01(_player.GetCurrentStepMastery() + evaluation.MasteryDelta);
                _player.UpdateStepMastery(newMastery);
                _currentStep.MasteryCurrent = newMastery;

                // Award experience
                int expGain = evaluation.IsCorrect ? 5 : 1;
                int timeBonus = 0;
                int estimated = 30;
                if (_currentQuestion is MultipleChoiceQuestion mcq)
                    estimated = mcq.EstimatedTimeSeconds;

                if (evaluation.IsCorrect && timeTaken <= estimated)
                    timeBonus = 2;

                _player.AddExp(expGain + timeBonus);
                _sessionExpEarned  += expGain + timeBonus;
                _sessionQuestions++;
                if (evaluation.IsCorrect) _sessionCorrect++;
                if (_currentStep.StreakCurrent > _sessionMaxStreak) _sessionMaxStreak = _currentStep.StreakCurrent;
                Debug.Log($"[QuestionFlowManager] Awarded EXP: {expGain + timeBonus} | TotalExp: {_player.TotalExp}");

                // Persist step progress after every answer (resilience guard)
                if (!string.IsNullOrEmpty(_player.Id) && !string.IsNullOrEmpty(_currentStep.Id))
                    _ = PlayerDataManager.Instance.UpdateStepProgressAsync(
                        _player.Id, _currentStep.Id,
                        _currentStep.MasteryCurrent, _currentStep.StreakCurrent, _currentStep.QuestionsCompleted);

                // Show EXP pop indicator
                StartCoroutine(ShowExpPop(expGain + timeBonus));

                // Save to CSV
                PlayerDataManager.Instance.SavePlayer(_player);

                // Show feedback with motivation + difficulty hint (only when difficulty actually changed)
                float currentDiff = _currentQuestion.Difficulty;
                string feedbackMsg = FeedbackBuilder.Build(
                    evaluation.IsCorrect,
                    _currentStep.StreakCurrent,
                    _currentStep.StreakGoal,
                    currentDiff,
                    _lastQuestionDifficulty,
                    evaluation.IsCorrect
                        ? _currentQuestion.SkillFocus
                        : (evaluation.StudentHint ?? evaluation.ErrorExplanation));
                _lastQuestionDifficulty = currentDiff;
                _questionDisplay.ShowFeedback(evaluation.IsCorrect, feedbackMsg);

                // Show status
                ShowStatus($"{(evaluation.IsCorrect ? "[Correct]" : "[Incorrect]")} | Streak: {_currentStep.StreakCurrent}/{_currentStep.StreakGoal}");

                UpdatePlayerStats();
                UpdateStepInfo();

                _isProcessingEvaluation = false;

                // Show next button
                if (nextQuestionButton != null)
                {
                    nextQuestionButton.interactable = true;
                    nextQuestionButton.gameObject.SetActive(true);
                }

                // Wait for user to click "Next Question"
                yield return new WaitUntil(() => !_isProcessingEvaluation && nextQuestionButton != null && !nextQuestionButton.gameObject.activeSelf);

                // Reset for next iteration
                _answerSubmitter.ResetForNextQuestion();
                _questionDisplay.ClearDisplay();
            }

            // Step is now complete!
            _currentStep.Status = StepStatus.Completed;

            // Award completion rewards
            bool isFirstCompletion = !_player.CompletedSteps.Contains(_currentStep.Id);
            int completionCoins = 0;
            int completionExp   = 0;
            if (isFirstCompletion)
            {
                completionCoins = 50;
                completionExp   = 50;
                _player.AddCoins(completionCoins);
                _player.AddExp(completionExp);
                _player.MarkStepCompleted(_currentStep.Id);
                Debug.Log($"[QuestionFlowManager] Completion rewards: {completionExp} EXP, {completionCoins} Coins for step {_currentStep.Id}");
            }
            _sessionCoinsEarned += completionCoins;
            _sessionExpEarned   += completionExp;

            // Persist final step progress as completed
            if (!string.IsNullOrEmpty(_player.Id) && !string.IsNullOrEmpty(_currentStep.Id))
                _ = PlayerDataManager.Instance.UpdateStepProgressAsync(
                    _player.Id, _currentStep.Id,
                    _currentStep.MasteryCurrent, _currentStep.StreakCurrent, _currentStep.QuestionsCompleted,
                    stepCompleted: true);

            PlayerDataManager.Instance.SavePlayer(_player);

            // End session
            if (!string.IsNullOrEmpty(_sessionId))
                _ = PlayerDataManager.Instance.EndSessionAsync(
                    _sessionId, _sessionQuestions, _sessionCorrect, _sessionMaxStreak,
                    _currentStep.MasteryCurrent, _sessionExpEarned, _sessionCoinsEarned, stepCompleted: true);
            _sessionId = null;

            // Show congratulations overlay
            ShowStepCompleteOverlay(completionExp, completionCoins, isFirstCompletion);

            if (stepCompleteButton != null)
                stepCompleteButton.gameObject.SetActive(true);

            // Wait for user to advance
            yield return new WaitUntil(() => stepCompleteButton == null || !stepCompleteButton.gameObject.activeSelf);

            // Hide overlay and move to next step
            if (stepCompletePanel != null) stepCompletePanel.SetActive(false);
            _player.AdvanceToNextStep();
            PlayerDataManager.Instance.SavePlayer(_player);
            _answerSubmitter.ResetForNextQuestion();
            _questionDisplay.ClearDisplay();

            if (stepCompleteButton != null)
                stepCompleteButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gets the correct answer from any IQuestion type.
    /// </summary>
    private string GetCorrectAnswer(IQuestion question)
    {
        if (question is MultipleChoiceQuestion mcQuestion)
            return mcQuestion.CorrectAnswer;

        return "unknown";
    }

    /// <summary>
    /// Called when answer is submitted via button click.
    /// </summary>
    private void OnAnswerSubmitted(string answer, float timeTaken)
    {
        Debug.Log($"[QuestionFlowManager] Answer received: {answer}");
    }

    /// <summary>
    /// Called when "Next Question" button is clicked.
    /// </summary>
    private void OnNextQuestionButtonClicked()
    {
        if (nextQuestionButton != null)
            nextQuestionButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when "Continue" button on step complete overlay is clicked.
    /// </summary>
    private void OnStepCompleteButtonClicked()
    {
        if (stepCompleteButton != null)
            stepCompleteButton.gameObject.SetActive(false);
    }

    // ─── Debug insta-win handlers ─────────────────────────────────────────────

    private void OnDebugWinQuestion()
    {
        if (!debugMode) return;
        Debug.LogWarning("[QuestionFlowManager] DEBUG: Win Question pressed");
        _isWaitingForAnswer = false;
        _debugForceCorrect = true;
    }

    private void OnDebugWinStep()
    {
        if (!debugMode) return;
        Debug.LogWarning("[QuestionFlowManager] DEBUG: Win Step pressed");
        _isWaitingForAnswer = false;
        _debugForceCompleteStep = true;
    }

    private void OnDebugWinChallenge()
    {
        if (!debugMode) return;
        Debug.LogWarning("[QuestionFlowManager] DEBUG: Win Challenge pressed");
        _isWaitingForAnswer = false;
        _debugForceCompleteChallenge = true;
    }

    private bool _challengeCompleteAcknowledged = false;

    private IEnumerator HandleChallengeComplete()
    {
        const int CHALLENGE_BONUS_EXP   = 200;
        const int CHALLENGE_BONUS_COINS = 100;

        // First completion = at least one step was NOT already in CompletedSteps before this run
        bool isFirstCompletion = !_currentChallenge.Steps.TrueForAll(s => _player.CompletedSteps.Contains(s.Id));

        if (isFirstCompletion)
        {
            _player.AddExp(CHALLENGE_BONUS_EXP);
            _player.AddCoins(CHALLENGE_BONUS_COINS);
        }

        // Persist challenge completion in DB
        if (!string.IsNullOrEmpty(_player.Id))
            _ = PlayerDataManager.Instance.MarkChallengeCompletedAsync(_player.Id, _currentChallenge.Id);

        PlayerDataManager.Instance.SavePlayer(_player);

        // Show challenge complete overlay
        ShowChallengeCompleteOverlay(CHALLENGE_BONUS_EXP, CHALLENGE_BONUS_COINS, isFirstCompletion);

        _challengeCompleteAcknowledged = false;
        yield return new WaitUntil(() => _challengeCompleteAcknowledged);

        // Return to challenge selection
        SceneManager.LoadScene("ChallengeSelect");
    }

    private void ShowChallengeCompleteOverlay(int bonusExp, int bonusCoins, bool firstCompletion)
    {
        // Use dedicated panel if assigned, otherwise fall back to step complete panel
        GameObject panel = challengeCompletePanel != null ? challengeCompletePanel : stepCompletePanel;
        TextMeshProUGUI txt = challengeCompleteText != null ? challengeCompleteText : stepCompleteText;

        if (panel != null) panel.SetActive(true);
        if (txt != null)
        {
            string rewards = firstCompletion
                ? $"<color=yellow>+{bonusExp} EXP   +{bonusCoins} Coins</color>"
                : "You've already completed this challenge!";

            txt.text =
                $"<size=130%><b>Challenge Complete!</b></size>\n\n" +
                $"<b>{_currentChallenge.Name}</b> — all {_currentChallenge.TotalSteps} steps finished!\n\n" +
                $"{rewards}\n\n" +
                $"<size=85%>Total EXP: {_player.TotalExp}   Coins: {_player.Coins}</size>";
        }

        // If using the fallback panel, repurpose its button
        if (challengeCompleteButton == null && stepCompleteButton != null)
        {
            stepCompleteButton.onClick.RemoveAllListeners();
            stepCompleteButton.onClick.AddListener(OnChallengeCompleteButtonClicked);
            stepCompleteButton.gameObject.SetActive(true);
        }
        else if (challengeCompleteButton != null)
        {
            challengeCompleteButton.gameObject.SetActive(true);
        }

        ShowStatus("");
    }

    private void OnChallengeCompleteButtonClicked()
    {
        if (challengeCompletePanel != null) challengeCompletePanel.SetActive(false);
        if (stepCompletePanel != null) stepCompletePanel.SetActive(false);
        _challengeCompleteAcknowledged = true;
    }

    private void OnBackButtonClicked()
    {
        if (!string.IsNullOrEmpty(_sessionId))
            _ = PlayerDataManager.Instance.EndSessionAsync(
                _sessionId, _sessionQuestions, _sessionCorrect, _sessionMaxStreak,
                _currentStep != null ? _currentStep.MasteryCurrent : 0f,
                _sessionExpEarned, _sessionCoinsEarned, stepCompleted: false);
        _sessionId = null;
        PlayerDataManager.Instance.SavePlayer(_player);
        SceneManager.LoadScene("ChallengeSelect");
    }

    private void OnApplicationPause(bool pausing)
    {
        if (!pausing || string.IsNullOrEmpty(_sessionId)) return;
        // Fire-and-forget: persist final state before app backgrounds
        if (_player != null && _currentStep != null && !string.IsNullOrEmpty(_player.Id))
            _ = PlayerDataManager.Instance.UpdateStepProgressAsync(
                _player.Id, _currentStep.Id,
                _currentStep.MasteryCurrent, _currentStep.StreakCurrent, _currentStep.QuestionsCompleted);
        _ = PlayerDataManager.Instance.EndSessionAsync(
            _sessionId, _sessionQuestions, _sessionCorrect, _sessionMaxStreak,
            _currentStep != null ? _currentStep.MasteryCurrent : 0f,
            _sessionExpEarned, _sessionCoinsEarned, stepCompleted: false);
        _sessionId = null;
    }

    private IEnumerator StartSessionCoroutine()
    {
        var task = PlayerDataManager.Instance.StartSessionAsync(_player.Id, _currentStep.Id, _sessionMasteryStart);
        yield return new UnityEngine.WaitUntil(() => task.IsCompleted);
        _sessionId = task.Result;
    }

    private IEnumerator HeartbeatCoroutine()
    {
        var wait = new WaitForSeconds(30f);
        while (_currentStep != null && _currentStep.Status != StepStatus.Completed)
        {
            yield return wait;
            if (!string.IsNullOrEmpty(_sessionId))
                _ = PlayerDataManager.Instance.HeartbeatAsync(_sessionId);
        }
    }

    /// <summary>
    /// Briefly shows "+N EXP" near the feedback area then hides it.
    /// </summary>
    private IEnumerator ShowExpPop(int amount)
    {
        if (expPopText == null) yield break;
        expPopText.text = $"<color=yellow>+{amount} EXP</color>";
        expPopText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.8f);
        expPopText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the congratulations overlay with earned rewards and any newly unlocked challenges.
    /// </summary>
    private void ShowStepCompleteOverlay(int expEarned, int coinsEarned, bool firstCompletion)
    {
        if (stepCompletePanel != null) stepCompletePanel.SetActive(true);
        if (stepCompleteText != null)
        {
            string title = firstCompletion ? "🎉 Step Complete!" : "✓ Step Replayed!";
            string rewards = firstCompletion
                ? $"+{expEarned} EXP   +{coinsEarned} Coins"
                : "No rewards (already completed)";

            string unlockLine = "";
            if (firstCompletion)
            {
                var unlocked = ChallengeDataManager.Instance.GetChallengesJustUnlockedByStep(_currentStep.Id, _player);
                if (unlocked.Count > 0)
                {
                    var names = string.Join(", ", unlocked.ConvertAll(c => c.Name));
                    unlockLine = $"\n<color=#7FFF00>🔓 Unlocked: {names}</color>";
                }
            }

            stepCompleteText.text =
                $"<b>{title}</b>\n" +
                $"Step {_currentStep.Number}: {_currentStep.Description}\n\n" +
                $"<color=yellow>{rewards}</color>" +
                unlockLine + "\n\n" +
                $"Total EXP: {_player.TotalExp}   Coins: {_player.Coins}";
        }
        ShowStatus("");
    }

    /// <summary>
    /// Updates on-screen player stats display.
    /// </summary>
    private void UpdatePlayerStats()
    {
        if (playerStatsText == null || _player == null) return;

        int completedCount = _player.CompletedSteps != null ? _player.CompletedSteps.Count : 0;

        playerStatsText.text = $@"<b>{_player.Name}</b>
Subject: {_player.CurrentSubject}
Challenge: {_player.CurrentChallenge}
EXP: {_player.TotalExp}
Coins: {_player.Coins}
Completed steps: {completedCount}";
    }

    /// <summary>
    /// Updates on-screen step info display.
    /// </summary>
    private void UpdateStepInfo()
    {
        if (stepInfoText == null || _currentStep == null) return;

        string phaseText = _currentStep.GetCurrentPhase().ToString();
        stepInfoText.text = $@"<b>Step {_currentStep.Number}: {_currentStep.Description}</b>
Streak: {_currentStep.StreakCurrent}/{_currentStep.StreakGoal}
Mastery: {_currentStep.MasteryCurrent:F2} | Target: {_currentStep.MasteryTarget:F2}
Phase: {phaseText}
Questions: {_currentStep.QuestionsCompleted}";
    }

    /// <summary>
    /// Shows status message on screen.
    /// </summary>
    private void ShowStatus(string message)
    {
        if (statusText == null) return;
        statusText.text = message;
        Debug.Log($"[QuestionFlowManager] Status: {message}");
    }

    private void OnDestroy()
    {
        if (_answerSubmitter != null)
            _answerSubmitter.OnAnswerSubmitted -= OnAnswerSubmitted;
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}
