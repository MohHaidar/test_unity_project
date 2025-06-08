using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ChallengeCooldownRow
{
    public string PlayerName;
    public string ChallengeName;
    public int Completions;
    public string LastClaimedISO;
    public int WaitDays;
}

[System.Serializable]
public class Question { 
    public string Prompt { get; set; }
    public string CorrectAnswer { get; set; }
    public List<string> Options { get; set; }
    public Dictionary<string, string> MatchPairs { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

[System.Serializable]
public class InventoryItem
{
    public string PlayerName;
    public string ItemName;
    public string Category;
    public string AcquiredDate;
    public string AcquiredFrom;
}

[System.Serializable]
public class ShopItem
{
    public string ItemName;
    public string Category;
    public int Price;
    public int LimitPerPlayer;
    public string AvatarPath;
}
enum Difficulty { Easy = 0, Medium = 1, Hard = 2 }
public enum StepAction
{
    CompleteStep,
    TryHard,
    TryMedium,
    DropToMedium,
    DropToEasy,
    Stay
}
public class QuestionPerformance
{
    public bool Success { get; set; }
    public float TimeElapsedSeconds { get; set; }
}
public class ActivityPerformance
{
    public double Accuracy { get; set; }
    public int AttemptsToMastery { get; set; }
    public double StreakStability { get; set; }
    public double TimeEfficiency { get; set; }
    public double ConfidenceScore { get; set; }
}
