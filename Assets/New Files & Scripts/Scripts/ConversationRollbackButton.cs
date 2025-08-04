using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ConversationRollbackButton : MonoBehaviour
{
    private Button _button;

    public bool Interactable
    {
        get => _button.interactable;
        set
        {
            _button.interactable = value;
        }
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(RollBack);
    }

    private void RollBack()
    {
        DialogueManager.Instance.Rollback();
    }
}
