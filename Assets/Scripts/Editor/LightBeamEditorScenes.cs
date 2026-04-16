using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Game scene wiring and blank-scene generator window (Tools / Light Beam).
/// </summary>
public static class LightBeamEditorScenes
{
    [MenuItem("Tools/Light Beam/Setup Game Scene")]
    public static void SetupGameScene()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
            mainCam = Object.FindAnyObjectByType<Camera>();

        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
            mainCam.orthographic       = true;
            mainCam.orthographicSize   = 5f;
            mainCam.nearClipPlane      = 0.3f;
            mainCam.farClipPlane       = 100f;
            mainCam.tag                = "MainCamera";
            AssignCameraToCanvas(mainCam);
            Debug.Log("Camera fixed: position (0,0,-10), ortho size 5.");
        }
        else
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam   = camGO.AddComponent<Camera>();
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
            mainCam.clearFlags         = CameraClearFlags.SolidColor;
            mainCam.backgroundColor    = new Color(0.04f, 0.06f, 0.14f);
            mainCam.orthographic       = true;
            mainCam.orthographicSize   = 5f;
            mainCam.nearClipPlane      = 0.3f;
            mainCam.farClipPlane       = 100f;
            AssignCameraToCanvas(mainCam);
            Debug.Log("Camera created at (0,0,-10).");
        }

        GridManager gridMgr = Object.FindAnyObjectByType<GridManager>();
        if (gridMgr == null)
        {
            var go = new GameObject("GridManager");
            gridMgr = go.AddComponent<GridManager>();
            Debug.Log("Created GridManager.");
        }
        else
        {
            Debug.Log("GridManager already present.");
        }

        EnsureGridContainer(gridMgr);
        AssignPrefabsToGridManager(gridMgr);

        LevelManager levelMgr = Object.FindAnyObjectByType<LevelManager>();
        if (levelMgr == null)
        {
            var go = new GameObject("LevelManager");
            levelMgr = go.AddComponent<LevelManager>();
            Debug.Log("Created LevelManager.");
        }
        else
        {
            Debug.Log("LevelManager already present.");
        }

        AssignLevelsToLevelManager(levelMgr);

        SerializedObject soLM = new SerializedObject(levelMgr);
        soLM.FindProperty("_gridManager").objectReferenceValue = gridMgr;
        soLM.ApplyModifiedProperties();

        BeamTracer beamTracer = Object.FindAnyObjectByType<BeamTracer>();
        if (beamTracer == null)
        {
            var go = new GameObject("BeamTracer");
            beamTracer = go.AddComponent<BeamTracer>();

            SerializedObject soBT = new SerializedObject(beamTracer);
            soBT.FindProperty("_grid").objectReferenceValue = gridMgr;
            soBT.ApplyModifiedProperties();

            Debug.Log("[LightBeamEditorScenes] Created BeamTracer.");
        }
        else
        {
            Debug.Log("[BeamTracer already present.");
        }

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem.");
        }

        GameObject winPanel = GameObject.Find("WinPanel");
        if (winPanel == null)
        {
            Debug.LogWarning("WinPanel not found in scene. Skipping WinPanelController setup.");
        }
        else
        {
            WinPanelController wpc = winPanel.GetComponent<WinPanelController>();
            if (wpc == null)
            {
                wpc = winPanel.AddComponent<WinPanelController>();
                Debug.Log("Added WinPanelController to WinPanel.");
            }

            SerializedObject soWPC = new SerializedObject(wpc);

            TMP_Text txtWin = FindChildByName<TMP_Text>(winPanel, "TxtWin");
            if (txtWin != null)
                soWPC.FindProperty("_headerText").objectReferenceValue = txtWin;

            GameObject btnNext = FindChildGameObject(winPanel, "BtnNext");
            if (btnNext != null)
                soWPC.FindProperty("_nextLevelButton").objectReferenceValue = btnNext;

            SerializedProperty starsProp = soWPC.FindProperty("_stars");
            var stars = FindStars(winPanel);
            starsProp.arraySize = stars.Length;
            for (int i = 0; i < stars.Length; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];

            soWPC.ApplyModifiedProperties();

            winPanel.SetActive(true);
        }

        GameObject canvasHUD = GameObject.Find("Canvas_HUD");
        if (canvasHUD == null)
        {
            Debug.LogWarning("Canvas_HUD not found. Skipping HUDController setup.");
        }
        else
        {
            HUDController hud = canvasHUD.GetComponent<HUDController>();
            if (hud == null)
            {
                hud = canvasHUD.AddComponent<HUDController>();
                Debug.Log("Added HUDController to Canvas_HUD.");
            }

            SerializedObject soHUD = new SerializedObject(hud);

            TMP_Text levelNameTxt = FindChildByName<TMP_Text>(canvasHUD, "LevelName");
            if (levelNameTxt != null)
                soHUD.FindProperty("_levelNameText").objectReferenceValue = levelNameTxt;

            TMP_Text moveCountTxt = FindChildByName<TMP_Text>(canvasHUD, "MoveCount");
            if (moveCountTxt == null)
            {
                GameObject topBar = FindChildGameObject(canvasHUD, "TopBar") ?? canvasHUD;
                moveCountTxt = CreateTMPText(topBar, "MoveCount", "Moves: 0", 28);
                Debug.Log("Created MoveCount TMP_Text under TopBar.");
            }
            soHUD.FindProperty("_moveCountText").objectReferenceValue = moveCountTxt;

            AssignLevelsToHUD(soHUD);

            soHUD.ApplyModifiedProperties();
        }

        UIManager uiMgr = Object.FindAnyObjectByType<UIManager>();
        if (uiMgr != null && winPanel != null)
        {
            SerializedObject soUI = new SerializedObject(uiMgr);
            SerializedProperty wpProp = soUI.FindProperty("_winPanel");
            SerializedProperty ppProp = soUI.FindProperty("_pausePanel");

            if (wpProp != null && wpProp.objectReferenceValue == null)
                wpProp.objectReferenceValue = winPanel;

            if (ppProp != null && ppProp.objectReferenceValue == null)
            {
                GameObject pausePanel = GameObject.Find("PausePanel");
                if (pausePanel != null)
                    ppProp.objectReferenceValue = pausePanel;
            }

            soUI.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Light Beam",
            "Game scene setup complete!\n\nPress Ctrl+S to save the scene.",
            "OK");

        Debug.Log("Done. Save the scene with Ctrl+S.");
    }

    private static void EnsureGridContainer(GridManager gridMgr)
    {
        GameObject canvasHUD = GameObject.Find("Canvas_HUD");
        if (canvasHUD == null)
        {
            Debug.LogWarning("Canvas_HUD not found — GridContainer not created. " +
                             "Run 'Setup Game Scene' again after creating the canvas.");
            return;
        }

        Transform existing = canvasHUD.transform.Find("GridContainer");
        RectTransform container;
        if (existing != null)
        {
            container = existing as RectTransform;
        }
        else
        {
            var go = new GameObject("GridContainer", typeof(RectTransform));
            go.transform.SetParent(canvasHUD.transform, false);
            container = go.GetComponent<RectTransform>();

            container.anchorMin        = new Vector2(0f, 0f);
            container.anchorMax        = new Vector2(1f, 1f);
            container.offsetMin        = Vector2.zero;
            container.offsetMax        = Vector2.zero;
            container.anchoredPosition = Vector2.zero;

            Debug.Log("Created GridContainer inside Canvas_HUD.");
        }

        SerializedObject so = new SerializedObject(gridMgr);
        var prop = so.FindProperty("_gridContainer");
        if (prop != null && prop.objectReferenceValue == null)
        {
            prop.objectReferenceValue = container;
            so.ApplyModifiedProperties();
        }
    }

    private static void AssignPrefabsToGridManager(GridManager gm)
    {
        SerializedObject so = new SerializedObject(gm);

        TryAssignPrefab(so, "_mirrorPrefab",  LightBeamEditorConfig.MirrorPrefabPath);
        TryAssignPrefab(so, "_targetPrefab",  LightBeamEditorConfig.TargetPrefabPath);
        TryAssignPrefab(so, "_emitterPrefab", LightBeamEditorConfig.EmitterPrefabPath);
        TryAssignPrefab(so, "_wallPrefab",    LightBeamEditorConfig.WallPrefabPath);

        so.ApplyModifiedProperties();
    }

    private static void TryAssignPrefab(SerializedObject so, string propName, string assetPath)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) return;
        if (prop.objectReferenceValue != null) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab != null)
            prop.objectReferenceValue = prefab;
        else
            Debug.LogWarning("Prefab not found at " + assetPath);
    }

    private static void AssignLevelsToLevelManager(LevelManager lm)
    {
        SerializedObject so = new SerializedObject(lm);
        var prop = so.FindProperty("_levels");
        if (prop == null) return;

        FillLevelsProperty(prop);
        so.ApplyModifiedProperties();
    }

    private static void AssignLevelsToHUD(SerializedObject so)
    {
        var prop = so.FindProperty("_levels");
        if (prop == null) return;

        FillLevelsProperty(prop);
    }

    private static void FillLevelsProperty(SerializedProperty prop)
    {
        string[] paths = LightBeamEditorConfig.LevelDataAssetPaths;
        prop.arraySize = paths.Length;
        for (int i = 0; i < paths.Length; i++)
        {
            var ld = AssetDatabase.LoadAssetAtPath<LevelData>(paths[i]);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = ld;
        }
    }

    private static T FindChildByName<T>(GameObject root, string name) where T : Component
    {
        foreach (var t in root.GetComponentsInChildren<T>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    private static GameObject FindChildGameObject(GameObject root, string name)
    {
        var t = root.transform.Find(name);
        if (t != null) return t.gameObject;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child.gameObject;

        return null;
    }

    private static GameObject[] FindStars(GameObject parent)
    {
        var result = new List<GameObject>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            string n = child.name.ToLower();
            if (n.StartsWith("star") || n == "star")
                result.Add(child.gameObject);
        }
        return result.ToArray();
    }

    private static TMP_Text CreateTMPText(GameObject parent, string name, string text, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(300f, 40f);
        rt.anchoredPosition = new Vector2(0f, 0f);

        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Left;

        return tmp;
    }

    private static void AssignCameraToCanvas(Camera cam)
    {
        const float planeDistance = 9f;
        bool changed = false;

        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.name != "Canvas_HUD")
                continue;

            Undo.RecordObject(canvas, "Canvas_HUD Screen Space Camera");
            canvas.renderMode    = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera   = cam;
            canvas.planeDistance = planeDistance;
            EditorUtility.SetDirty(canvas);
            changed = true;
            Debug.Log($"Canvas_HUD → Screen Space Camera (plane {planeDistance}), camera '{cam.name}'.");
        }

        if (changed)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}

