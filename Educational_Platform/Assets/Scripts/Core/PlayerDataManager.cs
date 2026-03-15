using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages player data persistence via Supabase (new UUID-based schema).
/// Cache is keyed by player name (UNIQUE in DB). Sync API is cache-based.
/// Call LoadPlayerAsync() at session start. Use UpdateStepProgressAsync() after every answer.
/// </summary>
public class PlayerDataManager
{
    private static PlayerDataManager _instance;
    private Dictionary<string, Player> _playerCache = new Dictionary<string, Player>(); // keyed by name

    public static PlayerDataManager Instance
    {
        get { if (_instance == null) _instance = new PlayerDataManager(); return _instance; }
    }

    // ─── Synchronous API (cache-based, backward compatible) ──────────────────

    public Player LoadPlayer(int playerId, string playerName = "Player")
    {
        if (_playerCache.TryGetValue(playerName, out var cached)) return cached;
        Debug.LogWarning("[PlayerDataManager] LoadPlayer called before LoadPlayerAsync.");
        var fallback = CreateDefaultPlayer(playerName);
        _playerCache[playerName] = fallback;
        return fallback;
    }

    public void SavePlayer(Player player)
    {
        if (player == null) return;
        _playerCache[player.Name] = player;
        _ = SavePlayerAsync(player);
    }

    public List<Player> GetAllPlayers() => new List<Player>(_playerCache.Values);
    public void ClearCache() { _playerCache.Clear(); }
    public string GetDataFilePath() => "Supabase (cloud)";

    // ─── Async API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads player by name from Supabase. Creates a new player row if first run.
    /// Recovers any orphan sessions left open from a previous crash.
    /// </summary>
    public async Task<Player> LoadPlayerAsync(int _ignored, string playerName = "Player")
    {
        if (_playerCache.TryGetValue(playerName, out var cached)) return cached;

        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogWarning("[PlayerDataManager] SupabaseClient not ready — using offline fallback.");
            var fb = CreateDefaultPlayer(playerName);
            _playerCache[playerName] = fb;
            return fb;
        }

