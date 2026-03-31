using System.Windows.Input;
using Waher.Events;

namespace LegalLabMaui.Models;

public delegate bool CanExecuteParametrizedHandler(object? Parameter);
public delegate void ExecuteParametrizedHandler(object? Parameter);

public class ParametrizedCommand(CanExecuteParametrizedHandler? CanExecuteCallback, ExecuteParametrizedHandler ExecuteCallback) : ICommand
{
    private readonly CanExecuteParametrizedHandler? canExecuteCallback = CanExecuteCallback;
    private readonly ExecuteParametrizedHandler executeCallback = ExecuteCallback;

    public ParametrizedCommand(ExecuteParametrizedHandler ExecuteCallback)
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
            return this.canExecuteCallback(parameter);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return false;
        }
    }

    public void Execute(object? parameter)
    {
        if (this.executeCallback is not null)
        {
            try
            {
                this.executeCallback(parameter);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                AppService.ErrorBox(ex.Message);
            }
        }
    }
}
