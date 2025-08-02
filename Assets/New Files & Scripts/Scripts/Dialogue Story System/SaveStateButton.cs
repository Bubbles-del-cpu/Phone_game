using UnityEngine;
using UnityEngine.UI;

public class SaveStateButton : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _clearSaveButton;
    [SerializeField] private SaveStateDialogBox _saveDialogPrefab;

    [SerializeField]
    private TMPro.TMP_Text _label;

    public int SlotNumber = 0;
    private bool _exists => SaveAndLoadManager.Instance.SaveStateExists(SlotNumber);
    private string _slotName
    {
        get
        {
            if (_exists)
            {
                return SaveAndLoadManager.Instance.CurrentSave.SaveStates[SlotNumber].Name;
            }

            return $"Empty Save Slot";
        }
    }

    private void Start()
    {
        _label.text = "Emtpy Save Slot";

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
            GameManager.Instance.DisplayDialog(GameConstants.DialogTexts.SAVE_STATE_DELETE, () =>
            {
                SaveAndLoadManager.Instance.ClearSaveStateSlot(SlotNumber);
            });
        });
    }

    private void Update()
    {
        _label.text = _slotName;
        var exists = _exists;
        _clearSaveButton.interactable = exists;
        _loadButton.interactable = exists;
        _saveButton.interactable = true;
    }
}
