using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages player data persistence via Supabase.
/// Synchronous public API is cache-based (backward compatible with all callers).
/// Call LoadPlayerAsync() once at session start to populate the cache from the cloud.
/// SavePlayer() updates the cache immediately and fires an async write to Supabase.
/// </summary>
public class PlayerDataManager
{
    private static PlayerDataManager _instance;
    private Dictionary<int, Player> _playerCache = new Dictionary<int, Player>();

    public static PlayerDataManager Instance
    {
        get { if (_instance == null) _instance = new PlayerDataManager(); return _instance; }
    }

    // ─── Synchronous API (cache-based, backward compatible) ──────────────────

    /// <summary>
    /// Returns player from cache. Returns a default player if not yet loaded.
    /// Always call LoadPlayerAsync() before relying on this method.
    /// </summary>
    public Player LoadPlayer(int playerId, string playerName = "Player")
    {
        if (_playerCache.ContainsKey(playerId))
            return _playerCache[playerId];

        Debug.LogWarning($"[PlayerDataManager] LoadPlayer({playerId}) called before LoadPlayerAsync. Call LoadPlayerAsync first.");
        var fallback = CreateDefaultPlayer(playerId, playerName);
        _playerCache[playerId] = fallback;
        return fallback;
    }

    /// <summary>
    /// Updates cache immediately and fires async save to Supabase.
    /// </summary>
    public void SavePlayer(Player player)
    {
        if (player == null) return;
        _playerCache[player.Id] = player;
        _ = SavePlayerAsync(player);
    }

    /// <summary>Returns all currently cached players.</summary>
    public List<Player> GetAllPlayers() => new List<Player>(_playerCache.Values);

    public void ClearCache()
    {
        _playerCache.Clear();
        Debug.Log("[PlayerDataManager] Cache cleared");
    }

    public string GetDataFilePath() => "Supabase (cloud)";

    // ─── Async API (Supabase) ─────────────────────────────────────────────────

    /// <summary>
    /// Loads player from Supabase and populates the cache.
    /// Call this once at session start before any synchronous LoadPlayer() calls.
    /// </summary>
    public async Task<Player> LoadPlayerAsync(int playerId, string playerName = "Player")
    {
        if (_playerCache.ContainsKey(playerId))
            return _playerCache[playerId];

        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogError("[PlayerDataManager] SupabaseClient not ready. Add SupabaseClient to a scene GameObject.");
            var fallback = CreateDefaultPlayer(playerId, playerName);
            _playerCache[playerId] = fallback;
            return fallback;
        }

