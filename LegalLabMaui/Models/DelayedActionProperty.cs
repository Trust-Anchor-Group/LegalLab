using Waher.Events;

namespace LegalLabMaui.Models;

public class DelayedActionProperty<T>(string Name, TimeSpan Delay, bool LiveUpdates,
    T DefaultValue, IModel Model)
    : Property<T>(Name, DefaultValue, Model), IDelayedAction
{
    private readonly bool liveUpdates = LiveUpdates;
    private readonly TimeSpan delay = Delay;
    private DateTime scheduledFor = DateTime.MinValue;
    private bool changed = false;

    public DelayedActionProperty(string Name, TimeSpan Delay, T DefaultValue, IModel Model)
        : this(Name, Delay, true, DefaultValue, Model)
    {
    }

    public bool LiveUpdates => this.liveUpdates;

    public override T Value
    {
        get => this.@value;
        set
        {
            if (this.@value is null && value is null)
                return;

            if (this.@value?.Equals(value) ?? false)
                return;

            this.@value = value;
            this.changed = true;

            if (this.liveUpdates)
                DelayedActions.Add(this, DateTime.Now + this.delay);

            this.Model.RaisePropertyChanged(this.Name);
        }
    }

    public bool Changed
    {
        get => this.changed;
        protected set => this.changed = value;
    }

    public DateTime ScheduledFor
    {
        get => this.scheduledFor;
        set => this.scheduledFor = value;
    }

    public virtual Task Action()
    {
        try
        {
            this.OnAction?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }

        this.changed = false;
        return Task.CompletedTask;
    }

    public event EventHandler? OnAction;
}
