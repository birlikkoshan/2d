using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MirrorController : MonoBehaviour, IPointerClickHandler
{
    [Header("Initial state (spawn)")]
    [Tooltip("Z rotation in degrees before GridManager applies LevelData (see GridManager mirror rotation source).")]
    [SerializeField] private float _initialRotationZ;

    [Header("Shape")]
    [Tooltip("1 = diagonal TL–BR; smaller = shorter hypotenuse along left/bottom edges from BL (symmetric).")]
    [SerializeField, Range(0.01f, 1f)] private float _hypotenuseCornerT = 1f;

    [Header("Colors")]
    [SerializeField] private Color _mirrorColor    = new Color(0.55f, 0.27f, 0.90f, 1f);
    [SerializeField] private Color _glowColor      = new Color(0.55f, 0.27f, 0.90f, 0.3f);
    [SerializeField] private Color _highlightColor = new Color(0.75f, 0.47f, 1f,   1f);

    [Header("Rotation animation duration (seconds)")]
    [SerializeField] private float _rotateDuration = 0.18f;

    private RectTransform _rt;
    private MirrorGraphic _graphic;
    private PolygonCollider2D _poly;

    private float _targetAngle;
    private float _currentAngle;
    private float _fromAngle;
    private float _animTimer;
    private bool  _animating;

    public float MirrorRotationZ => _currentAngle;

    public float InitialRotationZ => _initialRotationZ;

    public float HypotenuseCornerT
    {
        get => _hypotenuseCornerT;
        set
        {
            _hypotenuseCornerT = Mathf.Clamp(value, 0.01f, 1f);
            ApplyRotation(_currentAngle);
        }
    }

    public void SetHypotenuseCornerT(float t) => HypotenuseCornerT = t;

    private void Awake()
    {
        _rt   = GetComponent<RectTransform>();
        _poly = GetComponent<PolygonCollider2D>();

        _graphic = GetComponent<MirrorGraphic>();
        if (_graphic == null) _graphic = gameObject.AddComponent<MirrorGraphic>();

        _graphic.MirrorColor = _mirrorColor;
        _graphic.GlowColor   = _glowColor;

        _currentAngle = _targetAngle = _fromAngle = _initialRotationZ;
    }

    private void Start()
    {
        _targetAngle = _fromAngle = _currentAngle;
        ApplyRotation(_currentAngle);
        _animating = false;
    }

    private void Update()
    {
        if (!_animating || !Application.isPlaying) return;

        _animTimer += Time.deltaTime;
        float t     = Mathf.Clamp01(_animTimer / _rotateDuration);
        float eased = 1f - Mathf.Pow(1f - t, 3f); 

        _currentAngle = Mathf.LerpAngle(_fromAngle, _targetAngle, eased);
        ApplyRotation(_currentAngle);

        if (t >= 1f)
        {
            _currentAngle = _targetAngle;
            ApplyRotation(_currentAngle);
            _animating = false;
            GridManager.NotifyBeamUpdated();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _hypotenuseCornerT = Mathf.Clamp(_hypotenuseCornerT, 0.01f, 1f);
        if (Application.isPlaying) return;
        _currentAngle = _targetAngle = _fromAngle = _initialRotationZ;
        _rt ??= GetComponent<RectTransform>();
        _poly ??= GetComponent<PolygonCollider2D>();
        _graphic ??= GetComponent<MirrorGraphic>();
        if (_rt == null || _rt.rect.width < 0.01f) return;
        float hs = _rt.rect.width * 0.5f;
        SyncColliderToAngle(hs, _currentAngle);
        _graphic?.SetVerticesDirty();
    }
#endif

    public void OnPointerClick(PointerEventData _) => Rotate90();

    public void Rotate90()
    {
        _fromAngle   = _currentAngle;
        _targetAngle = _currentAngle + 90f;

        _animTimer = 0f;
        _animating = true;

        _graphic.FlashHighlight(_highlightColor);
        AudioManager.Instance?.PlayMirrorRotate();
    }

    public void SetRotationImmediate(float zDegrees)
    {
        _currentAngle = _targetAngle = _fromAngle = zDegrees;
        ApplyRotation(_currentAngle);
        _animating = false;
    }

    private void ApplyRotation(float angle)
    {
        _rt.localRotation = Quaternion.identity;
        float hs = _rt.rect.width * 0.5f;
        SyncColliderToAngle(hs, angle);
        if (_graphic != null)
            _graphic.SetVerticesDirty();
    }

    /// <summary>Right-angle triangle BL–P–Q (P on left edge, Q on bottom), rotated around cell center.</summary>
    public void SyncColliderToAngle(float halfSize, float angleDeg)
    {
        if (_poly == null) return;
        GetMirrorTriangleVerticesLocal(halfSize, _hypotenuseCornerT, angleDeg,
            out Vector2 p, out Vector2 q, out Vector2 bl);
        _poly.pathCount = 1;
        _poly.SetPath(0, new Vector2[] { p, q, bl });
    }

    /// <summary>Unrotated vertices: P on left edge, Q on bottom, BL corner; then rotated by <paramref name="zDeg"/>.</summary>
    public static void GetMirrorTriangleVerticesLocal(float halfSize, float cornerT, float zDeg,
        out Vector2 p, out Vector2 q, out Vector2 bl)
    {
        float hs = halfSize;
        float t  = Mathf.Clamp(cornerT, 0.01f, 1f);
        bl = new Vector2(-hs, -hs);
        p  = new Vector2(-hs, -hs + 2f * t * hs);
        q  = new Vector2(-hs + 2f * t * hs, -hs);
        p = RotateZ(p, zDeg);
        q = RotateZ(q, zDeg);
        bl = RotateZ(bl, zDeg);
    }

    /// <summary>Hypotenuse segment in the same space as <see cref="GridManager.GetCellCenter"/> (canvas local under GridContainer).</summary>
    public void GetHypotenuseCanvasSegment(float halfSize, Vector2 cellCenter, out Vector2 hp0, out Vector2 hp1)
    {
        GetMirrorTriangleVerticesLocal(halfSize, _hypotenuseCornerT, _currentAngle,
            out Vector2 p, out Vector2 q, out _);
        hp0 = cellCenter + p;
        hp1 = cellCenter + q;
    }

    private static Vector2 RotateZ(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float c   = Mathf.Cos(rad);
        float s   = Mathf.Sin(rad);
        return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
    }

    public static Vector2Int Reflect(Vector2Int incoming, float mirrorZDeg, float hypotenuseCornerT = 1f)
    {
        Vector2 n = GetReflectNormal(mirrorZDeg, hypotenuseCornerT);
        Vector2 d = new Vector2(incoming.x, incoming.y);
        Vector2 r = d - 2f * Vector2.Dot(d, n) * n;

        int rx = Mathf.RoundToInt(r.x);
        int ry = Mathf.RoundToInt(r.y);
        if (rx == 0 && ry == 0)
        {
            if (Mathf.Abs(r.x) >= Mathf.Abs(r.y))
                rx = r.x >= 0f ? 1 : -1;
            else
                ry = r.y >= 0f ? 1 : -1;
        }
        return new Vector2Int(rx, ry);
    }

    private static Vector2 GetReflectNormal(float mirrorZDeg, float hypotenuseCornerT)
    {
        float t = Mathf.Clamp(hypotenuseCornerT, 0.01f, 1f);
        const float hs = 1f;
        Vector2 p = new Vector2(-hs, -hs + 2f * t * hs);
        Vector2 q = new Vector2(-hs + 2f * t * hs, -hs);
        Vector2 v = q - p;
        Vector2 n0 = new Vector2(-v.y, v.x).normalized;
        return RotateZ(n0, mirrorZDeg);
    }
}