        try
        {
            string json = await client.GetAsync("players", $"name=eq.{Uri.EscapeDataString(playerName)}&select=*");
            var rows = JsonHelper.FromJsonArray<PlayerRow>(json);
            Player player;

            if (rows == null || rows.Length == 0)
            {
                player = CreateDefaultPlayer(playerName);
                await SavePlayerAsync(player);  // inserts, fills player.Id from response
            }
            else
            {
                player = RowToPlayer(rows[0]);
                await LoadStepProgressAsync(player, client);
            }

            // Recover any orphan sessions
            if (!string.IsNullOrEmpty(player.Id))
                await RecoverOrphanSessionsAsync(player.Id, client);

            _playerCache[playerName] = player;
            Debug.Log($"[PlayerDataManager] Loaded: {player}");
            return player;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] LoadPlayerAsync failed: {e.Message}");
            var fb = CreateDefaultPlayer(playerName);
            _playerCache[playerName] = fb;
            return fb;
        }
    }

    public async Task SavePlayerAsync(Player player)
    {
        if (player == null) return;
        _playerCache[player.Name] = player;

        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return;

        try
        {
            string playerJson = BuildPlayerJson(player);
            string response = await client.UpsertAsync("players", playerJson);

            // Capture UUID assigned by DB on first insert
            if (string.IsNullOrEmpty(player.Id))
            {
                var rows = JsonHelper.FromJsonArray<PlayerRow>(response);
                if (rows != null && rows.Length > 0) player.Id = rows[0].id;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] SavePlayerAsync failed: {e.Message}");
        }
    }

    /// <summary>
    /// Upserts a single step progress row — call after every question answer.
    /// This is the resilience guard: progress survives app crash if written here.
    /// </summary>
    public async Task UpdateStepProgressAsync(string playerId, string stepId, float mastery, int streak, int questionsTotal, bool stepCompleted = false)
    {
        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(stepId)) return;
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return;

        try
        {
            string status = stepCompleted ? "completed" : (questionsTotal > 0 ? "in_progress" : "not_started");
            string firstCompleted = stepCompleted ? $"\"{DateTime.UtcNow:O}\"" : "null";
            string json =
                $"{{\"player_id\":\"{playerId}\"," +
                $"\"step_id\":\"{stepId}\"," +
                $"\"mastery\":{mastery}," +
                $"\"best_streak\":{streak}," +
                $"\"times_completed\":{(stepCompleted ? 1 : 0)}," +
                $"\"status\":\"{status}\"," +
                $"\"first_completed_at\":{firstCompleted}," +
                $"\"last_played_at\":\"{DateTime.UtcNow:O}\"}}";
            await client.UpsertAsync("player_step_progress", json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] UpdateStepProgressAsync failed: {e.Message}");
        }
    }

    /// <summary>
    /// Opens a new play session. Returns the session UUID, or null on failure.
    /// </summary>
    public async Task<string> StartSessionAsync(string playerId, string stepId, float masteryStart)
    {
        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(stepId)) return null;
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return null;

        try
        {
            string json =
                $"{{\"player_id\":\"{playerId}\"," +
                $"\"step_id\":\"{stepId}\"," +
                $"\"status\":\"active\"," +
                $"\"mastery_start\":{masteryStart}}}";
            string response = await client.PostAsync("play_sessions", json);
            var rows = JsonHelper.FromJsonArray<SessionIdRow>(response);
            string id = rows?.Length > 0 ? rows[0].id : null;
            if (id != null) Debug.Log($"[PlayerDataManager] Session started: {id}");
            return id;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] StartSessionAsync failed: {e.Message}");
            return null;
        }
    }

    /// <summary>Updates last_heartbeat_at for the active session (call every ~30 s).</summary>
    public async Task HeartbeatAsync(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId)) return;
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return;

        try
        {
            string json = $"{{\"last_heartbeat_at\":\"{DateTime.UtcNow:O}\"}}";
            await client.PatchAsync("play_sessions", $"id=eq.{sessionId}", json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] HeartbeatAsync failed: {e.Message}");
        }
    }

    /// <summary>Closes a session with final statistics.</summary>
    public async Task EndSessionAsync(string sessionId, int questionsAnswered, int correctAnswers, int maxStreak, float masteryEnd, int expEarned, int coinsEarned, bool stepCompleted)
    {
        if (string.IsNullOrEmpty(sessionId)) return;
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady) return;

        try
        {
            string json =
                $"{{\"ended_at\":\"{DateTime.UtcNow:O}\"," +
                $"\"last_heartbeat_at\":\"{DateTime.UtcNow:O}\"," +
                $"\"status\":\"{(stepCompleted ? "completed" : "abandoned")}\"," +
                $"\"questions_answered\":{questionsAnswered}," +
                $"\"correct_answers\":{correctAnswers}," +
                $"\"max_streak_reached\":{maxStreak}," +
                $"\"mastery_end\":{masteryEnd}," +
                $"\"exp_earned\":{expEarned}," +
                $"\"coins_earned\":{coinsEarned}," +
                $"\"step_completed\":{stepCompleted.ToString().ToLower()}}}";
            await client.PatchAsync("play_sessions", $"id=eq.{sessionId}", json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] EndSessionAsync failed: {e.Message}");
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task LoadStepProgressAsync(Player player, SupabaseClient client)
    {
        try
        {
            string json = await client.GetAsync("player_step_progress", $"player_id=eq.{player.Id}&select=*");
            var rows = JsonHelper.FromJsonArray<StepProgressRow>(json);
            if (rows == null) return;

            foreach (var row in rows)
            {
                player.MasteryByStep[row.step_id] = row.mastery;
                if (row.status == "completed" && !player.CompletedSteps.Contains(row.step_id))
                    player.CompletedSteps.Add(row.step_id);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] LoadStepProgressAsync failed: {e.Message}");
        }
    }

    private async Task RecoverOrphanSessionsAsync(string playerId, SupabaseClient client)
    {
        try
        {
            // Sessions last_heartbeat_at older than 5 minutes and still active = orphaned
            string cutoff = Uri.EscapeDataString(DateTime.UtcNow.AddMinutes(-5).ToString("O"));
            string filter = $"player_id=eq.{playerId}&status=eq.active&last_heartbeat_at=lt.{cutoff}";
            string json = await client.GetAsync("play_sessions", filter + "&select=id");
            var rows = JsonHelper.FromJsonArray<SessionIdRow>(json);
            if (rows == null || rows.Length == 0) return;

            foreach (var row in rows)
            {
                await client.PatchAsync("play_sessions", $"id=eq.{row.id}",
                    "{\"status\":\"abandoned\"}");
                Debug.Log($"[PlayerDataManager] Recovered orphan session: {row.id}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerDataManager] RecoverOrphanSessionsAsync failed: {e.Message}");
        }
    }

    private Player CreateDefaultPlayer(string playerName)
    {
        return new Player
        {
            Id = "",  // filled by DB on first save
            Name = playerName,
            CurrentSubject = "Math",
            CurrentChallenge = "addition",
            CurrentStep = 1,
            CurrentStepId = ChallengeDataManager.STEP_ADDITION_1_ID,
            MasteryByStep = new Dictionary<string, float>(),
            CompletedSteps = new List<string>(),
            QuestionHistory = new List<QuestionResult>()
        };
    }

    private Player RowToPlayer(PlayerRow row)
    {
        var player = new Player
        {
            Id = row.id,
            Name = row.name,
            TotalExp = row.total_exp,
            Coins = row.coins,
            CurrentStepId = row.current_step_id ?? "",
            MasteryByStep = new Dictionary<string, float>(),
            CompletedSteps = new List<string>(),
            QuestionHistory = new List<QuestionResult>()
        };

        // Resolve runtime navigation helpers from step UUID if possible
        if (!string.IsNullOrEmpty(player.CurrentStepId))
        {
            var step = ChallengeDataManager.Instance.GetStepById(player.CurrentStepId);
            if (step != null)
            {
                player.CurrentSubject   = step.Subject;
                player.CurrentChallenge = step.Challenge?.ToLower() ?? player.CurrentChallenge;
                player.CurrentStep      = step.Number;
            }
        }
        return player;
    }

    private string BuildPlayerJson(Player p)
    {
        string stepId = string.IsNullOrEmpty(p.CurrentStepId) ? "null" : $"\"{p.CurrentStepId}\"";
        string id     = string.IsNullOrEmpty(p.Id)            ? ""     : $"\"id\":\"{p.Id}\",";
        return $"{{{id}\"name\":\"{Esc(p.Name)}\"," +
               $"\"total_exp\":{p.TotalExp}," +
               $"\"coins\":{p.Coins}," +
               $"\"current_step_id\":{stepId}," +
               $"\"last_seen_at\":\"{DateTime.UtcNow:O}\"}}";
    }

    private static string Esc(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    [Serializable] private class PlayerRow
    {
        public string id;
        public string name;
        public int    total_exp;
        public int    coins;
        public string current_step_id;
    }

    [Serializable] private class StepProgressRow
    {
        public string step_id;
        public float  mastery;
        public int    best_streak;
        public int    times_completed;
        public string status;
    }

    [Serializable] private class SessionIdRow { public string id; }
}