/// <summary>
/// Generates blank scaffold scenes and merges them into Build Settings (keeps other scenes).
/// </summary>
public class LightBeamSceneGeneratorWindow : EditorWindow
{
    private bool _generateBoot        = true;
    private bool _generateMainMenu    = true;
    private bool _generateLevelSelect = true;
    private bool _generateGame        = true;
    private bool _createFolders       = true;
    private bool _addToBuildSettings  = true;

    [MenuItem("Tools/Light Beam/Generate Scenes")]
    public static void ShowWindow()
    {
        var window = GetWindow<LightBeamSceneGeneratorWindow>("Scene Generator");
        window.minSize = new Vector2(360, 280);
    }

    private void OnGUI()
    {
        GUILayout.Label("Light Beam — Scene Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        _createFolders = EditorGUILayout.Toggle("Создать папки", _createFolders);
        EditorGUILayout.Space(4);

        GUILayout.Label("Сцены для генерации:", EditorStyles.miniBoldLabel);
        _generateBoot        = EditorGUILayout.Toggle("Boot",        _generateBoot);
        _generateMainMenu    = EditorGUILayout.Toggle("MainMenu",    _generateMainMenu);
        _generateLevelSelect = EditorGUILayout.Toggle("LevelSelect", _generateLevelSelect);
        _generateGame        = EditorGUILayout.Toggle("Game",        _generateGame);

        EditorGUILayout.Space(4);
        _addToBuildSettings = EditorGUILayout.Toggle("Добавить в Build Settings", _addToBuildSettings);

        EditorGUILayout.Space(10);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate All", GUILayout.Height(36)))
                GenerateAll();

            if (GUILayout.Button("Only Folders", GUILayout.Height(36)))
                CreateFolders();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Сгенерированные сцены объединяются с текущим списком Build Settings: " +
            "пути Boot / MainMenu / LevelSelect / Game обновляются, остальные сцены сохраняются.",
            MessageType.Info);
    }

