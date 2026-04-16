using UnityEngine;

/// <summary>
/// Stores grid coordinates of a cell object.
/// Added to every prefab instantiated by GridManager so BeamTracer
/// can look up what occupies a given (x, y) cell.
/// </summary>
public class GridCell : MonoBehaviour
{
    public int GridX;
    public int GridY;
}
