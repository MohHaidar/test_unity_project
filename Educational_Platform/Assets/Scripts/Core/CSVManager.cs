using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System;
using System.Linq;

public static class PlayerCSVManager
{
    private static string csvFilePath = "Assets/Data/PlayerProfiles.csv";
    private static PlayerProfile currentPlayer;

    public static PlayerProfile QueryPlayerProfile(string playerName)
    {
        if (File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);

            foreach (string line in lines)
            {
                string[] values = line.Split(',');

                if (values[0] == playerName)
                {
                    return new PlayerProfile(
                        values[0],
                        int.Parse(values[1]),
                        values[2],
                        values[3],
                        int.Parse(values[4]),
                        int.Parse(values[5]),
                        int.Parse(values[6])
                    );
                }
            }
        }
        else
        {
            Debug.LogError("CSV file not found at " + csvFilePath);
        }

        return null;
    }

    public static void LoadPlayerToMemory(string playerName)
    {
        currentPlayer = QueryPlayerProfile(playerName);

        if (currentPlayer != null)
        {
            PlayerPrefs.SetString("PlayerName", currentPlayer.Name);
            PlayerPrefs.SetInt("PlayerLevel", currentPlayer.Level);
            PlayerPrefs.SetString("PlayerAchievements", currentPlayer.Achievements);
            PlayerPrefs.SetString("PlayerAvatar", currentPlayer.AvatarPath);
            PlayerPrefs.SetInt("PlayerCoins", currentPlayer.PlayerCoins);
            PlayerPrefs.SetInt("MMR", currentPlayer.MMR);
            PlayerPrefs.SetInt("TotalScore", currentPlayer.TotalScore);

            Debug.Log("Player data loaded.");
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }

    public static void UpdatePlayerProfile(PlayerProfile profile)
    {

        string[] lines = File.ReadAllLines("Assets/Data/PlayerProfiles.csv");
        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values[0] == profile.Name)
            {
                lines[i] = string.Join(",", new string[] {
                profile.Name,
                profile.Level.ToString(),
                profile.Achievements,
                profile.AvatarPath,
                profile.PlayerCoins.ToString(),
                profile.MMR.ToString(),
                profile.TotalScore.ToString()
            });
                break;
            }
        }
        File.WriteAllLines("Assets/Data/PlayerProfiles.csv", lines);
    }
}

public static class CooldownCSVManager
{
    private static string filePath = "Assets/Data/ChallengeCooldowns.csv";

    public static List<ChallengeCooldownRow> LoadAllCooldowns()
    {
        var list = new List<ChallengeCooldownRow>();

        if (!File.Exists(filePath))
            return list;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');

            list.Add(new ChallengeCooldownRow
            {
                PlayerName = parts[0],
                ChallengeName = parts[1],
                Completions = int.Parse(parts[2]),
                LastClaimedISO = parts[3],
                WaitDays = int.Parse(parts[4])
            });
        }

        return list;
    }

    public static void SaveAllCooldowns(List<ChallengeCooldownRow> cooldowns)
    {
        var lines = new List<string>
        {
            "PlayerName,ChallengeName,Completions,LastClaimedISO,WaitDays"
        };

        foreach (var c in cooldowns)
        {
            lines.Add($"{c.PlayerName},{c.ChallengeName},{c.Completions},{c.LastClaimedISO},{c.WaitDays}");
        }

        File.WriteAllLines(filePath, lines);
    }

    public static void SaveOrUpdateCooldown(ChallengeCooldownRow newCooldown)
    {
        var all = LoadAllCooldowns();

        int index = all.FindIndex(c =>
            c.PlayerName == newCooldown.PlayerName &&
            c.ChallengeName == newCooldown.ChallengeName);

        if (index >= 0)
            all[index] = newCooldown;
        else
            all.Add(newCooldown);

        SaveAllCooldowns(all);
    }

    public static ChallengeCooldownRow GetCooldown(string playerName, string challengeName)
    {
        var allCooldowns = LoadAllCooldowns();

        return allCooldowns.Find(c =>
            c.PlayerName.Equals(playerName, System.StringComparison.OrdinalIgnoreCase) &&
            c.ChallengeName.Equals(challengeName, System.StringComparison.OrdinalIgnoreCase));
    }
}

public static class ChallengeLogger
{
    private static string logPath = "Assets/Data/ChallengeLogs.csv";

    public static void LogResult(string playerName, Challenge challenge, int score, int mmrBefore, int mmrAfter)
    {
        string result = string.Join(",", new[] {
            playerName,
            challenge.ChallengeName,
            score.ToString(),
            challenge.GetAverageTimePerQuestion().ToString("F2"),
            mmrBefore.ToString(),
            mmrAfter.ToString(),
            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
        // Debug.Log(result);
        File.AppendAllLines(logPath, new[] { result });
    }
}

public static class InventoryCSVManager
{
    private static string filePath = "Assets/Data/PlayerInventory.csv";

    public static List<InventoryItem> LoadAllInventory()
    {
        var list = new List<InventoryItem>();

        if (!File.Exists(filePath))
            return list;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');

            if (parts.Length >= 5)
            {
                list.Add(new InventoryItem
                {
                    PlayerName = parts[0],
                    ItemName = parts[1],
                    Category = parts[2],
                    AcquiredDate = parts[3],
                    AcquiredFrom = parts[4]
                });
            }
        }

        return list;
    }

    public static void AddInventoryItem(InventoryItem item)
    {
        // Append to file
        bool fileExists = File.Exists(filePath);
        using (StreamWriter sw = File.AppendText(filePath))
        {
            if (!fileExists)
            {
                sw.WriteLine("PlayerName,ItemName,Category,AcquiredDate,AcquiredFrom");
            }

            sw.WriteLine($"{item.PlayerName},{item.ItemName},{item.Category},{item.AcquiredDate},{item.AcquiredFrom}");
        }
    }

    public static List<InventoryItem> GetPlayerInventory(string playerName)
    {
        return LoadAllInventory()
            .Where(item => item.PlayerName == playerName)
            .ToList();
    }
}

public static class ShopCSVManager
{
    private static string filePath = "Assets/Data/ShopItems.csv";

    public static List<ShopItem> LoadShopItems()
    {
        var list = new List<ShopItem>();

        if (!File.Exists(filePath))
            return list;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');

            list.Add(new ShopItem
            {
                ItemName = parts[0],
                Category = parts[1],
                Price = int.Parse(parts[2]),
                LimitPerPlayer = int.Parse(parts[3]),
                AvatarPath = parts[4]
            });
        }

        return list;
    }

    public static ShopItem GetShopItemByName(string itemName)
    {
        return LoadShopItems().FirstOrDefault(i => i.ItemName == itemName);
    }
}