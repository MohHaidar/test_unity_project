using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the catalog of virtual characters used in parlour challenges.
/// Hardcoded characters (with fixed UUIDs matching the migration seed) serve as
/// offline fallback. Call LoadFromSupabaseAsync() at session start to override.
///
/// UUID namespace: 0c000000–0c400000 (characters table, separate from steps/challenges)
/// </summary>
public class CharacterManager
{
    private static CharacterManager _instance;
    public static CharacterManager Instance
    {
        get { if (_instance == null) _instance = new CharacterManager(); return _instance; }
    }

    // ── Fixed character UUIDs — must match migration seed ────────────────────
    public const string CHAR_MAYA_ID     = "0c000000-0000-0000-0000-000000000000";
    public const string CHAR_VICTOR_ID   = "0c100000-0000-0000-0000-000000000000";
    public const string CHAR_ZOE_ID      = "0c200000-0000-0000-0000-000000000000";
    public const string CHAR_DR_CHEN_ID  = "0c300000-0000-0000-0000-000000000000";
    public const string CHAR_ALEX_ID     = "0c400000-0000-0000-0000-000000000000";

    private Dictionary<string, Character> _charactersById = new Dictionary<string, Character>();

    public CharacterManager()
    {
        InitializeHardcodedCharacters();
    }

    // ── Public query API ─────────────────────────────────────────────────────

    public Character GetCharacterById(string id) =>
        _charactersById.TryGetValue(id, out var c) ? c : null;

    public List<Character> GetAllCharacters() => _charactersById.Values.ToList();

    // ── Hardcoded fallback ───────────────────────────────────────────────────

    private void InitializeHardcodedCharacters()
    {
        const string VC_SUBJECT_ID = "a4000000-0000-0000-0000-000000000000";

        Register(new Character(
            CHAR_MAYA_ID,
            "Maya",
            "Warm, encouraging, and celebratory. She genuinely delights in the player's progress and frames every challenge as a shared adventure. She uses analogies from everyday life and is never condescending.",
            "Friendly and casual. Uses \"we\" and \"us\" often to create togetherness. Short enthusiastic sentences, occasional exclamation marks. Uses contractions freely. Celebrates even small wins.",
            "maya_placeholder",
            VC_SUBJECT_ID));

        Register(new Character(
            CHAR_VICTOR_ID,
            "Victor",
            "Formal, precise, and mildly impatient. He values professionalism above all and holds the player to a high standard. He is not unkind, but he does not tolerate vagueness or sloppiness.",
            "Structured and professional. No contractions. Full sentences. Measured tone. Uses transitional phrases like \"Furthermore\" and \"With that said\". Rarely praises unless deserved.",
            "victor_placeholder",
            VC_SUBJECT_ID));

        Register(new Character(
            CHAR_ZOE_ID,
            "Zoe",
            "Playful, mischievous, and effortlessly witty. She uses humour as a teaching tool and enjoys testing the player with ambiguous or layered language. Always friendly but loves to keep people on their toes.",
            "Witty and punchy. Short bursts of speech. Lots of rhetorical questions and gentle sarcasm. Uses \"Sooo...\" to introduce something tricky. Casual slang but never rude.",
            "zoe_placeholder",
            VC_SUBJECT_ID));

        Register(new Character(
            CHAR_DR_CHEN_ID,
            "Dr. Chen",
            "Analytical, curious, and fascinated by nuance. She finds beauty in precise word choice and subtle meaning. Patient and encouraging, but gently probes when the player is being superficial.",
            "Measured and thoughtful. Starts observations with \"Interesting...\" or \"Notice how...\". Uses academic vocabulary but explains it naturally. Poses Socratic follow-up questions.",
            "dr_chen_placeholder",
            VC_SUBJECT_ID));

        Register(new Character(
            CHAR_ALEX_ID,
            "Alex",
            "Sarcastic, sharp-tongued, and deceptively insightful. Alex challenges the player by being indirect and testing their ability to read subtext. Behind the dry exterior is a genuine interest in whether the player truly understands.",
            "Dry humour delivered deadpan. Uses understatement and rhetorical questions constantly. Will say the opposite of what they mean to see if the player catches it. Never obviously helpful.",
            "alex_placeholder",
            VC_SUBJECT_ID));

        Debug.Log($"[CharacterManager] Hardcoded catalog: {_charactersById.Count} characters.");
    }

    private void Register(Character c) => _charactersById[c.Id] = c;

    // ── Supabase loader ──────────────────────────────────────────────────────

    /// <summary>
    /// Loads characters from Supabase and rebuilds the lookup dictionary.
    /// Falls back to hardcoded data on error.
    /// </summary>
    public async Task LoadFromSupabaseAsync()
    {
        var client = SupabaseClient.Instance;
        if (client == null || !client.IsReady)
        {
            Debug.LogWarning("[CharacterManager] SupabaseClient not ready — using hardcoded characters.");
            return;
        }

        try
        {
            string json = await client.GetAsync("characters", "select=*");
            var rows = JsonHelper.FromJsonArray<CharacterRow>(json);

            if (rows == null || rows.Length == 0)
            {
                Debug.Log("[CharacterManager] No characters in Supabase — using hardcoded catalog.");
                return;
            }

            _charactersById.Clear();
            foreach (var r in rows)
                Register(new Character(r.id, r.name, r.personality, r.speaking_style, r.avatar_key, r.subject_id));

            Debug.Log($"[CharacterManager] Loaded {rows.Length} characters from Supabase.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[CharacterManager] Load failed — using hardcoded catalog. {e.Message}");
        }
    }

    // ── DTO ──────────────────────────────────────────────────────────────────

    [System.Serializable]
    private class CharacterRow
    {
        public string id;
        public string name;
        public string personality;
        public string speaking_style;
        public string avatar_key;
        public string subject_id;
    }
}