        try
        {
            string json = await client.GetAsync("players", $"id=eq.{playerId}&select=*");
            var rows = JsonHelper.FromJsonArray<PlayerRow>(json);
            Player player;

            if (rows == null || rows.Length == 0)
            {
                player = CreateDefaultPlayer(playerId, playerName);
                await SavePlayerAsync(player);
            }
            else
            {
                player = RowToPlayer(rows[0]);

                string masteryJson = await client.GetAsync("player_step_mastery", $"player_id=eq.{playerId}&select=*");
                var masteryRows = JsonHelper.FromJsonArray<MasteryRow>(masteryJson);
                if (masteryRows != null)
                    foreach (var m in masteryRows)
                        player.MasteryByStep[m.step_key] = m.mastery;

                string completedJson = await client.GetAsync("player_completed_steps", $"player_id=eq.{playerId}&select=step_key");
                var completedRows = JsonHelper.FromJsonArray<CompletedStepRow>(completedJson);
                if (completedRows != null)
                    foreach (var c in completedRows)
                        if (!player.CompletedSteps.Contains(c.step_key))
                            player.CompletedSteps.Add(c.step_key);
            }

            _playerCache[playerId] = player;
            Debug.Log($"[PlayerDataManager] Loaded from Supabase: {player}");
            return player;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] LoadPlayerAsync failed: {e.Message}");
            var fallback = CreateDefaultPlayer(playerId, playerName);
            _playerCache[playerId] = fallback;
            return fallback;
        }
    }

    /// <summary>Upserts the player and all related data to Supabase.</summary>
    public async Task SavePlayerAsync(Player player)
    {
        if (player == null) return;
        _playerCache[player.Id] = player;

        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogError("[PlayerDataManager] SupabaseClient not ready. Cannot save to Supabase.");
            return;
        }

        try
        {
            await client.UpsertAsync("players", PlayerToJson(player));

            foreach (var kvp in player.MasteryByStep)
            {
                string masteryJson = $"{{\"player_id\":{player.Id},\"step_key\":\"{Esc(kvp.Key)}\",\"mastery\":{kvp.Value}}}";
                await client.UpsertAsync("player_step_mastery", masteryJson);
            }

            foreach (var stepKey in player.CompletedSteps)
            {
                string completedJson = $"{{\"player_id\":{player.Id},\"step_key\":\"{Esc(stepKey)}\"}}";
                await client.UpsertAsync("player_completed_steps", completedJson);
            }

            Debug.Log($"[PlayerDataManager] Saved to Supabase: {player.Name} (ID: {player.Id})");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] SavePlayerAsync failed: {e.Message}");
        }
    }

    /// <summary>Appends a single question result to Supabase question_history.</summary>
    public async Task LogQuestionResultAsync(int playerId, QuestionResult result)
    {
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return;

        try
        {
            string json = $"{{" +
                $"\"player_id\":{playerId}," +
                $"\"question_text\":\"{Esc(result.QuestionText)}\"," +
                $"\"student_answer\":\"{Esc(result.StudentAnswer)}\"," +
                $"\"correct_answer\":\"{Esc(result.CorrectAnswer)}\"," +
                $"\"is_correct\":{result.IsCorrect.ToString().ToLower()}," +
                $"\"time_taken_sec\":{result.TimeTakenSeconds}," +
                $"\"difficulty\":{result.Difficulty}," +
                $"\"error_type\":{(result.ErrorType != null ? $"\"{Esc(result.ErrorType)}\"" : "null")}" +
                $"}}";
            await client.PostAsync("question_history", json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] LogQuestionResultAsync failed: {e.Message}");
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Player CreateDefaultPlayer(int playerId, string playerName)
    {
        var player = new Player
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
        float initialMastery = Mathf.Clamp01((new Step().MasteryTarget + 0.30f) / 2.0f);
        player.MasteryByStep[$"{player.CurrentSubject}:{player.CurrentChallenge}:{player.CurrentStep}"] = initialMastery;
        return player;
    }

    private Player RowToPlayer(PlayerRow row) => new Player
    {
        Id = row.id,
        Name = row.name,
        CurrentSubject = row.subject,
        CurrentChallenge = row.challenge.ToLower(),
        CurrentStep = row.step,
        StreakInCurrentStep = row.streak,
        QuestionsInCurrentStep = row.questions_count,
        Coins = row.coins,
        TotalExp = row.total_exp,
        LastUpdated = string.IsNullOrEmpty(row.last_updated) ? DateTime.Now : DateTime.Parse(row.last_updated),
        MasteryByStep = new Dictionary<string, float>(),
        CompletedSteps = new List<string>(),
        QuestionHistory = new List<QuestionResult>()
    };

    private string PlayerToJson(Player p) =>
        $"{{\"id\":{p.Id},\"name\":\"{Esc(p.Name)}\",\"subject\":\"{Esc(p.CurrentSubject)}\"," +
        $"\"challenge\":\"{Esc(p.CurrentChallenge)}\",\"step\":{p.CurrentStep}," +
        $"\"streak\":{p.StreakInCurrentStep},\"questions_count\":{p.QuestionsInCurrentStep}," +
        $"\"coins\":{p.Coins},\"total_exp\":{p.TotalExp},\"last_updated\":\"{p.LastUpdated:O}\"}}";

    private static string Esc(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    [Serializable] private class PlayerRow
    {
        public int id;
        public string name;
        public string subject;
        public string challenge;
        public int step;
        public int streak;
        public int questions_count;
        public int coins;
        public int total_exp;
        public string last_updated;
    }

    [Serializable] private class MasteryRow
    {
        public int player_id;
        public string step_key;
        public float mastery;
    }

    [Serializable] private class CompletedStepRow
    {
        public string step_key;
    }
}
