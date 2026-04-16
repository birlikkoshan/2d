using UnityEngine;

public class EmitterController : MonoBehaviour
{

    public int GridX;

    public int GridY;

    public Vector2Int Direction { get; private set; }

    public void RefreshDirectionFromTransform()
    {
        float angleRad = transform.localEulerAngles.z * Mathf.Deg2Rad;
        float dx = -Mathf.Sin(angleRad);
        float dy =  Mathf.Cos(angleRad);

        Direction = new Vector2Int(Mathf.RoundToInt(dx), Mathf.RoundToInt(dy));
        if (Direction == Vector2Int.zero)
            Direction = new Vector2Int(1, 0);
    }
}
