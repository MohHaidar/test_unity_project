
using System;
using System.Collections.Generic;
using System.Linq;

public static class Evaluation
{
    public static double CalculateAccuracy(List<bool> history)
    {
        if (history.Count == 0) return 0;
        int correct = history.Count(x => x);
        return (double)correct / history.Count;
    }

    public static int AttemptsToMastery(List<bool> history)
    {
        int streak = 0;
        for (int i = 0; i < history.Count; i++)
        {
            streak = history[i] ? streak + 1 : 0;
            if (streak == 5)
                return i + 1;
        }
        return history.Count + 1;
    }

    public static double CalculateStreakStability(List<bool> history)
    {
        List<int> streaks = new List<int>();
        int current = 0;

        foreach (var result in history)
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

    public static double CalculateTimeEfficiency(List<Attempt> history)
    {
        var correctTimes = history.Where(x => x.Success).Select(x => x.TimeElapsedSeconds).ToList();
        if (!correctTimes.Any()) return 0;

        double avgTime = correctTimes.Average();
        return Math.Clamp(1.0 - ((avgTime - 3) / 7.0), 0.0, 1.0);
    }

    public static PerformanceReport GeneratePerformanceReport(List<Attempt> history)
    {
        List<bool> resultHistory = history.Select(a => a.Success).ToList();

        double accuracy = CalculateAccuracy(resultHistory);
        int attempts_to_mastery = AttemptsToMastery(resultHistory);
        double streak_stability = CalculateStreakStability(resultHistory);
        double time_efficiency = CalculateTimeEfficiency(history);

        double masterySpeedScore = 1.0 - Math.Min(attempts_to_mastery - 5, 10) / 10.0;
        masterySpeedScore = Math.Clamp(masterySpeedScore, 0.0, 1.0);

        double confidence_score = (
            0.3 * accuracy +
            0.25 * attempts_to_mastery +
            0.25 * streak_stability +
            0.2 * time_efficiency
        );

        PerformanceReport report = new PerformanceReport
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

