using UnityEngine;

/// <summary>
/// Generates short, friendly feedback messages shown after each question answer.
/// Combines a motivational reaction with a difficulty-direction hint.
/// </summary>
public static class FeedbackBuilder
{
    // ── Correct-answer reactions keyed by streak milestone ──────────────────
    private static readonly string[] _correctGeneral =
    {
        "Nice work! ✅",
        "That's right! 👍",
        "Correct! Well done.",
        "Nailed it! ✅",
        "You got it! 😊",
    };

    private static readonly string[] _correctStreak =
    {
        "Keep the streak alive! 🔥",
        "You're on a roll! 🔥",
        "Unstoppable! 🔥",
    };

    private static readonly string[] _correctFinal =
    {
        "Streak complete! Amazing! 🎉",
        "You did it! Step cleared! 🎉",
        "Perfect streak! 🌟",
    };

    // ── Incorrect-answer reactions ───────────────────────────────────────────
    private static readonly string[] _incorrect =
    {
        "Not quite — you'll get it! 💪",
        "Close! Keep going. 😊",
        "Oops! Let's try again. 💡",
        "Almost there! Don't give up. 🙌",
    };

    // ── Difficulty-direction hints ───────────────────────────────────────────
    private const string DiffUp   = "↑ Stepping it up a bit!";
    private const string DiffDown = "↓ Easing in next one.";

    /// <summary>
    /// Builds the full feedback string to show to the player.
    /// </summary>
    /// <param name="isCorrect">Whether the answer was correct.</param>
    /// <param name="streakCurrent">Streak AFTER this answer.</param>
    /// <param name="streakGoal">Target streak for step completion.</param>
    /// <param name="currentDifficulty">Difficulty of the question just answered.</param>
    /// <param name="previousDifficulty">Difficulty of the previous question (-1 if none).</param>
    /// <param name="explanation">Skill/error explanation from evaluator.</param>
    public static string Build(bool isCorrect, int streakCurrent, int streakGoal, float currentDifficulty, float previousDifficulty, string explanation)
    {
        string reaction   = GetReaction(isCorrect, streakCurrent, streakGoal);
        string diffHint   = GetDifficultyHint(currentDifficulty, previousDifficulty);
        string streakLine = GetStreakLine(isCorrect, streakCurrent, streakGoal);

        string color  = isCorrect ? "green" : "#FF6B6B";
        string result = isCorrect ? "Correct!" : "Not quite!";

        string diffLine = string.IsNullOrEmpty(diffHint)
            ? ""
            : $"\n<size=85%><color=#AAAAAA>{diffHint}</color></size>";

        return $"<color={color}><b>{result}</b></color>  {reaction}{diffLine}\n" +
               $"<size=80%>{streakLine}\n{explanation}</size>";
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetReaction(bool isCorrect, int streak, int goal)
    {
        if (!isCorrect)
            return _incorrect[Random.Range(0, _incorrect.Length)];

        if (streak >= goal)
            return _correctFinal[Random.Range(0, _correctFinal.Length)];

        if (streak >= 3)
            return _correctStreak[Random.Range(0, _correctStreak.Length)];

        return _correctGeneral[Random.Range(0, _correctGeneral.Length)];
    }

    private static string GetDifficultyHint(float current, float previous)
    {
        // No previous question yet, or no meaningful change — show nothing
        if (previous < 0f) return "";

        float delta = current - previous;
        if (Mathf.Abs(delta) < 0.05f) return "";   // noise threshold

        return delta > 0 ? DiffUp : DiffDown;
    }

    private static string GetStreakLine(bool isCorrect, int streak, int goal)
    {
        if (!isCorrect)
        {
            return streak > 0
                ? $"Streak reset (was {streak}). You can rebuild it! 💪"
                : "No streak yet — just keep answering!";
        }

        int remaining = goal - streak;
        if (remaining <= 0) return $"🎯 Streak: {streak}/{goal} — Step complete!";
        if (remaining == 1) return $"🎯 Streak: {streak}/{goal} — One more to go!";
        return $"🎯 Streak: {streak}/{goal} — {remaining} more to complete this step.";
    }
}
