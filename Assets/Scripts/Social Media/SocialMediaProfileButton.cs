using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

public class SocialMediaProfileButton : MonoBehaviour
{
    [SerializeField] private Image _icon;
    public Image Icon => _icon;
    [SerializeField] private Button _button;
    [SerializeField] private DialogueCharacterSO _assignedCharacter;
    [SerializeField] private MediaTargetPlatform _platform;


    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            // In replays we cannot access the social media profile pages
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.WARNING_SOCIAL_MEDIA_ACCESS_DURING_REPLAY, null, GameConstants.UIElementKeys.CONTINUE, null, false);
            return;
        }

        var command = new SocialMediaProfileOpenCommand(_assignedCharacter, _platform);
        NavigationManager.Instance.InvokeCommand(command, allowUndo: true);
    }

    public void Initialize(DialogueCharacterSO character)
    {
        _assignedCharacter = character;
        _assignedCharacter.SocialMediaProfile.SetupProfileButton(this);
    }
}