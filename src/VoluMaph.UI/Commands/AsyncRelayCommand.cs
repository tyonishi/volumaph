using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace VoluMaph.UI.Commands;

/// <summary>
/// Implementation of ICommand for asynchronous WPF binding.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private int _isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The asynchronous action to execute.</param>
    /// <param name="canExecute">Optional function to determine if command can execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when execute parameter is null.</exception>
    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when the ability to execute the command changes.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>True if the command can execute; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        return Interlocked.CompareExchange(ref _isExecuting, 0, 0) == 0
            && (_canExecute?.Invoke(parameter) ?? true);
    }

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public async void Execute(object? parameter)
    {
        if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
            return;

        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AsyncRelayCommand error: {ex}");
        }
        finally
        {
            Interlocked.Exchange(ref _isExecuting, 0);
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler == null) return;

        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            handler(this, EventArgs.Empty);
        }
        else
        {
            dispatcher.BeginInvoke(new Action(() => handler(this, EventArgs.Empty)), DispatcherPriority.Normal);
        }
    }
}
