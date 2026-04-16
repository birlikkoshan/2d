using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Custom UI Graphic that draws a triangular mirror shape via Mesh.
/// Attached automatically by MirrorController.
///
/// Shape: right-angle triangle occupying half the cell.
/// The hypotenuse is the reflective surface, drawn with a bright line.
/// Rotation matches <see cref="MirrorController.MirrorRotationZ"/> (vertices rotated; RectTransform stays axis-aligned).
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class MirrorGraphic : Graphic
{
    private MirrorController _mirror;

    public Color MirrorColor = new Color(0.55f, 0.27f, 0.90f, 0.85f);
    public Color GlowColor   = new Color(0.55f, 0.27f, 0.90f, 0.25f);

    private Color _highlightOverlay  = Color.clear;
    private float _highlightTimer    = 0f;
    private const float HI_DURATION  = 0.25f;

    protected override void Awake()
    {
        base.Awake();
        _mirror = GetComponent<MirrorController>();
    }

    private void Update()
    {
        if (_highlightTimer <= 0f) return;
        _highlightTimer -= Time.deltaTime;
        _highlightOverlay.a = Mathf.Clamp01(_highlightTimer / HI_DURATION) * 0.5f;
        SetVerticesDirty();
    }

    /// <summary>Brief flash on click.</summary>
    public void FlashHighlight(Color color)
    {
        _highlightOverlay = color;
        _highlightTimer   = HI_DURATION;
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = rectTransform.rect;
        float hs = r.width * 0.5f;
        float z  = _mirror != null ? _mirror.MirrorRotationZ : 0f;
        float t  = _mirror != null ? _mirror.HypotenuseCornerT : 1f;

        MirrorController.GetMirrorTriangleVerticesLocal(hs, t, z, out Vector2 p, out Vector2 q, out Vector2 bl);

        Color body = MirrorColor;
        body.r = Mathf.Lerp(body.r, _highlightOverlay.r, _highlightOverlay.a);
        body.g = Mathf.Lerp(body.g, _highlightOverlay.g, _highlightOverlay.a);
        body.b = Mathf.Lerp(body.b, _highlightOverlay.b, _highlightOverlay.a);

        AddTriangle(vh, bl, p, q, body);

        DrawLine(vh, p, q, 3f, new Color(1f, 1f, 1f, 0.9f));
    }

    private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color col)
    {
        int i = vh.currentVertCount;
        vh.AddVert(a, col, Vector2.zero);
        vh.AddVert(b, col, Vector2.zero);
        vh.AddVert(c, col, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2);
    }

    private static void DrawLine(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color col)
    {
        Vector2 dir  = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int i = vh.currentVertCount;
        vh.AddVert(a + perp, col, Vector2.zero);
        vh.AddVert(a - perp, col, Vector2.zero);
        vh.AddVert(b + perp, col, Vector2.zero);
        vh.AddVert(b - perp, col, Vector2.zero);
        vh.AddTriangle(i,     i + 1, i + 2);
        vh.AddTriangle(i + 1, i + 3, i + 2);
    }
}
