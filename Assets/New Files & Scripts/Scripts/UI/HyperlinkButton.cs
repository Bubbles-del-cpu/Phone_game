using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HyperlinkButton : MonoBehaviour
{
    public string Link;

    private Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Click);
    }

    private void Click()
    {
        Application.OpenURL(Link);
    }
}