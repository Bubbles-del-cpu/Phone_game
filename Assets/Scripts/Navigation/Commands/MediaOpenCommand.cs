public class MediaOpenCommand : PanelOpenCommand
{
    private UIPanel targetPanel;
    private bool _openFromConversation;
    public MediaOpenCommand(UICanvas canvasPanel, bool openState, UIPanel targetMediaPanel, bool openedFromConversation) : base(canvasPanel, openState)
    {
        targetPanel = targetMediaPanel;
        _openFromConversation = openedFromConversation;
    }

    public override void Execute()
    {
        targetPanel.Open();
        base.Execute();
    }

    public override void Undo()
    {
        targetPanel.Close();

        var galleryCanvas = (GalleryCanvas)_panel;
        galleryCanvas.Close(_openFromConversation);
    }
}
