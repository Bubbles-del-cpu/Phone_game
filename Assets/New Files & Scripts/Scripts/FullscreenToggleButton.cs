using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FullscreenToggleButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            Screen.fullScreen = !Screen.fullScreen;
        });
    }
}