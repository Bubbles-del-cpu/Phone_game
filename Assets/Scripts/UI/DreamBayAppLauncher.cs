using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class DreamBayAppLauncher : MonoBehaviour
{
    [SerializeField] private DreamBayCanvas _canvas;

    private Button _button;

    public DreamBayCanvas Canvas => _canvas;

    public void Configure(DreamBayCanvas canvas)
    {
        _canvas = canvas;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OpenDreamBay);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OpenDreamBay);
    }

    public void OpenDreamBay()
    {
        if (_canvas != null)
            _canvas.Open();
    }
}
