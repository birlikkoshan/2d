/// <summary>
/// Central paths and scene names for Light Beam editor tools.
/// Keep in sync with <see cref="LevelManager"/>, <see cref="HUDController"/>, and <see cref="LevelSelectUI"/> Inspector arrays.
/// </summary>
public static class LightBeamEditorConfig
{
    public const string PrefabsFolder = "Assets/Prefabs";
    public const string LevelsFolder  = "Assets/Levels";
    public const string ScenesFolder  = "Assets/Scenes";

    public const string MirrorPrefabPath  = PrefabsFolder + "/Mirror.prefab";
    public const string TargetPrefabPath  = PrefabsFolder + "/Target.prefab";
    public const string EmitterPrefabPath = PrefabsFolder + "/Emitter.prefab";
    public const string WallPrefabPath    = PrefabsFolder + "/Wall.prefab";

    /// <summary>LevelData assets in play order (same as LevelManager._levels).</summary>
    public static readonly string[] LevelDataAssetPaths =
    {
        LevelsFolder + "/Level_00.asset",
        LevelsFolder + "/Level_00 1.asset",
        LevelsFolder + "/Level_00 2.asset",
    };

    public const string SceneBootName        = "Boot";
    public const string SceneMainMenuName    = "MainMenu";
    public const string SceneLevelSelectName = "LevelSelect";
    public const string SceneGameName        = "Game";

    public static string SceneAssetPath(string sceneName) => $"{ScenesFolder}/{sceneName}.unity";
}
