using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputFieldDialogBox : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("_saveButton")] protected Button _submitButton;
    [SerializeField] protected Button _cancelButton;
    [SerializeField] protected TMPro.TMP_InputField _inputField;
    public UnityEvent OnSubmit;
    public UnityEvent OnCancel;

    private void Awake()
    {
        _submitButton.onClick.AddListener(Submit);
        _cancelButton.onClick.AddListener(Cancel);
    }

    public virtual void Submit()
    {
        Destroy(gameObject);
        OnSubmit?.Invoke();
    }

    public void Cancel()
    {
        Destroy(gameObject);
        OnCancel?.Invoke();
    }
}