    private void GenerateAll()
    {
        if (_createFolders)
            CreateFolders();

        var generated = new List<EditorBuildSettingsScene>();

        if (_generateBoot)
            generated.Add(new EditorBuildSettingsScene(BuildScene(LightBeamEditorConfig.SceneBootName, SetupBootScene), true));

        if (_generateMainMenu)
            generated.Add(new EditorBuildSettingsScene(BuildScene(LightBeamEditorConfig.SceneMainMenuName, SetupMainMenuScene), true));

        if (_generateLevelSelect)
            generated.Add(new EditorBuildSettingsScene(BuildScene(LightBeamEditorConfig.SceneLevelSelectName, SetupLevelSelectScene), true));

        if (_generateGame)
            generated.Add(new EditorBuildSettingsScene(BuildScene(LightBeamEditorConfig.SceneGameName, SetupGameScene), true));

        if (_addToBuildSettings && generated.Count > 0)
            MergeIntoBuildSettings(generated);

        AssetDatabase.Refresh();
        Debug.Log("Готово!");
        EditorUtility.DisplayDialog("Light Beam", $"Сгенерировано {generated.Count} сцен.", "OK");
    }

    private static void MergeIntoBuildSettings(List<EditorBuildSettingsScene> generated)
    {
        var byPath = new Dictionary<string, EditorBuildSettingsScene>();
        foreach (var s in EditorBuildSettings.scenes)
            byPath[s.path] = s;

        foreach (var g in generated)
            byPath[g.path] = g;

        string[] preferredOrder =
        {
            LightBeamEditorConfig.SceneAssetPath(LightBeamEditorConfig.SceneBootName),
            LightBeamEditorConfig.SceneAssetPath(LightBeamEditorConfig.SceneMainMenuName),
            LightBeamEditorConfig.SceneAssetPath(LightBeamEditorConfig.SceneLevelSelectName),
            LightBeamEditorConfig.SceneAssetPath(LightBeamEditorConfig.SceneGameName),
        };

        var result = new List<EditorBuildSettingsScene>();
        var used = new HashSet<string>();

        foreach (var path in preferredOrder)
        {
            if (byPath.TryGetValue(path, out var scene))
            {
                result.Add(scene);
                used.Add(path);
            }
        }

        foreach (var kvp in byPath)
        {
            if (!used.Contains(kvp.Key))
                result.Add(kvp.Value);
        }

        EditorBuildSettings.scenes = result.ToArray();
        Debug.Log("[LightBeamSceneGenerator] Build Settings обновлён (merge), всего сцен: " + result.Count);
    }

