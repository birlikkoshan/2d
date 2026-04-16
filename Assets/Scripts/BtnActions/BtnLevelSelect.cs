using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach to each level button in the LevelSelect scene.
/// Set <see cref="_levelIndex"/> to 1, 2, or 3 in the Inspector (1-based display label).
/// Internally maps to 0-based save/load index automatically.
/// Locks the button if the level has not been unlocked yet.
/// </summary>
[RequireComponent(typeof(Button))]
public class BtnLevelSelect : MonoBehaviour
{
    [SerializeField] private int _levelIndex = 1; // 1-based: shown on button, set in Inspector

    private void Start()
    {
        int zeroBasedIndex = _levelIndex - 1;

        var tmp = GetComponentInChildren<TextMeshProUGUI>();

        bool unlocked = SaveSystem.IsLevelUnlocked(zeroBasedIndex);
        var btn       = GetComponent<Button>();
        var img       = GetComponent<Image>();

        if (unlocked)
        {
            if (tmp != null) tmp.text = _levelIndex.ToString();
            btn.interactable = true;

            btn.onClick.AddListener(() =>
            {
                SaveSystem.SelectLevel(zeroBasedIndex);
                SceneManager.LoadScene("Game");
            });
        }
        else
        {
            if (tmp != null) tmp.text = "?";
            if (img  != null) img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            btn.interactable = false;
        }
    }
}
