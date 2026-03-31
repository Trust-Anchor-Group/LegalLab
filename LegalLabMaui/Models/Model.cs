using System.ComponentModel;
using System.Threading.Tasks;
using Waher.Events;

namespace LegalLabMaui.Models;

public abstract class Model : IModel
{
    private bool started = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void RaisePropertyChanged(string PropertyName)
    {
        try
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }

    public virtual Task Start()
    {
        this.started = true;
        return Task.CompletedTask;
    }

    public virtual Task Stop()
    {
        this.started = false;
        return Task.CompletedTask;
    }

    public bool Started => this.started;
}
