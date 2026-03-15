using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Thin REST client for Supabase. Attach to a persistent GameObject in your first scene.
/// Credentials are loaded from Assets/Resources/SupabaseConfig.asset (gitignored).
/// </summary>
public class SupabaseClient : MonoBehaviour
{
    public static SupabaseClient Instance { get; private set; }

    private string _projectUrl;
    private string _anonKey;
    private bool _isReady;

    public bool IsReady => _isReady;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var config = Resources.Load<SupabaseConfig>("SupabaseConfig");
        if (config == null || string.IsNullOrEmpty(config.ProjectUrl) || string.IsNullOrEmpty(config.AnonKey))
        {
            Debug.LogError("[SupabaseClient] SupabaseConfig.asset not found or empty in Resources/. " +
                           "Create it via: Right-click Assets/Resources → Create → Educational Platform → Supabase Config");
            return;
        }

        _projectUrl = config.ProjectUrl.TrimEnd('/');
        _anonKey = config.AnonKey;
        _isReady = true;

        Debug.Log($"[SupabaseClient] Connected to Supabase: {_projectUrl}");
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    /// <summary>GET /rest/v1/{table}?{query}</summary>
    public Task<string> GetAsync(string table, string query = "")
        => Send(BuildGet(Endpoint(table, query)));

    /// <summary>POST /rest/v1/{table} with JSON body. Returns inserted row(s).</summary>
    public Task<string> PostAsync(string table, string json)
        => Send(BuildPost(Endpoint(table), json), "return=representation");

    /// <summary>POST with upsert (merge on conflict). Use for insert-or-update.
    /// Pass onConflict (e.g. "player_id,step_id") when the conflict key is not the primary key.</summary>
    public Task<string> UpsertAsync(string table, string json, string onConflict = null)
    {
        string url = string.IsNullOrEmpty(onConflict)
            ? Endpoint(table)
            : Endpoint(table, $"on_conflict={onConflict}");
        return Send(BuildPost(url, json), "resolution=merge-duplicates,return=representation");
    }

    /// <summary>PATCH /rest/v1/{table}?{query} with JSON body. Returns updated row(s).</summary>
    public Task<string> PatchAsync(string table, string query, string json)
        => Send(BuildPatch(Endpoint(table, query), json), "return=representation");

    /// <summary>DELETE /rest/v1/{table}?{query}</summary>
    public Task<string> DeleteAsync(string table, string query)
        => Send(BuildDelete(Endpoint(table, query)));

    // ─── Internals ─────────────────────────────────────────────────────────────

    private string Endpoint(string table, string query = "")
        => $"{_projectUrl}/rest/v1/{table}{(string.IsNullOrEmpty(query) ? "" : "?" + query)}";

    private Task<string> Send(UnityWebRequest req, string preferHeader = null)
    {
        var tcs = new TaskCompletionSource<string>();
        StartCoroutine(SendCoroutine(req, tcs, preferHeader));
        return tcs.Task;
    }

    private IEnumerator SendCoroutine(UnityWebRequest req, TaskCompletionSource<string> tcs, string preferHeader)
    {
        req.SetRequestHeader("apikey", _anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {_anonKey}");
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(preferHeader))
            req.SetRequestHeader("Prefer", preferHeader);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            tcs.SetResult(req.downloadHandler?.text ?? "[]");
        }
        else
        {
            string error = $"[SupabaseClient] {req.method} {req.url} → {req.responseCode} {req.error}";
            string body = req.downloadHandler?.text;
            if (!string.IsNullOrEmpty(body)) error += $"\n{body}";
            Debug.LogError(error);
            tcs.SetException(new Exception(error));
        }

        req.Dispose();
    }

    private static UnityWebRequest BuildGet(string url)
    {
        var req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerBuffer();
        return req;
    }

    private static UnityWebRequest BuildPost(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        return req;
    }

    private static UnityWebRequest BuildPatch(string url, string json)
    {
        var req = new UnityWebRequest(url, "PATCH");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        return req;
    }

    private static UnityWebRequest BuildDelete(string url)
    {
        var req = new UnityWebRequest(url, "DELETE");
        req.downloadHandler = new DownloadHandlerBuffer();
        return req;
    }
}
