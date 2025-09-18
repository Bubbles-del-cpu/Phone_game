using MeetAndTalk;

public class ConversationOpenCommand : PanelOpenCommand
{
    private DialogueCharacterSO _character;
    private bool _openedFromNotification;
    private bool _wasGalleryOpen;

    public ConversationOpenCommand(UICanvas canvasPanel, bool openState, DialogueCharacterSO character, bool fromNotification) : base(canvasPanel, openState)
    {
        _character = character;
        _openedFromNotification = fromNotification;
        _wasGalleryOpen = GameManager.Instance.GalleryCanvas.IsOpen;
    }

    protected override void Open()
    {
        GameManager.Instance.MessagingCanvas.SetupPanel(_character);
        base.Open();
    }

    protected override void Close()
    {
        GameManager.Instance.MessagingCanvas.Close(_openedFromNotification, _character);

        if (!_openedFromNotification)
            NavigationManager.Instance.PanelOpenCount -= 1;
    }
}