using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputFieldDialogBox : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("_saveButton")] protected Button _submitButton;
    [SerializeField] protected Button _cancelButton;
    [SerializeField] protected TMPro.TMP_InputField _inputField;

    private void Awake()
    {
        _submitButton.onClick.AddListener(Submit);
        _cancelButton.onClick.AddListener(Cancel);
    }

    public virtual void Submit()
    {
        Destroy(gameObject);
    }

    public void Cancel()
    {
        Destroy(gameObject);
    }
}
