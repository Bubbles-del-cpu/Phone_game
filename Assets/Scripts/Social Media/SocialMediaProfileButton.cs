using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

public class SocialMediaProfileButton : MonoBehaviour
{
    [SerializeField] private Image _icon;
    public Image Icon => _icon;
    [SerializeField] private Button _button;
    [SerializeField] private DialogueCharacterSO _assignedCharacter;


    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        var command = new SocialMediaProfileOpenCommand(_assignedCharacter);
        NavigationManager.Instance.InvokeCommand(command, allowUndo: true);
    }

    public void Initialize(DialogueCharacterSO character)
    {
        _assignedCharacter = character;
        _assignedCharacter.SocialMediaProfile.SetupProfileButton(this);
    }
}