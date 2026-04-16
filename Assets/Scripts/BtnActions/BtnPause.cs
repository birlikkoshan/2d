using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BtnPause : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_pausePanel == null) return;
        bool active = !_pausePanel.activeSelf;
        _pausePanel.SetActive(active);
        Time.timeScale = active ? 0f : 1f;
    }
}
