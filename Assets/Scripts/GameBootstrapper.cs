using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Точка входа в игру. Живёт только в сцене Boot.
/// Инициализирует системы и переходит в MainMenu.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Загрузка")]
    [SerializeField] private float _splashDuration = 1.5f;
    [SerializeField] private string _firstScene    = "MainMenu";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start() => StartCoroutine(Boot());
    private IEnumerator Boot()
    {
        Debug.Log("[Boot] Инициализация...");

        yield return InitSystems();

        yield return new WaitForSeconds(_splashDuration);

        Debug.Log($"[Boot] Переход в {_firstScene}");
        SceneManager.LoadScene(_firstScene);
    }

    private IEnumerator InitSystems()
    {
        yield return null;
    }
}
