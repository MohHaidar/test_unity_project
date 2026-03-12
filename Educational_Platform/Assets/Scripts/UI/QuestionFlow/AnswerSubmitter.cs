using UnityEngine;

/// <summary>
/// Handles answer submission from UI.
/// Stores selected answer and notifies listeners when ready.
/// </summary>
public class AnswerSubmitter : MonoBehaviour
{
    private string _selectedAnswer;
    private float _answerSubmitTime;
    private bool _answerReady = false;

    public string SelectedAnswer => _selectedAnswer;
    public float AnswerSubmitTime => _answerSubmitTime;
    public bool IsAnswerReady => _answerReady;

    // Event fired when answer is submitted
    public delegate void OnAnswerSubmittedDelegate(string answer, float timeTaken);
    public event OnAnswerSubmittedDelegate OnAnswerSubmitted;

    /// <summary>
    /// Records the student's selected answer.
    /// Called by QuestionDisplay when a button is clicked.
    /// </summary>
    public void SubmitAnswer(string answer)
    {
        if (string.IsNullOrEmpty(answer))
        {
            Debug.LogError("[AnswerSubmitter] Answer is null or empty");
            return;
        }

        _selectedAnswer = answer;
        _answerSubmitTime = Time.time;
        _answerReady = true;

        Debug.Log($"[AnswerSubmitter] Answer submitted: {answer}");

        // Fire event for listeners
        OnAnswerSubmitted?.Invoke(answer, _answerSubmitTime);
    }

    /// <summary>
    /// Resets state for the next question.
    /// </summary>
    public void ResetForNextQuestion()
    {
        _selectedAnswer = null;
        _answerSubmitTime = 0;
        _answerReady = false;
    }
}
