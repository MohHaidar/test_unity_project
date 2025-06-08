
using System;
using System.Collections.Generic;
using System.Linq;

public static class Evaluation
{
    public static double CalculateAccuracy(List<bool> AnswersHistory)
    {
        if (AnswersHistory.Count == 0) return 0;
        int correct = AnswersHistory.Count(x => x);
        return (double)correct / AnswersHistory.Count;
    }

    public static int AttemptsToMastery(List<bool> AnswersHistory)
    {
        int streak = 0;
        for (int i = 0; i < AnswersHistory.Count; i++)
        {
            streak = AnswersHistory[i] ? streak + 1 : 0;
            if (streak == 5)
                return i + 1;
        }
        return AnswersHistory.Count + 1;
    }

    public static double CalculateStreakStability(List<bool> AnswersHistory)
    {
        List<int> streaks = new List<int>();
        int current = 0;

        foreach (var result in AnswersHistory)
        {
            if (result) current++;
            else
            {
                if (current > 0) streaks.Add(current);
                current = 0;
            }
        }
        if (current > 0) streaks.Add(current);

        if (streaks.Count == 0) return 0;
        return streaks.Average() / 5.0;
    }

    public static double CalculateTimeEfficiency(List<QuestionPerformance> PerformanceHistory)
    {
        var correctTimes = PerformanceHistory
            .Where(entry => entry.Success)
            .Select(entry => entry.TimeElapsedSeconds)
            .ToList();

        if (!correctTimes.Any())
            return 0;

        double avgTime = correctTimes.Average();
        return Math.Clamp(1.0 - ((avgTime - 3) / 7.0), 0.0, 1.0);
    }

    public static ActivityPerformance GeneratePerformanceReport(List<QuestionPerformance> PerformanceHistory)
    {
        List<bool> AnswersHistory = PerformanceHistory.Select(a => a.Success).ToList();

        double accuracy = CalculateAccuracy(AnswersHistory);
        int attempts_to_mastery = AttemptsToMastery(AnswersHistory);
        double streak_stability = CalculateStreakStability(AnswersHistory);
        double time_efficiency = CalculateTimeEfficiency(PerformanceHistory);

        double masterySpeedScore = 1.0 - Math.Min(attempts_to_mastery - 5, 10) / 10.0;
        masterySpeedScore = Math.Clamp(masterySpeedScore, 0.0, 1.0);

        double confidence_score = (
            0.3 * accuracy +
            0.25 * attempts_to_mastery +
            0.25 * streak_stability +
            0.2 * time_efficiency
        );

        ActivityPerformance report = new ActivityPerformance
        {
            Accuracy = accuracy,
            AttemptsToMastery = attempts_to_mastery,
            StreakStability = streak_stability,
            TimeEfficiency = time_efficiency,
            ConfidenceScore = confidence_score
        };

        return report;
    }

}

