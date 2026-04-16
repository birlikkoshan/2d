using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class BtnNextLevel : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        int current = PlayerPrefs.GetInt("SelectedLevel", 0);
        PlayerPrefs.SetInt("SelectedLevel", current + 1);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }
}
