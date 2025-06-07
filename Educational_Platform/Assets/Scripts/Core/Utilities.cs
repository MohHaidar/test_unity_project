using System;
using System.Collections.Generic;
using UnityEngine;

public static class ChallengeRewardCooldown
{
    private static readonly int[] CooldownDays = { 0, 0, 0, 1, 3, 7, 30 };

    public static bool CanClaimReward(ChallengeCooldownRow data)
    {
        DateTime last = DateTime.Parse(data.LastClaimedISO);
        return DateTime.Now >= last.AddDays(data.WaitDays);
    }

    public static void MarkClaimed(ChallengeCooldownRow data)
    {
        data.LastClaimedISO = DateTime.Now.ToString("o");
        data.Completions++;
        int index = Mathf.Clamp(data.Completions, 0, CooldownDays.Length - 1) - 1;
        data.WaitDays = CooldownDays[index];
    }

    public static ChallengeCooldownRow GetOrCreateCooldown(string playerName, string challengeName)
    {
        var data = CooldownCSVManager.GetCooldown(playerName, challengeName);
        if (data == null)
        {
            data = new ChallengeCooldownRow
            {
                PlayerName = playerName,
                ChallengeName = challengeName,
                Completions = 0,
                LastClaimedISO = DateTime.MinValue.ToString("o"),
                WaitDays = 0
            };
        }
        return data;
    }
}

public static class MMRCalculator {
    public static int CalculateNewMMR(int playerMMR, int challengeMMR, bool won) {
        int k = 32;
        float expected = 1 / (1 + Mathf.Pow(10, (challengeMMR - playerMMR) / 400f));
        float score = won ? 1 : 0;
        return Mathf.RoundToInt(playerMMR + k * (score - expected));
    }
}

public static class ChallengeLoader {
    public static Challenge SelectedChallenge;
}