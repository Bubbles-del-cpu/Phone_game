using MeetAndTalk;

public class ConversationOpenCommand : PanelOpenCommand
{
    private DialogueCharacterSO _character;
    private bool _openedFromNotification;

    public ConversationOpenCommand(UICanvas canvasPanel, bool openState, DialogueCharacterSO character, bool fromNotification) : base(canvasPanel, openState)
    {
        _character = character;
        _openedFromNotification = fromNotification;
    }

    public override void Execute()
    {
        GameManager.Instance.MessagingCanvas.SetupPanel(_character);
        base.Execute();
    }

    public override void Undo()
    {
        GameManager.Instance.MessagingCanvas.Close(_openedFromNotification, _character);
    }
}