using MeetAndTalk;

public class SocialMediaProfileOpenCommand : ICommand
{
    private DialogueCharacterSO _character;
    private SocialMediaCanvas _canvas;
    public SocialMediaProfileOpenCommand(DialogueCharacterSO character, MediaTargetPlatform platform)
    {
        _character = character;
        switch (platform)
        {
            case MediaTargetPlatform.SocialMediaPost:
                _canvas = GameManager.Instance.SocialMediaCanvas;
                break;
            case MediaTargetPlatform.SpicySocialMediaPost:
                _canvas = GameManager.Instance.SpicySocialMediaCanvas;
                break;
        }
    }
    public void Execute()
    {
        _canvas.OpenProfilePage(_character);
    }

    public void Undo()
    {
        _canvas.CloseProfilePage();
    }
}