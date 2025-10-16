using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DialogWarningMessage : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _textField;

    [SerializeField]
    private LocalizeStringEvent _localizedString;

    [SerializeField]
    private System.Action _onConfirm;

    [SerializeField]
    private Button _confirmButton;
    [SerializeField] private LocalizeStringEvent _confirmButtonLocalizedString;

    [SerializeField]
    private Button _cancelButton;
    [SerializeField] private LocalizeStringEvent _cancelButtonLocalizedString;

    private void Awake()
    {
        _localizedString.OnUpdateString.AddListener((newString) =>
        {
            _textField.text = GameManager.ToUTF32(newString);
        });
    }

    public void Setup(string message_key, System.Action eventToTrigger, string confirmButtonKey, bool twoButtonSetup, string cancelButtonKey, object[] args)
    {
        if (_localizedString)
        {
            _localizedString.StringReference.Arguments = args;
            _localizedString.StringReference.SetReference(GameConstants.DialogTextKeys.DIALOGUE_TABLE_KEY, message_key);
        }

        _onConfirm = eventToTrigger;
        _confirmButtonLocalizedString.StringReference.SetReference(GameConstants.UIElementKeys.UI_ELEMENTS_TABLE_KEY, confirmButtonKey);
        _cancelButtonLocalizedString.StringReference.SetReference(GameConstants.UIElementKeys.UI_ELEMENTS_TABLE_KEY, cancelButtonKey);

        if (!twoButtonSetup)
            _cancelButton.gameObject.SetActive(false);
    }

    public void OnConfirm()
    {
        if (_onConfirm != null)
        {
            _onConfirm.Invoke();
        }

        Close();
    }

    public void OnCancel()
    {
        Close();
    }

    private void Close()
    {
        // var overlayCanvas = FindFirstObjectByType<OverlayCanvas>();
        // if (overlayCanvas)
        // {
        //     overlayCanvas.Close();
        // }

        Destroy(gameObject);
    }
}
