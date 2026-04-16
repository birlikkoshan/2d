using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Universal UIManager — present in every scene.
/// Listens to GridManager events and controls panel visibility.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels (assign per scene)")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _pausePanel;

    // ════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        // Give panels their own Canvas so they always render on top of game elements,
        // regardless of sibling order in Canvas_HUD.
        EnsureTopSorting(_pausePanel, sortOrder: 100);
        EnsureTopSorting(_winPanel,   sortOrder: 101);
    }

    private static void EnsureTopSorting(GameObject panel, int sortOrder)
    {
        if (panel == null) return;
        var canvas = panel.GetComponent<Canvas>();
        if (canvas == null) canvas = panel.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder    = sortOrder;
        if (panel.GetComponent<GraphicRaycaster>() == null)
            panel.AddComponent<GraphicRaycaster>();
    }

    // ════════════════════════════════════════════════════════════════════════
    private void OnEnable()  => GridManager.OnLevelComplete += HandleLevelComplete;
    private void OnDisable() => GridManager.OnLevelComplete -= HandleLevelComplete;

    // ════════════════════════════════════════════════════════════════════════
    private void HandleLevelComplete(int levelIndex)
    {
        if (_winPanel != null)
        {
            _winPanel.SetActive(true);
            _winPanel.transform.SetAsLastSibling();
            var wpc = _winPanel.GetComponent<WinPanelController>();
            if (wpc != null) wpc.ShowWin();
        }

        Debug.Log($"[UIManager] Win! Level {levelIndex}");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Button callbacks — wire via Inspector UnityEvents or BtnAction scripts
    // ════════════════════════════════════════════════════════════════════════

    public void GoToMainMenu()    => SceneManager.LoadScene("MainMenu");
    public void GoToLevelSelect() => SceneManager.LoadScene("LevelSelect");
    public void RestartLevel()    => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void TogglePause()
    {
        if (_pausePanel == null) return;
        bool active = !_pausePanel.activeSelf;
        _pausePanel.SetActive(active);
        Time.timeScale = active ? 0f : 1f;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
