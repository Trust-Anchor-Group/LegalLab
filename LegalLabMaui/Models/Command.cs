using System.Windows.Input;
using Waher.Events;

namespace LegalLabMaui.Models;

public delegate bool CanExecuteHandler();
public delegate Task ExecuteHandler();

public class Command(CanExecuteHandler? CanExecuteCallback, ExecuteHandler ExecuteCallback) : ICommand
{
    private readonly CanExecuteHandler? canExecuteCallback = CanExecuteCallback;
    private readonly ExecuteHandler executeCallback = ExecuteCallback;

    public Command(ExecuteHandler ExecuteCallback)
        : this(null, ExecuteCallback)
    {
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        AppService.UpdateGui(() =>
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        });
    }

    public bool CanExecute(object? parameter)
    {
        if (this.canExecuteCallback is null)
            return true;

        try
        {
            return this.canExecuteCallback();
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return false;
        }
    }

    public async void Execute(object? parameter)
    {
        if (this.executeCallback is not null)
        {
            try
            {
                await this.executeCallback();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                AppService.ErrorBox(ex.Message);
            }
        }
    }
}
