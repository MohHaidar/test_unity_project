using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private TextMeshProUGUI playerStatsText;
    [SerializeField] private TextMeshProUGUI stepInfoText;
    [SerializeField] private TextMeshProUGUI statusText;

    private Player _player;
    private Challenge _currentChallenge;
    private Step _currentStep;
    private IQuestion _currentQuestion;
    private float _questionStartTime;

    private OllamaQuestionGenerator _questionGenerator;
    private OllamaPerformanceEvaluator _performanceEvaluator;
    private QuestionDisplay _questionDisplay;
    private AnswerSubmitter _answerSubmitter;

    private bool _isWaitingForAnswer = false;
    private bool _isProcessingEvaluation = false;

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
        _questionGenerator = new OllamaQuestionGenerator("gemma3:4b");
        _performanceEvaluator = new OllamaPerformanceEvaluator("gemma3:4b");

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

        // Setup buttons
        if (nextQuestionButton != null)
            nextQuestionButton.onClick.AddListener(OnNextQuestionButtonClicked);

        if (stepCompleteButton != null)
            stepCompleteButton.onClick.AddListener(OnStepCompleteButtonClicked);

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
            // Get current step
            _currentStep = _currentChallenge.GetStep(_player.CurrentStep);
            if (_currentStep == null)
            {
                // Challenge complete!
                ShowStatus($"🎉 {_currentChallenge.Name} Complete! All steps finished.");
                if (stepCompleteButton != null) stepCompleteButton.gameObject.SetActive(true);
                yield return new WaitUntil(() => stepCompleteButton == null || !stepCompleteButton.gameObject.activeSelf);
                break;
            }

            _currentStep.Status = StepStatus.InProgress;
            UpdateStepInfo();

            // Question loop: keep asking until step is fully complete (5-streak + optional ultimate challenge)
            while (!_currentStep.IsFullyComplete)
            {
                // Generate question
                ShowStatus($"Generating question... (Streak: {_currentStep.StreakCurrent}/{_currentStep.StreakGoal})");
                _currentQuestion = _questionGenerator.GenerateQuestion(_player, _currentStep);

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

                UpdatePlayerStats();

                // Wait for answer
                yield return new WaitUntil(() => _answerSubmitter.IsAnswerReady || !_isWaitingForAnswer);

                if (!_isWaitingForAnswer) continue; // User clicked next without answering

                _isWaitingForAnswer = false;
                _isProcessingEvaluation = true;

                // Get answer and time
                string studentAnswer = _answerSubmitter.SelectedAnswer;
                float timeTaken = Time.time - _questionStartTime;

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
                    QuestionText = _currentQuestion.QuestionText,
                    StudentAnswer = studentAnswer,
                    CorrectAnswer = GetCorrectAnswer(_currentQuestion),
                    IsCorrect = evaluation.IsCorrect,
                    TimeTakenSeconds = timeTaken,
                    Difficulty = _currentQuestion.Difficulty,
                    ErrorType = evaluation.ErrorType,
                    AnsweredAt = System.DateTime.Now
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

                // Save to CSV
                PlayerDataManager.Instance.SavePlayer(_player);

                // Show feedback
                string feedbackMsg = evaluation.IsCorrect ? _currentQuestion.SkillFocus : evaluation.ErrorExplanation;
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
            ShowStatus($"✓ Step {_currentStep.Number} Complete! ({_currentStep.Description})");

            if (stepCompleteButton != null)
            {
                stepCompleteButton.gameObject.SetActive(true);
            }

            // Wait for user to advance to next step
            yield return new WaitUntil(() => stepCompleteButton == null || !stepCompleteButton.gameObject.activeSelf);

            // Move to next step
            _player.AdvanceToNextStep();
            PlayerDataManager.Instance.SavePlayer(_player);
            _answerSubmitter.ResetForNextQuestion();
            _questionDisplay.ClearDisplay();

            if (stepCompleteButton != null)
            {
                stepCompleteButton.gameObject.SetActive(false);
            }
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
        Debug.Log("[QuestionFlowManager] Next question button clicked");
        if (nextQuestionButton != null)
        {
            nextQuestionButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when "Step Complete / Next Step" button is clicked.
    /// </summary>
    private void OnStepCompleteButtonClicked()
    {
        Debug.Log("[QuestionFlowManager] Step complete button clicked");
        if (stepCompleteButton != null)
        {
            stepCompleteButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates on-screen player stats display.
    /// </summary>
    private void UpdatePlayerStats()
    {
        if (playerStatsText == null || _player == null) return;

        playerStatsText.text = $@"<b>{_player.Name}</b>
Subject: {_player.CurrentSubject}
Challenge: {_player.CurrentChallenge}";
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
        {
            _answerSubmitter.OnAnswerSubmitted -= OnAnswerSubmitted;
        }
    }
}
