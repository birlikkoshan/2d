using UnityEngine;
using TMPro;

/// <summary>
/// Displays the current level name and a move counter in the HUD.
/// Attach to Canvas_HUD or any always-active child in the Game scene.
///
/// The move counter increments on every GridManager.OnBeamUpdated event
/// fired by a player action. The initial fire from BuildGrid is skipped
/// via an _initialized flag so the counter starts at 0 on level load.
///
/// Wire the same LevelData array as in LevelManager (Inspector).
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _levelNameText;
    [SerializeField] private TMP_Text _moveCountText;

    [Header("Level Data (same order as LevelManager)")]
    [SerializeField] private LevelData[] _levels;

    // ─── State ────────────────────────────────────────────────────────────────
    private int  _moveCount;
    private bool _initialized;

    // ═══════════════════════════════════════════════════════════════════════════
    private void OnEnable()  => GridManager.OnBeamUpdated += OnBeamUpdated;
    private void OnDisable() => GridManager.OnBeamUpdated -= OnBeamUpdated;

    private void Start()
    {
        _moveCount   = 0;
        _initialized = false;

        if (_levels != null && _levels.Length > 0)
        {
            int idx = Mathf.Clamp(PlayerPrefs.GetInt("SelectedLevel", 0), 0, _levels.Length - 1);
            if (_levelNameText != null)
                _levelNameText.text = _levels[idx] != null ? _levels[idx].levelName : $"Level {idx + 1}";
        }
        else if (_levelNameText != null)
        {
            _levelNameText.text = "Level";
        }

        if (_moveCountText != null) _moveCountText.text = "Moves: 0";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    private void OnBeamUpdated()
    {
        if (!_initialized) { _initialized = true; return; } 

        _moveCount++;
        if (_moveCountText != null)
            _moveCountText.text = $"Moves: {_moveCount}";
    }
}
