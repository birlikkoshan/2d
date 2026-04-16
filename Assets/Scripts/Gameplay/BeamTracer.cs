using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Traces the laser beam through the grid in integer cell-space.
///
/// Algorithm:
///   1. Resets all TargetControllers.
///   2. Finds every EmitterController and starts a trace from each one.
///   3. Steps cell-by-cell in the current direction.
///   4. Mirror  → ray from previous cell center vs triangle (hypotenuse + two legs).
///              First edge hit: hypotenuse → Reflect + hit on that edge;
///              leg (opaque) → beam stops like a wall at the hit point.
///   5. Target  → calls TargetController.Hit(), stops beam.
///   6. Wall or out-of-bounds → stops beam.
///
/// Subscribes to GridManager.OnBeamUpdated so it re-runs whenever a mirror rotates.
///
/// Fires two events after tracing:
///   OnBeamTraced      — cell-space paths (List<List<Vector2Int>>) for game logic.
///   OnBeamWorldTraced — world-space paths (List<List<Vector3>>) for LineRenderer rendering.
///
/// Attach to any active GameObject in the Game scene.
/// </summary>
public class BeamTracer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager _grid;

    [Header("Max steps per beam (prevents infinite loops)")]
    [SerializeField] private int _maxSteps = 64;

    private const float RayTEpsilon = 1e-4f;
    private const float TieEpsilon  = 1e-5f;

    private enum MirrorEdgeKind { Hypotenuse, Leg }

    /// <summary>Fired after every full trace. Each inner list is one beam's grid-coord path.</summary>
    public static event System.Action<List<List<Vector2Int>>> OnBeamTraced;

    /// <summary>Fired after every full trace. Each inner list is one beam's world-space positions.</summary>
    public static event System.Action<List<List<Vector3>>> OnBeamWorldTraced;

    private void Awake()
    {
        if (_grid == null)
        {
            var found = FindObjectsByType<GridManager>(FindObjectsSortMode.None);
            if (found.Length > 0) _grid = found[0];
            else Debug.LogWarning("[BeamTracer] GridManager not found in scene.");
        }

        if (FindObjectsByType<BeamVisualizer>(FindObjectsSortMode.None).Length == 0)
            gameObject.AddComponent<BeamVisualizer>();
    }

    private void OnEnable()  => GridManager.OnBeamUpdated += Trace;
    private void OnDisable() => GridManager.OnBeamUpdated -= Trace;

    public void Trace()
    {
        if (_grid == null) return;

        foreach (var t in FindObjectsByType<TargetController>(FindObjectsSortMode.None))
            t.Reset();

        var allCellPaths  = new List<List<Vector2Int>>();
        var allWorldPaths = new List<List<Vector3>>();

        foreach (var em in FindObjectsByType<EmitterController>(FindObjectsSortMode.None))
        {
            TraceFrom(em.GridX, em.GridY, em.Direction,
                out var cellPath, out var worldPath);
            allCellPaths.Add(cellPath);
            allWorldPaths.Add(worldPath);
        }

        OnBeamTraced?.Invoke(allCellPaths);
        OnBeamWorldTraced?.Invoke(allWorldPaths);
    }

    private void TraceFrom(
        int startX, int startY, Vector2Int dir,
        out List<Vector2Int> cellPath,
        out List<Vector3>    worldPath)
    {
        cellPath  = new List<Vector2Int> { new Vector2Int(startX, startY) };
        worldPath = new List<Vector3>    { _grid.CanvasToWorld(_grid.GetCellCenter(startX, startY)) };

        int x  = startX;
        int y  = startY;
        int dx = dir.x;
        int dy = dir.y;

        int prevX = startX;
        int prevY = startY;

        for (int step = 0; step < _maxSteps; step++)
        {
            x += dx;
            y += dy;

            if (x < 0 || x >= _grid.GridWidth || y < 0 || y >= _grid.GridHeight)
                break;

            GridCell cell = FindCell(x, y);

            if (cell == null)
            {
                worldPath.Add(_grid.CanvasToWorld(_grid.GetCellCenter(x, y)));
                prevX = x;
                prevY = y;
                continue;
            }

            cellPath.Add(new Vector2Int(x, y));

            if (cell.TryGetComponent<MirrorController>(out var mirror))
            {
                float mirrorZ = mirror.MirrorRotationZ;
                float cornerT = mirror.HypotenuseCornerT;

                if (!TryResolveMirrorFirstHit(
                        prevX, prevY, x, y, dx, dy, mirror,
                        out Vector2 hitCanvas, out MirrorEdgeKind edgeKind))
                {
                    Vector2 mc = _grid.GetCellCenter(x, y);
                    worldPath.Add(_grid.CanvasToWorld(mc));
                    break;
                }

                worldPath.Add(_grid.CanvasToWorld(hitCanvas));

                if (edgeKind == MirrorEdgeKind.Leg)
                    break;

                Vector2Int reflected = MirrorController.Reflect(
                    new Vector2Int(dx, dy), mirrorZ, cornerT);

                if (reflected == Vector2Int.zero) break;

                dx = reflected.x;
                dy = reflected.y;

                prevX = x;
                prevY = y;
                continue;
            }

            if (cell.TryGetComponent<TargetController>(out var target))
            {
                worldPath.Add(_grid.CanvasToWorld(_grid.GetCellCenter(x, y)));
                target.Hit();
                break;
            }

            if (cell.TryGetComponent<WallController>(out _))
            {
                worldPath.Add(_grid.CanvasToWorld(_grid.GetCellCenter(x, y)));
                break;
            }

            worldPath.Add(_grid.CanvasToWorld(_grid.GetCellCenter(x, y)));
            prevX = x;
            prevY = y;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Finds the first intersection of the beam ray (prev cell → cardinal direction)
    /// with the mirror triangle boundary. Leg hits are opaque; hypotenuse is reflective.
    /// On tie (corner), prefers leg as obstacle.
    /// </summary>
    private bool TryResolveMirrorFirstHit(
        int prevX, int prevY,
        int mirX, int mirY,
        int dx, int dy,
        MirrorController mirror,
        out Vector2 hitCanvas,
        out MirrorEdgeKind edgeKind)
    {
        hitCanvas = Vector2.zero;
        edgeKind  = MirrorEdgeKind.Leg;

        float hs = _grid.CellSize * 0.5f;
        Vector2 mc = _grid.GetCellCenter(mirX, mirY);
        MirrorController.GetMirrorTriangleVerticesLocal(
            hs, mirror.HypotenuseCornerT, mirror.MirrorRotationZ,
            out Vector2 p, out Vector2 q, out Vector2 bl);

        Vector2 P  = mc + p;
        Vector2 Q  = mc + q;
        Vector2 BL = mc + bl;

        Vector2 origin = _grid.GetCellCenter(prevX, prevY);
        Vector2 rayDir = new Vector2(dx, dy);

        float bestT = float.MaxValue;
        bool  have  = false;
        Vector2 bestHit = Vector2.zero;
        MirrorEdgeKind bestKind = MirrorEdgeKind.Leg;

        void Consider(Vector2 a, Vector2 b, MirrorEdgeKind kind)
        {
            if (!TryRaySegment(origin, rayDir, a, b, out Vector2 hit, out float t))
                return;
            if (t < RayTEpsilon)
                return;

            if (!have || t < bestT - TieEpsilon)
            {
                bestT    = t;
                bestHit  = hit;
                bestKind = kind;
                have     = true;
            }
            else if (Mathf.Abs(t - bestT) <= TieEpsilon && kind == MirrorEdgeKind.Leg)
            {
                bestHit  = hit;
                bestKind = MirrorEdgeKind.Leg;
            }
        }

        Consider(P, Q, MirrorEdgeKind.Hypotenuse);
        Consider(P, BL, MirrorEdgeKind.Leg);
        Consider(Q, BL, MirrorEdgeKind.Leg);

        if (!have)
            return false;

        hitCanvas = bestHit;
        edgeKind  = bestKind;
        return true;
    }

    /// <summary>
    /// Ray–line-segment intersection.
    /// Ray: ro + t*rd (t ≥ 0).
    /// Segment: a + u*(b−a), u ∈ [0,1].
    /// Returns true with hit and ray parameter t if they intersect.
    /// </summary>
    private static bool TryRaySegment(
        Vector2 ro, Vector2 rd,
        Vector2 a,  Vector2 b,
        out Vector2 hit,
        out float tRay)
    {
        Vector2 ab    = b - a;
        float   denom = rd.x * ab.y - rd.y * ab.x;
        hit  = Vector2.zero;
        tRay = 0f;

        if (Mathf.Abs(denom) < 1e-6f) return false;   // parallel

        Vector2 ao = a - ro;
        float   t  = (ao.x * ab.y - ao.y * ab.x) / denom;
        float   u  = (ao.x * rd.y - ao.y * rd.x) / denom;

        if (t >= 0f && u >= 0f && u <= 1f)
        {
            tRay = t;
            hit  = ro + rd * tRay;
            return true;
        }
        return false;
    }

    // ════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Finds the GridCell at grid position (x, y).
    /// O(n) search — acceptable for levels with ≤100 cells.
    /// </summary>
    private static GridCell FindCell(int x, int y)
    {
        foreach (var c in FindObjectsByType<GridCell>(FindObjectsSortMode.None))
            if (c.GridX == x && c.GridY == y) return c;
        return null;
    }
}
