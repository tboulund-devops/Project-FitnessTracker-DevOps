using System;
using System.Windows.Input;

namespace FitnessTracker.UI.ViewModels;

/// <summary>
/// this serves as a bridge between UI actions and ViewModel methods
/// </summary>
public class ICommandBase : ICommand
{
    //Generic delegates
    public Action<object?> _executeAction;
    public Func<object?, bool> _canExecuteAction;

    /// <summary>
    /// Constructor for commands with parameter and custom canExecute logic
    /// </summary>
    /// <param name="executeAction"></param>
    /// <param name="canExecuteAction"></param>
    public ICommandBase(Action<object?> executeAction, Func<object?, bool>? canExecuteAction)
    {
        _executeAction = executeAction;
        _canExecuteAction = canExecuteAction ?? DefaultCanExecute; // Use default if none provided
    }

    /// <summary>
    /// Constructor for parameterless commands with custom canExecute logic
    /// </summary>
    /// <param name="executeAction"></param>
    /// <param name="canExecuteAction"></param>
    public ICommandBase(Action executeAction, Func<object?, bool>? canExecuteAction)
        : this(_ => executeAction(), canExecuteAction)
    {
        
    }
    

/// <summary>
/// Constructor for commands with parameter that always execute
/// </summary>
/// <param name="executeAction"></param>
    public ICommandBase(Action<object?> executeAction) : this(executeAction, DefaultCanExecute)
    {
        this._executeAction = executeAction;
    }
    

    /// <summary>
    /// Event that notifies when the CanExecute status changes
    /// CONNECTION: Avalonia automatically subscribes to this for UI updates
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Manually raises the CanExecuteChanged event
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Can execute, the method which enables or disables the UI element trough the Command Manager
    /// Will just forward invoke the call
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public bool CanExecute(object? parameter)
    {
        Boolean returnVal = true;
        if (_canExecuteAction != null)
            returnVal = _canExecuteAction(parameter);
        return returnVal;
    }

    /// <summary>
    /// The method which executes when an UI element is clicked
    /// WIll just forward invoke the call
    /// </summary>
    /// <param name="parameter"></param>
    public void Execute(object? parameter)
    {
        _executeAction?.Invoke(parameter);
    }
    
    /// <summary>
    /// Defines if command can be executed (default behaviour)
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>Always true</returns>
    private static bool DefaultCanExecute(object parameter)
    {
        return true;
    }
}
