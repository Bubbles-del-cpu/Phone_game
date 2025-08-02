using UnityEngine;
using UnityEngine.UI;

public class DialogWarningMessage : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text _textField;

    [SerializeField]
    private System.Action _onConfirm;

    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    public void Setup(string message, System.Action eventToTrigger, string confirmButtonText, bool twoButtonSetup, string cancelButtonTest)
    {
        if (_textField)
        {
            _textField.text = message == string.Empty ? "Are you sure?" : message;
            _textField.text = GameManager.ToUTF32(_textField.text);
        }
        _onConfirm = eventToTrigger;

        _confirmButton.GetComponentInChildren<TMPro.TMP_Text>().text = confirmButtonText;
        _cancelButton.GetComponentInChildren<TMPro.TMP_Text>().text = cancelButtonTest;

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
