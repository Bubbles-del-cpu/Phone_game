public class MediaOpenCommand : PanelOpenCommand
{
    private UIPanel targetPanel;
    private bool _openFromConversation;
    public MediaOpenCommand(UICanvas canvasPanel, bool openState, UIPanel targetMediaPanel, bool openedFromConversation) : base(canvasPanel, openState)
    {
        targetPanel = targetMediaPanel;
        _openFromConversation = openedFromConversation;
    }

    protected override void Open()
    {
        targetPanel.Open();
        base.Open();
    }

    protected override void Close()
    {
        var galleryCanvas = (GalleryCanvas)_panel;
        galleryCanvas.Close(_openFromConversation);

        targetPanel.Close();

        if (!_openFromConversation)
            NavigationManager.Instance.PanelOpenCount -= 1;
    }
}
