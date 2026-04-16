using UnityEngine;

/// <summary>
/// Saves and loads player progress via PlayerPrefs.
/// Static class — call from anywhere.
/// </summary>
public static class SaveSystem
{
    private const string KEY_MAX_LEVEL = "MaxUnlockedLevel";
    private const string KEY_SELECTED  = "SelectedLevel";

    public static void CompleteLevel(int levelIndex)
    {
        int current = GetMaxUnlockedLevel();
        if (levelIndex >= current)
            PlayerPrefs.SetInt(KEY_MAX_LEVEL, levelIndex + 1);

        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Level {levelIndex} complete. Unlocked up to: {GetMaxUnlockedLevel()}");
    }

    public static int  GetMaxUnlockedLevel() => PlayerPrefs.GetInt(KEY_MAX_LEVEL, 0);
    public static bool IsLevelUnlocked(int index) => index <= GetMaxUnlockedLevel();
    public static void SelectLevel(int index) => PlayerPrefs.SetInt(KEY_SELECTED, index);

    /// <summary>Resets all progress (for testing).</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveSystem] Progress reset.");
    }
}
