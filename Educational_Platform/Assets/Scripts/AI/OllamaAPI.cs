using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Low-level HTTP wrapper for Ollama API.
/// Handles all communication with local Ollama instance.
/// </summary>
public class OllamaAPI
{
    private const string OLLAMA_BASE_URL = "http://localhost:11434";
    private const string GENERATE_ENDPOINT = "/api/generate";
    private const string DEFAULT_MODEL = "mistral";
    private const int TIMEOUT_SECONDS = 30;

    private string _model;

    public OllamaAPI(string model = DEFAULT_MODEL)
    {
        _model = model;
    }

    /// <summary>
    /// Makes a synchronous request to Ollama API.
    /// WARNING: Blocks the main thread. Use sparingly or from background thread.
    /// </summary>
    public string GenerateSync(string prompt, float temperature = 0.3f)
    {
        string url = OLLAMA_BASE_URL + GENERATE_ENDPOINT;
        
        // Create request JSON
        OllamaRequest request = new OllamaRequest
        {
            model = _model,
            prompt = prompt,
            stream = false,
            temperature = temperature
        };

        string jsonBody = JsonUtility.ToJson(request);
        Debug.Log($"[OllamaAPI] Sending request to {url}");
        Debug.Log($"[OllamaAPI] Prompt length: {prompt.Length} chars");

        try
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                // Set headers
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = TIMEOUT_SECONDS;

                // Set body
                webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                // Send and wait for response
                webRequest.SendWebRequest();

                // Busy wait (not ideal but simple for MVP)
                while (!webRequest.isDone)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[OllamaAPI] Error: {webRequest.error}");
                    return null;
                }

                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"[OllamaAPI] Response received ({responseText.Length} chars)");

                // Parse response to extract generated text
                OllamaResponse response = JsonUtility.FromJson<OllamaResponse>(responseText);
                return response?.response ?? null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[OllamaAPI] Exception: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Checks if Ollama is reachable at localhost:11434.
    /// </summary>
    public bool IsOllamaAvailable()
    {
        try
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(OLLAMA_BASE_URL))
            {
                webRequest.timeout = 5;
                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    System.Threading.Thread.Sleep(50);
                }

                return webRequest.result == UnityWebRequest.Result.Success;
            }
        }
        catch
        {
            return false;
        }
    }

    [System.Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
        public float temperature;
    }

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
        public string model;
        public long created_at;
        public long eval_count;
        public long eval_duration;
    }
}
