using System;
using System.Collections.Generic;
using UnityEngine;

public class Challenge
{
    public string ChallengeName;
    public int ChallengeMMR;
    public List<Func<QuestionModule>> Modules = new();
    public QuestionModule CurrentQuestion;
    public QuestionModule UltimateModule;
    public int CurrentIndex = 0;
    public int Streak = 0;
    public float TimeSpent = 0f;
    public int QuestionsAnswered = 0;
    public List<float> TimePerQuestion = new();
    public bool UltimateUnlocked = false;
    public bool ChallengeCompleted = false;

    private DateTime questionStartTime;

    public void StartQuestionTimer()
    {
        questionStartTime = DateTime.Now;
    }

    public void RefreshChallenge()
    {
        CurrentIndex = 0;
        Streak = 0;
        TimeSpent = 0f;
        QuestionsAnswered = 0;
        TimePerQuestion = new();
        UltimateUnlocked = false;
        ChallengeCompleted = false;
    }

    public QuestionModule GetCurrentQuestion()
    {
        if (UltimateUnlocked)
            return UltimateModule;

        if (CurrentIndex < Modules.Count)
        {
            CurrentQuestion = Modules[CurrentIndex].Invoke();

            return CurrentQuestion;
        }

        return null;
    }

    public void SubmitAnswer(string answer)
    {
        if (CurrentQuestion == null) return;

        float timeTaken = (float)(DateTime.Now - questionStartTime).TotalSeconds;
        TimeSpent += timeTaken;
        TimePerQuestion.Add(timeTaken);
        QuestionsAnswered++;

        if (CurrentQuestion.IsCorrect(answer))
        {
            Streak++;
            if (Streak >= 5)
            {
                // Streak = 0;
                CurrentIndex++;
                if (CurrentIndex >= Modules.Count)
                    UnlockUltimateChallenge();
            }
        }
        else
        {
            Streak = 0;
        }
    }

    public float GetAverageTimePerQuestion()
    {
        return QuestionsAnswered == 0 ? 0f : TimeSpent / QuestionsAnswered;
    }

    public int CalculateScore()
    {
        float avgTime = GetAverageTimePerQuestion();
        int baseScore = 100 * CurrentIndex;
        int streakBonus = Mathf.Clamp(25 * Streak, 0, 100);
        int timeBonus = avgTime < 5 ? 100 : avgTime < 10 ? 50 : 0;
        return baseScore + streakBonus + timeBonus;
    }

    public void UnlockUltimateChallenge()
    {
        UltimateUnlocked = true;
    }

    public void SubmitUltimateAnswer(string answer)
    {
        if (UltimateModule.IsCorrect(answer))
        {
            UltimateUnlocked = false;
            ChallengeCompleted = true;
        }
        else
        {
            UltimateUnlocked = false;
            CurrentIndex = Modules.Count - 1;
            Streak = 0;
            CurrentQuestion = null;
        }
    }
}
