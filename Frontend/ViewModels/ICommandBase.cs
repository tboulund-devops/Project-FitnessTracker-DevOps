using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Frontend.ViewModels;

/// <summary>
/// this serves as a bridge between UI actions and ViewModel methods
/// </summary>
public class ICommandBase : ICommand
{
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _executeAction;
    private readonly Func<object?, bool> _canExecuteAction;
    private bool _isExecuting;

    /// <summary>
    /// Constructor for async commands
    /// </summary>
    public ICommandBase(Func<object?, Task> executeAsync, Func<object?, bool>? canExecuteAction = null)
    {
        _executeAsync = executeAsync;
        _executeAction = null;
        _canExecuteAction = canExecuteAction ?? DefaultCanExecute;
    }

    /// <summary>
    /// Constructor for sync commands with parameter
    /// </summary>
    public ICommandBase(Action<object?> executeAction, Func<object?, bool>? canExecuteAction = null)
    {
        _executeAction = executeAction;
        _executeAsync = null;
        _canExecuteAction = canExecuteAction ?? DefaultCanExecute;
    }

    /// <summary>
    /// Constructor for sync commands without parameter
    /// </summary>
    public ICommandBase(Action executeAction, Func<object?, bool>? canExecuteAction = null)
        : this(_ => executeAction(), canExecuteAction)
    {
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && _canExecuteAction(parameter);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            if (_executeAsync != null)
            {
                await _executeAsync(parameter);
            }
            else
            {
                _executeAction?.Invoke(parameter);
            }
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool DefaultCanExecute(object parameter) => true;
}