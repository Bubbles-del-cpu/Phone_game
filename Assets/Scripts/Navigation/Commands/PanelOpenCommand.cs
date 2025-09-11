public class PanelOpenCommand : ICommand
{
    protected UICanvas _panel;
    protected bool _openState;
    public PanelOpenCommand(UICanvas canvasPanel, bool openState)
    {
        _panel = canvasPanel;
        _openState = openState;
    }
    public virtual void Execute()
    {
        _panel.Canvas.enabled = _openState;
    }

    public virtual void Undo()
    {
        _panel.Canvas.enabled = !_openState;
    }
}