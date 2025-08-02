using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GalleryUnlockButton : MonoBehaviour
{
    [SerializeField] private GalleryUnlockDialog _prefab;

    private Button _button;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            var newDialog = Instantiate(_prefab);
            OverlayCanvas.Instance.ShowDialog(newDialog.gameObject);
        });
    }
}