using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renders the laser beam paths using world-space LineRenderer objects.
///
/// Subscribes to BeamTracer.OnBeamWorldTraced and updates one LineRenderer
/// per emitter. Each LineRenderer is created at the scene root (NOT inside
/// the Canvas hierarchy) so it renders correctly in world space and sorts
/// above Canvas elements via sortingOrder.
///
/// Auto-created by BeamTracer if no BeamVisualizer exists in the scene.
/// </summary>
public class BeamVisualizer : MonoBehaviour
{
    [Header("Beam appearance")]
    [SerializeField] private Color _beamColorStart = new Color(0f, 0.9f, 1f, 1f);   // opaque cyan
    [SerializeField] private Color _beamColorEnd   = new Color(0f, 0.9f, 1f, 0f);   // fade out
    [SerializeField] private float _beamWidth      = 0.05f;
    [SerializeField] private int   _sortingOrder   = 2;   // above Canvas (Canvas default = 0)

    private readonly List<LineRenderer> _lines = new List<LineRenderer>();

    private void OnEnable()  => BeamTracer.OnBeamWorldTraced += DrawBeams;
    private void OnDisable() => BeamTracer.OnBeamWorldTraced -= DrawBeams;

    private void OnDestroy()
    {
        foreach (var lr in _lines)
            if (lr != null) Destroy(lr.gameObject);
        _lines.Clear();
    }

    private void DrawBeams(List<List<Vector3>> worldPaths)
    {
        while (_lines.Count < worldPaths.Count)
            _lines.Add(CreateBeamLine());

        for (int i = worldPaths.Count; i < _lines.Count; i++)
            _lines[i].positionCount = 0;

        for (int i = 0; i < worldPaths.Count; i++)
        {
            var lr   = _lines[i];
            var path = worldPaths[i];

            lr.positionCount = path.Count;
            for (int j = 0; j < path.Count; j++)
                lr.SetPosition(j, path[j]);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Creates a new scene-root LineRenderer GameObject.
    /// Placed at the scene root (not inside Canvas) so it renders correctly
    /// as a world-space object. sortingOrder > canvas sortingOrder ensures
    /// it draws on top of grid cells.
    /// </summary>
    private LineRenderer CreateBeamLine()
    {
        var go = new GameObject("BeamLine");
        go.transform.SetParent(null);
        go.transform.position = Vector3.zero;

        var lr            = go.AddComponent<LineRenderer>();
        lr.positionCount  = 0;
        lr.startWidth     = _beamWidth;
        lr.endWidth       = _beamWidth;
        lr.useWorldSpace  = true;
        lr.startColor     = _beamColorStart;
        lr.endColor       = _beamColorEnd;
        lr.numCapVertices = 4;
        lr.material       = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder   = _sortingOrder;

        return lr;
    }
}