    private string BuildScene(string sceneName, System.Action<Scene> setup)
    {
        string path = LightBeamEditorConfig.SceneAssetPath(sceneName);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        setup?.Invoke(scene);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[LightBeamSceneGenerator] Сцена сохранена: " + path);
        return path;
    }

    private void SetupBootScene(Scene scene)
    {
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.04f, 0.06f, 0.14f);
        cam.orthographic       = true;
        cam.orthographicSize   = 5f;
        cam.nearClipPlane      = 0.3f;
        cam.farClipPlane       = 100f;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.tag              = "MainCamera";

        var bootGO = new GameObject("GameBootstrapper");
        bootGO.AddComponent<GameBootstrapper>();

        SceneManager.SetActiveScene(scene);
    }

    private void SetupMainMenuScene(Scene scene)
    {
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.04f, 0.06f, 0.14f);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane    = 0.3f;
        cam.farClipPlane     = 100f;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.tag            = "MainCamera";

        var canvasGO = CreateUICanvas("Canvas_MainMenu");

        var bg = CreateUIPanel(canvasGO, "Background", Vector2.zero, new Vector2(1920, 1080));
        bg.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.14f);

        CreateEmpty(canvasGO, "BeamLayer");
        CreateEmpty(canvasGO, "Logo");
        CreateEmpty(canvasGO, "MenuButtons");

        var uiGO = new GameObject("UIManager");
        uiGO.AddComponent<UIManager>();

        SceneManager.SetActiveScene(scene);
    }

    private void SetupLevelSelectScene(Scene scene)
    {
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.04f, 0.06f, 0.14f);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane    = 0.3f;
        cam.farClipPlane     = 100f;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.tag            = "MainCamera";

        var canvasGO = CreateUICanvas("Canvas_LevelSelect");

        CreateEmpty(canvasGO, "Header");
        CreateEmpty(canvasGO, "LevelGrid");
        CreateEmpty(canvasGO, "BackButton");

        var uiGO = new GameObject("UIManager");
        uiGO.AddComponent<UIManager>();

        SceneManager.SetActiveScene(scene);
    }

    private void SetupGameScene(Scene scene)
    {
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.04f, 0.06f, 0.14f);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane    = 0.3f;
        cam.farClipPlane     = 100f;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.tag            = "MainCamera";

        var gridGO = new GameObject("GridManager");
        gridGO.AddComponent<GridManager>();

        var canvasGO = CreateUICanvas("Canvas_HUD", forGame: true);
        CreateEmpty(canvasGO, "TopBar");
        CreateEmpty(canvasGO, "WinPanel");
        CreateEmpty(canvasGO, "PausePanel");

        var uiGO = new GameObject("UIManager");
        uiGO.AddComponent<UIManager>();

        if (FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length == 0)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        SceneManager.SetActiveScene(scene);
    }

    private static void CreateFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Scripts");
        EnsureFolder("Assets", "Prefabs");
        AssetDatabase.Refresh();
        Debug.Log("[LightBeamSceneGenerator] Папки созданы.");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string full = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static GameObject CreateUICanvas(string name, bool forGame = false)
    {
        var go     = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();

        if (forGame)
        {
            canvas.renderMode    = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 9f;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        canvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = forGame ? new Vector2(800, 600) : new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
        go.AddComponent<Image>();
        return go;
    }

    private static GameObject CreateEmpty(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }
}
