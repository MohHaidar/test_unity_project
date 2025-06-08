using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PlayerProfile
{
    public string Name;
    public int Level;
    public string Achievements;
    public string AvatarPath;
    public int PlayerCoins;
    public int MMR;
    public int TotalScore;

    public PlayerProfile(
        string name,
        int level,
        string achievements,
        string avatarPath,
        int playerCoins,
        int mmr,
        int totalscore
    )
    {
        Name = name;
        Level = level;
        Achievements = achievements;
        AvatarPath = avatarPath;
        PlayerCoins = playerCoins;
        MMR = mmr;
        TotalScore = totalscore;
    }
    public static PlayerProfile GetFromMemory()
    {
        return new PlayerProfile(
        PlayerPrefs.GetString("PlayerName", "Guest"),
        PlayerPrefs.GetInt("PlayerLevel", 1),
        PlayerPrefs.GetString("PlayerAchievements", ""),
        PlayerPrefs.GetString("PlayerAvatar", ""),
        PlayerPrefs.GetInt("PlayerCoins", 0),
        PlayerPrefs.GetInt("MMR", 1200),
        PlayerPrefs.GetInt("TotalScore", 0)
        );
    }
}