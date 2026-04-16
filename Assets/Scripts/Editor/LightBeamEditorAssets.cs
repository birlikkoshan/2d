using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor menus: create/update cell prefabs and LevelData assets (Tools / Light Beam).
/// </summary>
public static class LightBeamEditorAssets
{
    private const float DefaultCell = 100f;
    private const float DefaultHalf = DefaultCell * 0.5f;

    #region Menu

    [MenuItem("Tools/Light Beam/Create Prefabs")]
    public static void CreatePrefabs()
    {
        EnsurePrefabsFolder();

        UpdateMirrorPrefab();
        CreateTargetPrefab();
        CreateEmitterPrefab();
        CreateWallPrefab();
        CreateLineRendererPrefab();
        CreateSpriteRendererPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Prefabs updated in " + LightBeamEditorConfig.PrefabsFolder);
        EditorUtility.DisplayDialog("Light Beam", "Prefabs created successfully!", "OK");
    }

    [MenuItem("Tools/Light Beam/Create Level Assets")]
    public static void CreateLevelAssets()
    {
        if (!AssetDatabase.IsValidFolder(LightBeamEditorConfig.LevelsFolder))
            AssetDatabase.CreateFolder("Assets", "Levels");

        CreateLevel0();
        CreateLevel1();
        CreateLevel2();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Light Beam",
            "Level assets created successfully in " + LightBeamEditorConfig.LevelsFolder,
            "OK");
    }

    #endregion

    #region Prefabs

    private static void UpdateMirrorPrefab()
    {
        string path = LightBeamEditorConfig.MirrorPrefabPath;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(path);

            if (contents.GetComponent<PolygonCollider2D>() == null)
            {
                var poly = contents.AddComponent<PolygonCollider2D>();
                SetMirrorTriangle(poly, DefaultHalf);
                Debug.Log("[LightBeamEditorAssets] Added PolygonCollider2D to existing Mirror prefab.");
            }

            int mirrorLayer = LayerMask.NameToLayer("Mirror");
            if (mirrorLayer != -1)
                contents.layer = mirrorLayer;

            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
            Debug.Log("[LightBeamEditorAssets] Updated: " + path);
        }
        else
        {
            CreateMirrorPrefab();
        }
    }

    private static void CreateMirrorPrefab()
    {
        var go = new GameObject("Mirror", typeof(RectTransform));
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<MirrorGraphic>();
        go.AddComponent<MirrorController>();
        go.AddComponent<GridCell>();

        var poly = go.AddComponent<PolygonCollider2D>();
        SetMirrorTriangle(poly, DefaultHalf);

        int mirrorLayer = LayerMask.NameToLayer("Mirror");
        if (mirrorLayer != -1) go.layer = mirrorLayer;

        SavePrefab(go, "Mirror");
    }

    private static void SetMirrorTriangle(PolygonCollider2D poly, float hs)
    {
        MirrorController.GetMirrorTriangleVerticesLocal(hs, 1f, 0f, out Vector2 p, out Vector2 q, out Vector2 bl);
        poly.pathCount = 1;
        poly.SetPath(0, new Vector2[] { p, q, bl });
    }

    private static void CreateTargetPrefab()
    {
        var go = new GameObject("Target", typeof(RectTransform));

        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.45f, 0.1f, 0.4f);
        img.type  = Image.Type.Filled;

        go.AddComponent<TargetController>();
        go.AddComponent<GridCell>();

        var circle = go.AddComponent<CircleCollider2D>();
        circle.radius    = DefaultHalf;
        circle.isTrigger = false;

        int layer = LayerMask.NameToLayer("Target");
        if (layer != -1) go.layer = layer;

        SavePrefab(go, "Target");
    }

    private static void CreateEmitterPrefab()
    {
        var go = new GameObject("Emitter", typeof(RectTransform));

        go.AddComponent<CanvasRenderer>();
        go.AddComponent<EmitterGraphic>();
        go.AddComponent<EmitterController>();
        go.AddComponent<GridCell>();

        var box = go.AddComponent<BoxCollider2D>();
        box.size      = new Vector2(DefaultCell, DefaultCell);
        box.isTrigger = false;

        int emitterLayer = LayerMask.NameToLayer("Emitter");
        if (emitterLayer != -1) go.layer = emitterLayer;

        SavePrefab(go, "Emitter");
    }

    private static void CreateWallPrefab()
    {
        var go = new GameObject("Wall", typeof(RectTransform));

        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.28f, 1f);

        go.AddComponent<WallController>();
        go.AddComponent<GridCell>();

        var box = go.AddComponent<BoxCollider2D>();
        box.size      = new Vector2(DefaultCell, DefaultCell);
        box.isTrigger = false;

        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer != -1) go.layer = wallLayer;

        SavePrefab(go, "Wall");
    }

    private static void CreateLineRendererPrefab()
    {
        var go = new GameObject("BeamLine");

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, Vector3.right);
        lr.startWidth    = 0.05f;
        lr.endWidth      = 0.05f;
        lr.useWorldSpace = true;
        lr.startColor    = new Color(0f, 0.9f, 1f, 1f);
        lr.endColor      = new Color(0f, 0.9f, 1f, 0f);
        lr.material      = new Material(Shader.Find("Sprites/Default"));

        SavePrefab(go, "BeamLine");
    }

    private static void CreateSpriteRendererPrefab()
    {
        var go = new GameObject("SpriteObject");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color        = Color.white;
        sr.sortingOrder = 0;

        SavePrefab(go, "SpriteObject");
    }

    private static void SavePrefab(GameObject go, string name)
    {
        string path = $"{LightBeamEditorConfig.PrefabsFolder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[LightBeamEditorAssets] Saved: " + path);
    }

    private static void EnsurePrefabsFolder()
    {
        if (!AssetDatabase.IsValidFolder(LightBeamEditorConfig.PrefabsFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
    }

    #endregion

    #region Level assets

    private static void CreateLevel0()
    {
        LevelData data = LoadOrCreate(LightBeamEditorConfig.LevelDataAssetPaths[0]);

        data.levelIndex = 0;
        data.levelName  = "Level 1";
        data.gridWidth  = 5;
        data.gridHeight = 5;
        data.cells = new LevelData.CellObject[]
        {
            new LevelData.CellObject { x = 0, y = 3, type = LevelData.CellType.Emitter, rotation = -90 },
            new LevelData.CellObject { x = 2, y = 3, type = LevelData.CellType.Mirror,  rotation = 45  },
            new LevelData.CellObject { x = 2, y = 1, type = LevelData.CellType.Mirror,  rotation = -45 },
            new LevelData.CellObject { x = 4, y = 1, type = LevelData.CellType.Target,  rotation = 0   },
            new LevelData.CellObject { x = 4, y = 3, type = LevelData.CellType.Wall,    rotation = 0   },
        };

        EditorUtility.SetDirty(data);
    }

    private static void CreateLevel1()
    {
        LevelData data = LoadOrCreate(LightBeamEditorConfig.LevelDataAssetPaths[1]);

        data.levelIndex = 1;
        data.levelName  = "Level 2";
        data.gridWidth  = 7;
        data.gridHeight = 7;
        data.cells = new LevelData.CellObject[]
        {
            new LevelData.CellObject { x = 0, y = 5, type = LevelData.CellType.Emitter, rotation = -90 },
            new LevelData.CellObject { x = 2, y = 5, type = LevelData.CellType.Mirror,  rotation = 45  },
            new LevelData.CellObject { x = 2, y = 2, type = LevelData.CellType.Mirror,  rotation = -45 },
            new LevelData.CellObject { x = 5, y = 2, type = LevelData.CellType.Mirror,  rotation = 45  },
            new LevelData.CellObject { x = 5, y = 0, type = LevelData.CellType.Target,  rotation = 0   },
            new LevelData.CellObject { x = 4, y = 5, type = LevelData.CellType.Wall,    rotation = 0   },
            new LevelData.CellObject { x = 0, y = 2, type = LevelData.CellType.Wall,    rotation = 0   },
        };

        EditorUtility.SetDirty(data);
    }

    private static void CreateLevel2()
    {
        LevelData data = LoadOrCreate(LightBeamEditorConfig.LevelDataAssetPaths[2]);

        data.levelIndex = 2;
        data.levelName  = "Level 3";
        data.gridWidth  = 8;
        data.gridHeight = 8;
        data.cells = new LevelData.CellObject[]
        {
            new LevelData.CellObject { x = 0, y = 6, type = LevelData.CellType.Emitter, rotation = -90 },
            new LevelData.CellObject { x = 2, y = 6, type = LevelData.CellType.Mirror,  rotation = 45  },
            new LevelData.CellObject { x = 2, y = 3, type = LevelData.CellType.Mirror,  rotation = -45 },
            new LevelData.CellObject { x = 5, y = 3, type = LevelData.CellType.Mirror,  rotation = 45  },
            new LevelData.CellObject { x = 5, y = 1, type = LevelData.CellType.Mirror,  rotation = -45 },
            new LevelData.CellObject { x = 7, y = 1, type = LevelData.CellType.Target,  rotation = 0   },
            new LevelData.CellObject { x = 4, y = 6, type = LevelData.CellType.Wall,    rotation = 0   },
            new LevelData.CellObject { x = 7, y = 3, type = LevelData.CellType.Wall,    rotation = 0   },
            new LevelData.CellObject { x = 0, y = 3, type = LevelData.CellType.Wall,    rotation = 0   },
        };

        EditorUtility.SetDirty(data);
    }

    private static LevelData LoadOrCreate(string path)
    {
        LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(data, path);
            Debug.Log("[LightBeamEditorAssets] Created: " + path);
        }
        else
        {
            Debug.Log("[LightBeamEditorAssets] Updated: " + path);
        }

        return data;
    }

    #endregion
}
