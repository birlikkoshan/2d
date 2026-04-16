using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Вешается на объект BeamLayer в сцене MainMenu.
/// Рисует лазерные лучи которые отражаются от краёв экрана.
/// Требует: LineRenderer на этом же объекте НЕ нужен —
/// скрипт сам создаёт дочерние объекты с LineRenderer.
/// </summary>
public class MenuBeamAnimator : MonoBehaviour
{
    [Header("Лучи")]
    [SerializeField] private int   _beamCount     = 5;
    [SerializeField] private int   _maxReflections = 8;
    [SerializeField] private float _beamSpeed     = 3f;

    [Header("Визуал")]
    [SerializeField] private float _lineWidth     = 0.04f;
    [SerializeField] private Color _colorCyan     = new Color(0f,   0.9f, 1f,   0.6f);
    [SerializeField] private Color _colorViolet   = new Color(0.6f, 0.2f, 1f,   0.5f);
    [SerializeField] private Color _colorOrange   = new Color(1f,   0.5f, 0.1f, 0.4f);

    [Header("Пересчёт")]
    [SerializeField] private float _respawnDelay  = 3f;   // через сколько секунд луч пересоздаётся

    private Camera        _cam;
    private List<BeamData> _beams = new();

    private float _left, _right, _top, _bottom;

    private void Start()
    {
        _cam = Camera.main;
        RefreshBounds();

        for (int i = 0; i < _beamCount; i++)
            _beams.Add(CreateBeam(i));
    }

    private void Update()
    {
        RefreshBounds();

        foreach (var beam in _beams)
            UpdateBeam(beam);
    }

    private BeamData CreateBeam(int index)
    {
        var go = new GameObject($"Beam_{index}");
        go.transform.SetParent(transform, false);

        var lr             = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = true;
        lr.startWidth      = _lineWidth;
        lr.endWidth        = _lineWidth * 0.3f;
        lr.numCapVertices  = 4;
        lr.material        = CreateBeamMaterial();
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Color c = PickColor(index);
        lr.startColor = c;
        lr.endColor   = new Color(c.r, c.g, c.b, 0f);

        var beam = new BeamData
        {
            lineRenderer = lr,
            color        = c,
        };

        ResetBeam(beam);
        return beam;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Сброс луча — новая случайная позиция и направление
    // ════════════════════════════════════════════════════════════════════════
    private void ResetBeam(BeamData beam)
    {
        // Стартовая точка — случайная на одной из четырёх сторон экрана
        beam.origin    = RandomEdgePoint();
        beam.direction = RandomDirection();
        beam.points    = BuildPath(beam.origin, beam.direction);
        beam.progress  = 0f;
        beam.totalLength = PathLength(beam.points);
        beam.respawnTimer = _respawnDelay + Random.Range(0f, 2f);

        beam.lineRenderer.positionCount = beam.points.Count;
        // Сначала все точки в начале
        for (int i = 0; i < beam.points.Count; i++)
            beam.lineRenderer.SetPosition(i, beam.points[0]);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Обновление — луч "ползёт" вперёд
    // ════════════════════════════════════════════════════════════════════════
    private void UpdateBeam(BeamData beam)
    {
        beam.progress += _beamSpeed * Time.deltaTime;

        float travelled = 0f;
        for (int i = 0; i < beam.points.Count - 1; i++)
        {
            float segLen = Vector2.Distance(beam.points[i], beam.points[i + 1]);

            if (beam.progress <= travelled + segLen)
            {
                float t = (beam.progress - travelled) / segLen;
                Vector3 tip = Vector2.Lerp(beam.points[i], beam.points[i + 1], t);

                // Рисуем от начала до текущей точки
                beam.lineRenderer.positionCount = i + 2;
                for (int j = 0; j <= i; j++)
                    beam.lineRenderer.SetPosition(j, beam.points[j]);
                beam.lineRenderer.SetPosition(i + 1, tip);
                break;
            }
            travelled += segLen;
        }

        // Луч дошёл до конца — запускаем таймер исчезновения
        if (beam.progress >= beam.totalLength)
        {
            beam.respawnTimer -= Time.deltaTime;
            if (beam.respawnTimer <= 0f)
                ResetBeam(beam);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Строим путь с отражениями
    // ════════════════════════════════════════════════════════════════════════
    private List<Vector2> BuildPath(Vector2 origin, Vector2 dir)
    {
        var points = new List<Vector2> { origin };
        Vector2 pos = origin;
        Vector2 d   = dir.normalized;

        for (int i = 0; i < _maxReflections; i++)
        {
            // Находим ближайшую стену
            float tMin = float.MaxValue;
            Vector2 normal = Vector2.zero;

            Check(pos, d, new Vector2(_right, pos.y), Vector2.left,  ref tMin, ref normal);
            Check(pos, d, new Vector2(_left,  pos.y), Vector2.right, ref tMin, ref normal);
            Check(pos, d, new Vector2(pos.x,  _top),  Vector2.down,  ref tMin, ref normal);
            Check(pos, d, new Vector2(pos.x, _bottom),Vector2.up,    ref tMin, ref normal);

            if (tMin == float.MaxValue) break;

            Vector2 hit = pos + d * tMin;
            points.Add(hit);
            pos = hit;
            d   = Vector2.Reflect(d, normal);
        }

        return points;
    }

    private void Check(Vector2 pos, Vector2 dir, Vector2 wallPoint, Vector2 wallNormal,
                       ref float tMin, ref Vector2 normal)
    {
        float denom = Vector2.Dot(dir, -wallNormal);
        if (denom <= 0.001f) return;

        float t = Vector2.Dot(wallPoint - pos, -wallNormal) / denom;
        if (t > 0.01f && t < tMin)
        {
            tMin   = t;
            normal = wallNormal;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════
    private void RefreshBounds()
    {
        if (_cam == null) return;
        float h = _cam.orthographicSize;
        float w = h * _cam.aspect;
        _top    =  h;  _bottom = -h;
        _right  =  w;  _left   = -w;
    }

    private Vector2 RandomEdgePoint()
    {
        int edge = Random.Range(0, 4);
        return edge switch
        {
            0 => new Vector2(Random.Range(_left, _right), _top),
            1 => new Vector2(Random.Range(_left, _right), _bottom),
            2 => new Vector2(_left,  Random.Range(_bottom, _top)),
            _ => new Vector2(_right, Random.Range(_bottom, _top)),
        };
    }

    private Vector2 RandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                           Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private float PathLength(List<Vector2> pts)
    {
        float len = 0f;
        for (int i = 0; i < pts.Count - 1; i++)
            len += Vector2.Distance(pts[i], pts[i + 1]);
        return len;
    }

    private Color PickColor(int index) => (index % 3) switch
    {
        0 => _colorCyan,
        1 => _colorViolet,
        _ => _colorOrange,
    };

    private Material CreateBeamMaterial()
    {
        // Спрайт/Unlit шейдер — работает и в Built-in и в URP
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        mat.SetInt("_ZWrite",   0);
        mat.renderQueue = 3000;
        return mat;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Data class
    // ════════════════════════════════════════════════════════════════════════
    private class BeamData
    {
        public LineRenderer  lineRenderer;
        public Color         color;
        public Vector2       origin;
        public Vector2       direction;
        public List<Vector2> points;
        public float         progress;
        public float         totalLength;
        public float         respawnTimer;
    }
}
