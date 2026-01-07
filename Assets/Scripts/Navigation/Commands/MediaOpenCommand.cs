public class MediaOpenCommand : PanelOpenCommand
{
    private UIPanel targetPanel;
    private bool _openedFromGallery;
    public MediaOpenCommand(UICanvas canvasPanel, bool openState, UIPanel targetMediaPanel, bool openedFromGallery) : base(canvasPanel, openState)
    {
        targetPanel = targetMediaPanel;
        _openedFromGallery = openedFromGallery;
    }

    protected override void Open()
    {
        targetPanel.Open();
        base.Open();
    }

    protected override void Close()
    {
        var galleryCanvas = (GalleryCanvas)_panel;
        galleryCanvas.Close(!_openedFromGallery);

        targetPanel.Close();

        if (_openedFromGallery)
            NavigationManager.Instance.PanelOpenCount -= 1;
    }
}
