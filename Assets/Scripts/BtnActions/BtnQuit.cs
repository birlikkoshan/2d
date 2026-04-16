using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BtnQuit : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
