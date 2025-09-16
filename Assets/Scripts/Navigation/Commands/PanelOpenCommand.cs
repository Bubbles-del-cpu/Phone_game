using System;

public class PanelOpenCommand : ICommand
{
    protected UICanvas _panel;
    protected bool _openState;
    private int _targetOrder = 5;

    private Action _undo, _execute;
    public PanelOpenCommand(UICanvas canvasPanel, bool openState, int overrideTarget = -1)
    {
        _panel = canvasPanel;
        _openState = openState;

        if (overrideTarget != -1)
            _targetOrder = overrideTarget;

        _undo = () =>
        {
            if (_openState)
                Close();
            else
                Open();
        };

        _execute = () =>
        {
            if (_openState)
                Open();
            else
                Close();
        };
    }
    public void Execute() => _execute();
    public void Undo() => _undo();

    protected virtual void Open()
    {
        _panel.Canvas.sortingOrder = _targetOrder + NavigationManager.Instance.PanelOpenCount;
        _panel.Canvas.enabled = true;
        NavigationManager.Instance.PanelOpenCount += 1;
    }

    protected virtual void Close()
    {
        _panel.Canvas.sortingOrder = -1;
        _panel.Canvas.enabled = false;
        NavigationManager.Instance.PanelOpenCount -= 1;
    }
}