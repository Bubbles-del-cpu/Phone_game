using MeetAndTalk;

public class SocialMediaProfileOpenCommand : ICommand
{
    private DialogueCharacterSO _character;
    public SocialMediaProfileOpenCommand(DialogueCharacterSO character)
    {
        _character = character;
    }
    public void Execute()
    {
        SocialMediaCanvas.Instance.OpenProfilePage(_character);
    }

    public void Undo()
    {
        SocialMediaCanvas.Instance.CloseProfilePage();
    }
}