using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    private static NavigationManager _instance;
    public static NavigationManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<NavigationManager>();

            return _instance;
        }
    }

    public int PanelOpenCount = 0;

    private CommandInvoker _invoker;

    private void Awake()
    {
        _invoker = new();
    }

    public void ResetStack()
    {
        _invoker.ClearCommands();
    }

    public void UndoLast(bool performUndo = true)
    {
        _invoker.UndoCommand(performUndo);
    }

    public void InvokeCommand(ICommand commandToInvoke, bool allowUndo = true)
    {
        _invoker.AddCommand(commandToInvoke, allowUndo);
    }

    public void HomeButtonPressed()
    {
        _invoker.UndoAll();
        _invoker.ClearCommands();

        PanelOpenCount = 0;
    }

    public void BackButtonPressed()
    {
        UndoLast();
    }
}
