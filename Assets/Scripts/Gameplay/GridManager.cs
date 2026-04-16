using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the game grid on a UI Canvas for a 1920×1080 reference resolution.
/// Creates cell objects as RectTransform elements inside GridContainer.
/// Also updates Physics2D collider sizes to match the actual cell size so that
/// world-space raycasting works correctly (canvas lossyScale converts pixels → world units).
/// Place on the GridManager GameObject in the Game scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    // Events
    public static event System.Action      OnBeamUpdated;
    public static event System.Action<int> OnLevelComplete;

    // Inspector
    [Header("UI Container")]
    [Tooltip("RectTransform that holds all cell objects. Its size defines the grid area.")]
    [SerializeField] private RectTransform _gridContainer;

    [Header("UI Prefabs (must contain RectTransform)")]
    [SerializeField] private GameObject _mirrorPrefab;
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private GameObject _emitterPrefab;
    [SerializeField] private GameObject _wallPrefab;

    [Header("Cell size in pixels (at 1920×1080 reference)")]
    [SerializeField] private float _cellSize = 100f;

    [Header("Grid padding from screen edges (pixels)")]
    [SerializeField] private float _padding = 80f;

    [Header("Mirror spawn")]
    [Tooltip("If true, Z rotation comes from Mirror prefab (MirrorController → Initial Rotation Z). If false, from LevelData cells[].rotation.")]
    [SerializeField] private bool _mirrorRotationFromMirrorPrefab;

    // Private
    private LevelData _levelData;

    /// <summary>
    /// Builds the level: resizes the container and instantiates all cell objects.
    /// </summary>
    public void BuildGrid(LevelData data)
    {
        ClearGrid();
        _levelData = data;

        FitCellSize(data.gridWidth, data.gridHeight);

        float gridPixelW = data.gridWidth  * _cellSize;
        float gridPixelH = data.gridHeight * _cellSize;
        _gridContainer.sizeDelta        = new Vector2(gridPixelW, gridPixelH);
        _gridContainer.anchoredPosition = Vector2.zero;

        Vector2 origin = new Vector2(
            -gridPixelW * 0.5f + _cellSize * 0.5f,
            -gridPixelH * 0.5f + _cellSize * 0.5f
        );

        foreach (var cell in data.cells)
        {
            GameObject prefab = GetPrefab(cell.type);
            if (prefab == null) continue;

            Vector2 cellPos = origin + new Vector2(cell.x * _cellSize, cell.y * _cellSize);

            var go = Instantiate(prefab, _gridContainer);
            var rt = go.GetComponent<RectTransform>();
            bool hasMirror = go.TryGetComponent<MirrorController>(out var mirrorCtrl);

            if (rt != null)
            {
                rt.anchorMin        = new Vector2(0.5f, 0.5f);
                rt.anchorMax        = new Vector2(0.5f, 0.5f);
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = cellPos;
                rt.sizeDelta        = new Vector2(_cellSize, _cellSize);
                rt.localRotation    = hasMirror
                    ? Quaternion.identity
                    : Quaternion.Euler(0f, 0f, cell.rotation);
            }

            go.name = $"{cell.type}_{cell.x}_{cell.y}";

            var gridCell = go.GetComponent<GridCell>();
            if (gridCell == null) gridCell = go.AddComponent<GridCell>();
            gridCell.GridX = cell.x;
            gridCell.GridY = cell.y;

            var emCtrl = go.GetComponent<EmitterController>();
            if (emCtrl != null)
            {
                emCtrl.GridX = cell.x;
                emCtrl.GridY = cell.y;
                emCtrl.RefreshDirectionFromTransform();
            }

            if (mirrorCtrl != null)
            {
                float mt = cell.mirrorShapeT < 0.01f ? 1f : cell.mirrorShapeT;
                mirrorCtrl.SetHypotenuseCornerT(mt);
                float mirrorZ = _mirrorRotationFromMirrorPrefab
                    ? mirrorCtrl.InitialRotationZ
                    : cell.rotation;
                mirrorCtrl.SetRotationImmediate(mirrorZ);
            }

            // Resize Physics2D colliders to match the actual cell size (canvas pixels).
            ResizeColliders(go, _cellSize);
        }

        Debug.Log($"[GridManager] Built level: {data.levelName} " +
                  $"({data.gridWidth}×{data.gridHeight}), cell size {_cellSize}px");

        NotifyBeamUpdated();
    }

    /// <summary>
    /// Adjusts _cellSize so the grid fits the screen within the padding bounds.
    /// </summary>
    private void FitCellSize(int cols, int rows)
    {
        const float refW = 800f;
        const float refH = 600f;

        float availW = refW - _padding * 2f;
        float availH = refH - _padding * 2f;

        _cellSize = Mathf.Clamp(
            Mathf.Min(availW / cols, availH / rows),
            40f, 200f);
    }

    public void ClearGrid()
    {
        if (_gridContainer == null) return;
        foreach (Transform child in _gridContainer)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Returns the center of cell (col, row) in Canvas local space (anchoredPosition).
    /// Used by BeamTracer to determine visual beam endpoints.
    /// </summary>
    public Vector2 GetCellCenter(int col, int row)
    {
        float gridPixelW = (_levelData != null ? _levelData.gridWidth  : 0) * _cellSize;
        float gridPixelH = (_levelData != null ? _levelData.gridHeight : 0) * _cellSize;

        Vector2 origin = new Vector2(
            -gridPixelW * 0.5f + _cellSize * 0.5f,
            -gridPixelH * 0.5f + _cellSize * 0.5f
        );
        return origin + new Vector2(col * _cellSize, row * _cellSize);
    }

    /// <summary>
    /// Converts a position in GridContainer canvas local space to world space.
    /// Used by BeamTracer to supply world-space positions to the LineRenderer.
    /// </summary>
    public Vector3 CanvasToWorld(Vector2 canvasLocal) =>
        _gridContainer.TransformPoint(new Vector3(canvasLocal.x, canvasLocal.y, 0f));


    public float CellSize => _cellSize;

    public int GridWidth  => _levelData != null ? _levelData.gridWidth  : 0;

    public int GridHeight => _levelData != null ? _levelData.gridHeight : 0;

    public RectTransform GridContainer => _gridContainer;

    public static void NotifyBeamUpdated() => OnBeamUpdated?.Invoke();

    public void NotifyLevelComplete()
    {
        int index = _levelData != null ? _levelData.levelIndex : -1;
        SaveSystem.CompleteLevel(index);
        OnLevelComplete?.Invoke(index);
    }

    private GameObject GetPrefab(LevelData.CellType type) => type switch
    {
        LevelData.CellType.Mirror  => _mirrorPrefab,
        LevelData.CellType.Target  => _targetPrefab,
        LevelData.CellType.Emitter => _emitterPrefab,
        LevelData.CellType.Wall    => _wallPrefab,
        _                          => null
    };


    private static void ResizeColliders(GameObject go, float cellSize)
    {
        float hs = cellSize * 0.5f;

        if (go.TryGetComponent<MirrorController>(out var mirror))
            mirror.SyncColliderToAngle(hs, mirror.MirrorRotationZ);

        // Emitter / Wall: BoxCollider2D
        if (go.TryGetComponent<BoxCollider2D>(out var box))
            box.size = new Vector2(cellSize, cellSize);

        // Target: CircleCollider2D
        if (go.TryGetComponent<CircleCollider2D>(out var circle))
            circle.radius = hs;
    }
}
