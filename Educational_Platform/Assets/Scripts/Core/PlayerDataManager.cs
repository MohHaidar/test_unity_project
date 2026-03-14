using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages player data persistence via CSV.
/// Handles loading and saving player profiles with step-based metrics.
/// </summary>
public class PlayerDataManager
{
    private static readonly string PLAYER_DATA_FILE = "player_data.csv";
    // Added fields: coins, total_exp, completed_steps_json
    private static readonly string CSV_HEADER = "player_id,name,subject,challenge,step,mastery_by_step_json,streak,questions_count,last_updated,coins,total_exp,completed_steps_json";

    // CSV indices
    private const int IDX_ID = 0;
    private const int IDX_NAME = 1;
    private const int IDX_SUBJECT = 2;
    private const int IDX_CHALLENGE = 3;
    private const int IDX_STEP = 4;
    private const int IDX_MASTERY_JSON = 5;
    private const int IDX_STREAK = 6;
    private const int IDX_QUESTIONS = 7;
    private const int IDX_UPDATED = 8;
    private const int IDX_COINS = 9;
    private const int IDX_TOTAL_EXP = 10;
    private const int IDX_COMPLETED_JSON = 11;

    private static PlayerDataManager _instance;
    private Dictionary<int, Player> _playerCache = new Dictionary<int, Player>();
    private string _playerDataPath;

