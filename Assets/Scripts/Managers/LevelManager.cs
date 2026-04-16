using UnityEngine;

/// <summary>
/// Loads the correct level from the _levels array using the index stored in PlayerPrefs.
/// Attach to the LevelManager GameObject in the Game scene.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("All levels in order")]
    [SerializeField] private LevelData[] _levels;

    [Header("References")]
    [SerializeField] private GridManager _gridManager;

    private void Start()
    {
        if (_gridManager == null)
        {
            var found = FindObjectsByType<GridManager>(FindObjectsSortMode.None);
            if (found.Length > 0) _gridManager = found[0];
        }

        LoadCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        if (_levels == null || _levels.Length == 0)
        {
            Debug.LogError("[LevelManager] Levels array is empty. Assign LevelData assets in Inspector.");
            return;
        }

        int index = Mathf.Clamp(PlayerPrefs.GetInt("SelectedLevel", 0), 0, _levels.Length - 1);
        LevelData data = _levels[index];

        if (data == null)
        {
            Debug.LogError($"[LevelManager] LevelData at index {index} is null.");
            return;
        }

        Debug.Log($"[LevelManager] Loading level {index}: {data.levelName}");
        _gridManager.BuildGrid(data);
    }

    public int  TotalLevels => _levels?.Length ?? 0;
    public bool HasNextLevel() => PlayerPrefs.GetInt("SelectedLevel", 0) + 1 < TotalLevels;
}
