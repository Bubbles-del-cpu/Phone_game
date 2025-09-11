public class GalleryTabSelectCommand : ICommand
{
    private MediaType _type;
    private MediaType _previousType;
    public GalleryTabSelectCommand(MediaType targetType, MediaType previousType)
    {
        _type = targetType;
        _previousType = previousType;
    }

    public void Execute()
    {
        GameManager.Instance.GalleryCanvas.ShowGalleryTable(_type);
    }

    public void Undo()
    {
        GameManager.Instance.GalleryCanvas.ShowGalleryTable(_previousType);
    }
}