    /// <summary>
    /// Singleton instance of PlayerDataManager.
    /// </summary>
    public static PlayerDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayerDataManager();
            }
            return _instance;
        }
    }

    public PlayerDataManager()
    {
        _playerDataPath = Path.Combine(Application.persistentDataPath, PLAYER_DATA_FILE);
        Debug.Log($"[PlayerDataManager] CSV Path: {_playerDataPath}");
        InitializeCSV();
    }

    /// <summary>
    /// Ensures CSV file exists with headers.
    /// </summary>
    private void InitializeCSV()
    {
        try
        {
            if (!File.Exists(_playerDataPath))
            {
                Debug.Log("[PlayerDataManager] Creating new player_data.csv");
                File.WriteAllText(_playerDataPath, CSV_HEADER + "\n");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error initializing CSV: {e.Message}");
        }
    }

    /// <summary>
    /// Loads a player by ID from CSV. Creates new player if not found.
    /// </summary>
    public Player LoadPlayer(int playerId, string playerName = "Player")
    {
        // Check cache first
        if (_playerCache.ContainsKey(playerId))
        {
            return _playerCache[playerId];
        }

        try
        {
            // Read all lines from CSV
            string[] lines = File.ReadAllLines(_playerDataPath);

            // Find matching player
            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string[] fields = lines[i].Split(',');
                if (fields.Length >= 9 && int.Parse(fields[IDX_ID]) == playerId)
                {
                    Player player = ParsePlayerFromCSV(fields);
                    // Normalize challenge name to lowercase for consistency
                    player.CurrentChallenge = player.CurrentChallenge.ToLower();
                    _playerCache[playerId] = player;
                    Debug.Log($"[PlayerDataManager] Loaded player: {player}");
                    return player;
                }
            }

            // Player not found, create new
            Player newPlayer = new Player
            {
                Id = playerId,
                Name = playerName,
                CurrentSubject = "Math",
                CurrentChallenge = "addition",
                CurrentStep = 1,
                MasteryByStep = new Dictionary<string, float>(),
                StreakInCurrentStep = 0,
                QuestionsInCurrentStep = 0,
                QuestionHistory = new List<QuestionResult>()
            };

            // Initialize mastery as the average of the step target and 30%
            // Use Step default target if specific step data is not yet loaded
            Step defaultStep = new Step();
            float initialMastery = Mathf.Clamp01((defaultStep.MasteryTarget + 0.30f) / 2.0f);
            string stepKey = $"{newPlayer.CurrentSubject}:{newPlayer.CurrentChallenge}:{newPlayer.CurrentStep}";
            newPlayer.MasteryByStep[stepKey] = initialMastery;

            _playerCache[playerId] = newPlayer;
            SavePlayer(newPlayer);
            
            Debug.Log($"[PlayerDataManager] Created new player: {newPlayer}");
            return newPlayer;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error loading player {playerId}: {e.Message}");

            // Return default player on error
            Player fallback = new Player { Id = playerId, Name = playerName };
            _playerCache[playerId] = fallback;
            return fallback;
        }
    }

    /// <summary>
    /// Saves player data to CSV (creates new row or updates existing).
    /// </summary>
    public void SavePlayer(Player player)
    {
        if (player == null) return;

        try
        {
            // Read all lines
            List<string> lines = new List<string>(File.ReadAllLines(_playerDataPath));

            // Find and update existing player, or append new
            bool found = false;
            for (int i = 1; i < lines.Count; i++) // Skip header
            {
                string[] fields = lines[i].Split(',');
                if (fields.Length >= 9 && int.Parse(fields[IDX_ID]) == player.Id)
                {
                    lines[i] = PlayerToCSVLine(player);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                lines.Add(PlayerToCSVLine(player));
            }

            // Write back to file
            File.WriteAllLines(_playerDataPath, lines);
            _playerCache[player.Id] = player;
            Debug.Log($"[PlayerDataManager] Saved player: {player.Name} (ID: {player.Id})");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error saving player {player.Id}: {e.Message}");
        }
    }

    /// <summary>
    /// Converts Player object to CSV line.
    /// </summary>
    private string PlayerToCSVLine(Player player)
    {
        // Serialize mastery dictionary to JSON
        string masteryJson = JsonUtility.ToJson(new MasteryDictWrapper { pairs = SerializeDictionary(player.MasteryByStep) });

        // Serialize completed steps list
        string completedJson = JsonUtility.ToJson(new CompletedListWrapper { items = player.CompletedSteps });

        return $"{player.Id},{player.Name},{player.CurrentSubject},{player.CurrentChallenge},{player.CurrentStep},{masteryJson},{player.StreakInCurrentStep},{player.QuestionsInCurrentStep},{player.LastUpdated:O},{player.Coins},{player.TotalExp},{completedJson}";
    }

    /// <summary>
    /// Parses a CSV line into a Player object.
    /// </summary>
    private Player ParsePlayerFromCSV(string[] fields)
    {
        // Deserialize mastery dictionary
        Dictionary<string, float> masteryByStep = new Dictionary<string, float>();
        try
        {
            if (fields.Length > IDX_MASTERY_JSON && !string.IsNullOrEmpty(fields[IDX_MASTERY_JSON]))
            {
                var wrapper = JsonUtility.FromJson<MasteryDictWrapper>(fields[IDX_MASTERY_JSON]);
                if (wrapper != null && wrapper.pairs != null)
                    masteryByStep = DeserializeDictionary(wrapper.pairs);
            }
        }
        catch
        {
            Debug.LogWarning("[PlayerDataManager] Failed to parse mastery JSON, using empty dict");
        }

        // Deserialize completed steps if present
        List<string> completedSteps = new List<string>();
        try
        {
            if (fields.Length > IDX_COMPLETED_JSON && !string.IsNullOrEmpty(fields[IDX_COMPLETED_JSON]))
            {
                var cwrap = JsonUtility.FromJson<CompletedListWrapper>(fields[IDX_COMPLETED_JSON]);
                if (cwrap != null && cwrap.items != null)
                    completedSteps = cwrap.items;
            }
        }
        catch
        {
            Debug.LogWarning("[PlayerDataManager] Failed to parse completed steps JSON, using empty list");
        }

        // Parse numeric fields with fallbacks
        int streak = 0;
        int questions = 0;
        DateTime lastUpdated = DateTime.Now;
        int coins = 0;
        int totalExp = 0;

        if (fields.Length > IDX_STREAK) int.TryParse(fields[IDX_STREAK], out streak);
        if (fields.Length > IDX_QUESTIONS) int.TryParse(fields[IDX_QUESTIONS], out questions);
        if (fields.Length > IDX_UPDATED) DateTime.TryParse(fields[IDX_UPDATED], out lastUpdated);
        if (fields.Length > IDX_COINS) int.TryParse(fields[IDX_COINS], out coins);
        if (fields.Length > IDX_TOTAL_EXP) int.TryParse(fields[IDX_TOTAL_EXP], out totalExp);

        return new Player
        {
            Id = fields.Length > IDX_ID ? int.Parse(fields[IDX_ID]) : 0,
            Name = fields.Length > IDX_NAME ? fields[IDX_NAME] : "Player",
            CurrentSubject = fields.Length > IDX_SUBJECT ? fields[IDX_SUBJECT] : "Math",
            CurrentChallenge = fields.Length > IDX_CHALLENGE ? fields[IDX_CHALLENGE] : "addition",
            CurrentStep = fields.Length > IDX_STEP && int.TryParse(fields[IDX_STEP], out int s) ? s : 1,
            MasteryByStep = masteryByStep,
            StreakInCurrentStep = streak,
            QuestionsInCurrentStep = questions,
            LastUpdated = lastUpdated,
            QuestionHistory = new List<QuestionResult>(),
            Coins = coins,
            TotalExp = totalExp,
            CompletedSteps = completedSteps
        };
    }

    /// <summary>
    /// Serializes a Dictionary to a list of key-value pairs for JSON.
    /// </summary>
    private List<MasteryPair> SerializeDictionary(Dictionary<string, float> dict)
    {
        return dict.Select(kvp => new MasteryPair { key = kvp.Key, value = kvp.Value }).ToList();
    }

    /// <summary>
    /// Deserializes a list of key-value pairs back to a Dictionary.
    /// </summary>
    private Dictionary<string, float> DeserializeDictionary(List<MasteryPair> pairs)
    {
        var dict = new Dictionary<string, float>();
        foreach (var pair in pairs)
        {
            dict[pair.key] = pair.value;
        }
        return dict;
    }

    /// <summary>
    /// Gets all players from CSV.
    /// </summary>
    public List<Player> GetAllPlayers()
    {
        List<Player> players = new List<Player>();

        try
        {
            string[] lines = File.ReadAllLines(_playerDataPath);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string[] fields = lines[i].Split(',');
                if (fields.Length >= 9)
                {
                    players.Add(ParsePlayerFromCSV(fields));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error reading all players: {e.Message}");
        }

        return players;
    }

    /// <summary>
    /// Clears the player cache (useful for testing).
    /// </summary>
    public void ClearCache()
    {
        _playerCache.Clear();
        Debug.Log("[PlayerDataManager] Cache cleared");
    }

    /// <summary>
    /// Gets the CSV file path (for debugging/inspection).
    /// </summary>
    public string GetDataFilePath()
    {
        return _playerDataPath;
    }

    // Helper classes for JSON serialization
    [System.Serializable]
    private class MasteryDictWrapper
    {
        public List<MasteryPair> pairs;
    }

    [System.Serializable]
    private class MasteryPair
    {
        public string key;
        public float value;
    }

    [System.Serializable]
    private class CompletedListWrapper
    {
        public List<string> items;
    }
}
