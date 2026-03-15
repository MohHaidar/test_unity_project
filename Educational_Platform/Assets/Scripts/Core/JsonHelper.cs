using UnityEngine;

/// <summary>
/// JSON array parsing helper for Unity's JsonUtility, which does not support top-level arrays.
/// Uses the standard wrapper trick: wraps the array in {"items":[...]} before deserializing.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Parses a JSON array string (e.g. Supabase REST response) into a typed array.
    /// Returns an empty array on null, empty, or malformed input.
    /// </summary>
    public static T[] FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]" || json == "null")
            return System.Array.Empty<T>();

        try
        {
            string wrapped = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<Wrapper<T>>(wrapped)?.items ?? System.Array.Empty<T>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[JsonHelper] Failed to parse JSON array: {e.Message}\nJSON: {json}");
            return System.Array.Empty<T>();
        }
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
