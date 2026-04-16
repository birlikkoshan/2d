using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dynamically spawns one level-select button per entry in _levels.
/// Attach to any active GameObject in the LevelSelect scene.
///
/// Button prefab requirements:
///   - Button component
///   - Image component (for tinting locked buttons)
///   - TMP_Text component somewhere in its children
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Data (same order as LevelManager)")]
    [SerializeField] private LevelData[] _levels;

    [Header("UI References")]
    [SerializeField] private Transform  _levelGrid;
    [SerializeField] private GameObject _buttonPrefab;

    private void Start()
    {
        if (_levels == null || _levels.Length == 0)
        {
            Debug.LogError("[LevelSelectUI] _levels array is empty.");
            return;
        }

        if (_levelGrid == null || _buttonPrefab == null)
        {
            Debug.LogError("[LevelSelectUI] _levelGrid or _buttonPrefab not assigned.");
            return;
        }

        for (int i = 0; i < _levels.Length; i++)
            SpawnButton(i);
    }

    private void SpawnButton(int i)
    {
        var go    = Instantiate(_buttonPrefab, _levelGrid);
        go.name   = $"LevelButton_{i}";

        var label = go.GetComponentInChildren<TMP_Text>();
        var btn   = go.GetComponent<Button>();
        var img   = go.GetComponent<Image>();

        if (SaveSystem.IsLevelUnlocked(i))
        {
            if (label != null)
                label.text = _levels[i] != null ? _levels[i].levelName : $"Level {i + 1}";

            int captured = i;   // closure-in-loop fix
            btn?.onClick.AddListener(() =>
            {
                SaveSystem.SelectLevel(captured);
                SceneManager.LoadScene("Game");
            });
        }
        else
        {
            if (label != null) label.text = "?";
            if (img   != null) img.color  = new Color(0.3f, 0.3f, 0.3f, 1f);
            if (btn   != null) btn.interactable = false;
        }
    }
}
