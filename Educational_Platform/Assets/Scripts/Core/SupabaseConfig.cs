using UnityEngine;/// <summary>
/// Supabase connection configuration. Store as SupabaseConfig.asset in Assets/Resources/.
/// This file is gitignored — each developer fills in their own credentials.
/// Load at runtime via: Resources.Load<SupabaseConfig>("SupabaseConfig")
/// </summary>
[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "Educational Platform/Supabase Config")]
public class SupabaseConfig : ScriptableObject
{
    [Tooltip("Your Supabase project URL. Found in: Settings → API → Project URL")]
    public string ProjectUrl = "";

    [Tooltip("Your Supabase anon public key. Found in: Settings → API → anon public")]
    public string AnonKey = "";
}
