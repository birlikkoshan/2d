using UnityEngine;

/// <summary>
/// ScriptableObject с данными одного уровня.
/// Создаётся через: Assets > Create > Light Beam > Level Data
/// </summary>
[CreateAssetMenu(fileName = "Level_00", menuName = "Light Beam/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Метаданные")]
    public int    levelIndex;
    public string levelName = "Level";

    [Header("Размер поля")]
    public int gridWidth  = 8;
    public int gridHeight = 8;

    [Header("Объекты на поле")]
    public CellObject[] cells;

    [System.Serializable]
    public struct CellObject
    {
        public int         x;
        public int         y;
        public CellType    type;
        public int         rotation; 
        [Tooltip("Для Mirror: доля гипотенузы (0–1). Почти 0 — как полная диагональ (см. GridManager).")]
        [Range(0f, 1f)]
        public float       mirrorShapeT;
    }

    public enum CellType
    {
        Empty,
        Mirror,
        Target,
        Emitter,
        Wall,
        Splitter
    }
}
