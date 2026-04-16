using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Animates the Win Panel when the level is complete.
/// <see cref="UIManager"/> calls <see cref="ShowWin"/> after activating the panel
/// so it works when WinPanel starts disabled in the scene.
/// </summary>
public class WinPanelController : MonoBehaviour
{
    [Header("Panel Content")]
    [SerializeField] private TMP_Text     _headerText;
    [SerializeField] private GameObject[] _stars;
    [SerializeField] private GameObject   _nextLevelButton;

    [Header("Animation")]
    [SerializeField] private float _animDuration = 0.4f;

    private LevelManager _levelManager;
    private Coroutine _showRoutine;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        var found = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
        if (found.Length > 0) _levelManager = found[0];
        else Debug.LogWarning("[WinPanelController] LevelManager not found.");
    }

    public void ShowWin()
    {
        if (_showRoutine != null)
            StopCoroutine(_showRoutine);
        _showRoutine = StartCoroutine(ShowPanel());
    }

    private IEnumerator ShowPanel()
    {
        if (_headerText != null) _headerText.text = "Level Complete!";

        float elapsed = 0f;
        while (elapsed < _animDuration)
        {
            transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, elapsed / _animDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;

        if (_stars != null)
            foreach (var s in _stars)
                if (s != null) s.SetActive(true);

        if (_nextLevelButton != null)
            _nextLevelButton.SetActive(_levelManager != null && _levelManager.HasNextLevel());

        AudioManager.Instance?.PlayWin();
        _showRoutine = null;
    }
}
