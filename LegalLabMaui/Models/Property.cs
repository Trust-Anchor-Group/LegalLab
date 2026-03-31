using System.ComponentModel;
using Waher.Events;

namespace LegalLabMaui.Models;

public class Property<T>(string Name, T DefaultValue, IModel Model) : IProperty
{
    protected T @value = DefaultValue;
    private readonly string name = Name;
    private readonly IModel model = Model;

    public string Name => this.name;
    public IModel Model => this.model;

    public object UntypedValue
    {
        get => this.Value!;
        set
        {
            if (value is T Typed)
                this.Value = Typed;
            else
                throw new ArgumentException("Expected value of type " + typeof(T).FullName, nameof(this.UntypedValue));
        }
    }

    public virtual T Value
    {
        get => this.@value;
        set
        {
            if (this.@value is null && value is null)
                return;

            if (this.@value?.Equals(value) ?? false)
                return;

            this.@value = value;

            PropertyChangedEventHandler? h = this.PropertyChanged;
            if (h is not null)
            {
                try
                {
                    h.Invoke(this, new PropertyChangedEventArgs(this.name));
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }

            this.model.RaisePropertyChanged(this.name);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
