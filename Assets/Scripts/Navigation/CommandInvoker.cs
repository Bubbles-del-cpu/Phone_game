using System.Collections.Generic;

public class CommandInvoker
{
    private Stack<ICommand> _commands;

    public CommandInvoker()
    {
        //Stack of commands executed by this invoker
        _commands = new Stack<ICommand>();
    }

    //Add a new command to the stack and execute
    public void AddCommand(ICommand newCommand, bool allowUndo = true)
    {
        newCommand.Execute();
        if (allowUndo)
            _commands.Push(newCommand);
    }

    //Undo a the lash pushed command
    public void UndoCommand()
    {
        if (_commands.Count > 0)
        {
            var command = _commands.Peek();
            command.Undo();

            _commands.Pop();
        }
    }

    public void UndoAll()
    {
        while (_commands.Count > 0)
        {
            UndoCommand();
        }
    }

    public void ClearCommands()
    {
        //When end turn is pressed we need to clear the commands as the Player is only allow to reset the current turn
        _commands.Clear();
    }
}