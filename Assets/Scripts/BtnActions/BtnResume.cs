using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BtnResume : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_pausePanel == null)
            _pausePanel = GameObject.Find("PausePanel");

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
