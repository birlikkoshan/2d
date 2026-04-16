using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Marks a cell as a beam target.
/// Visually switches between dim (unlit) and bright (lit) states via an Image component.
/// When all targets in the scene are hit, triggers GridManager.NotifyLevelComplete().
///
/// Called by BeamTracer: Hit() on beam arrival, Reset() at the start of each new trace.
/// </summary>
[RequireComponent(typeof(Image))]
public class TargetController : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color _offColor = new Color(1f, 0.45f, 0.1f, 0.3f);  // dim orange
    [SerializeField] private Color _onColor  = new Color(1f, 0.45f, 0.1f, 1.0f);  // bright orange

    private Image _image;
    private bool  _isHit;

    public bool IsHit => _isHit;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.color = _offColor;
    }

    public void Hit()
    {
        if (_isHit) return;
        _isHit = true;

        if (_image != null) _image.color = _onColor;

        AudioManager.Instance?.PlayBeamHit();
        CheckAllHit();
    }

    /// <summary>Called by BeamTracer at the start of each new trace to reset state.</summary>
    public void Reset()
    {
        _isHit = false;
        if (_image != null) _image.color = _offColor;
    }

    private void CheckAllHit()
    {
        var all = FindObjectsByType<TargetController>(FindObjectsSortMode.None);
        foreach (var t in all)
            if (!t._isHit) return;

        var grids = FindObjectsByType<GridManager>(FindObjectsSortMode.None);
        if (grids.Length > 0) grids[0].NotifyLevelComplete();
    }
}
