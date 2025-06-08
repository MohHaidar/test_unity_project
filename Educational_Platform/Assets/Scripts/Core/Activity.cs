using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
public class Activity
{
    public string ActivityId { get; set; }
    public int Streak = 0;
    public float TimeSpent = 0f;
    private DateTime questionStartTime;
    public List<QuestionPerformance> PerformanceHistory;
    public QuestionModule CurrentQuestion;
    public string ActivityType { get; set; }
    public string Modality { get; set; }
    public Difficulty DifficultyLevel { get; set; }
    public List<string> Tags { get; set; }

    public Func<Question> QuestionGenerator { get; set; }

    public void RefreshActivity()
    {
        Streak = 0;
        TimeSpent = 0f;
    }

    public Question GenerateQuestion()
    {
        StartQuestionTimer();
        return QuestionGenerator?.Invoke();
    }

    public bool CheckAnswer(string userAnswer)
    {
        var question = CurrentQuestion;
        if (question == null) return false;
        return question.CorrectAnswer == userAnswer;
    }

    public void StartQuestionTimer()
    {
        questionStartTime = DateTime.Now;
    }
    public float GetAverageTimePerQuestion()
    {
        return QuestionsAnswered == 0 ? 0f : TimeSpent / QuestionsAnswered;
    }
    public void SubmitAnswer(string answer)
    {
        if (CurrentQuestion == null) return;

        float timeTaken = (float)(DateTime.Now - questionStartTime).TotalSeconds;
        TimeSpent += timeTaken;
        isCorrect = CheckAnswer(answer);
        PerformanceHistory.Add(new QuestionPerformance { Success = isCorrect, TimeElapsedSeconds = timeTaken });

        if (isCorrect)
        {
            Streak++;
        }
        else
        {
            Streak = 0;
        }

    }

}