using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SaveStateButton : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _clearSaveButton;
    [SerializeField] private SaveStateDialogBox _saveDialogPrefab;
    public int SlotNumber = 0;

    [Header("Localization Components")]
    [SerializeField] private LocalizeStringEvent _localizedString;
    [SerializeField] private LocalizedString _emptySlotString;
    [SerializeField] private LocalizedString _filledSlotString;

    private bool _exists => SaveAndLoadManager.Instance.SaveStateExists(SlotNumber);

    private void Awake()
    {
        _saveButton.onClick.AddListener(() =>
        {
            var newDialog = Instantiate(_saveDialogPrefab);
            newDialog.Setup(SlotNumber);
            OverlayCanvas.Instance.ShowDialog(newDialog.gameObject);
        });

        _loadButton.onClick.AddListener(() =>
        {
            SaveAndLoadManager.Instance.LoadSaveSlot(SlotNumber);
        });

        _clearSaveButton.onClick.AddListener(() =>
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.SAVE_STATE_DELETE, () =>
            {
                SaveAndLoadManager.Instance.ClearSaveStateSlot(SlotNumber);
            });
        });
    }

    private void Update()
    {
        var exists = _exists; //Prevent checking the SaveAndLoadManager get function so many times
        if (exists)
        {
            _localizedString.StringReference.Arguments = new object[] { SlotNumber };
            _localizedString.StringReference = _filledSlotString;
        }
        else
        {
            _localizedString.StringReference.Arguments = null;
            _localizedString.StringReference = _emptySlotString;
        }

        _clearSaveButton.interactable = exists;
        _loadButton.interactable = exists;
        _saveButton.interactable = true;
    }
}
