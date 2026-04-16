using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class EmitterGraphic : Graphic
{
    public Color EmitterColor = new Color(0f, 0.85f, 1f, 0.95f);
    public Color GlowColor    = new Color(0f, 0.85f, 1f, 0.22f);

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect  r  = rectTransform.rect;
        float cx = (r.xMin + r.xMax) * 0.5f;

        Vector2 tip  = new Vector2(cx,     r.yMax);
        Vector2 botR = new Vector2(r.xMax, r.yMin);
        Vector2 botL = new Vector2(r.xMin, r.yMin);

        float g = r.width * 0.1f;
        AddTriangle(vh,
            new Vector2(tip.x,         tip.y  + g),
            new Vector2(botR.x + g,    botR.y - g),
            new Vector2(botL.x - g,    botL.y - g),
            GlowColor);

        AddTriangle(vh, tip, botR, botL, EmitterColor);

        DrawLine(vh, botL, tip,  2.5f, new Color(1f, 1f, 1f, 0.85f));
        DrawLine(vh, tip,  botR, 2.5f, new Color(1f, 1f, 1f, 0.85f));
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
