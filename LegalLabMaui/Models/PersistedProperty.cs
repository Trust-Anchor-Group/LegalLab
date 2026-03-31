using Waher.Runtime.Settings;

namespace LegalLabMaui.Models;

public class PersistedProperty<T>(string Category, string Name, bool LiveUpdates,
    T DefaultValue, IModel Model)
    : DelayedActionProperty<T>(Name, TimeSpan.FromSeconds(1), LiveUpdates, DefaultValue, Model),
      IPersistedProperty
{
    private readonly string category = Category;

    public string Category => this.category;

    public override async Task Action()
    {
        await this.Save();
        await base.Action();
    }

    public async Task Load()
    {
        this.@value = (T)await RuntimeSettings.GetAsync(this.category + "." + this.Name, this.@value);
    }

    public async Task Save()
    {
        if (this.Changed)
        {
            await RuntimeSettings.SetAsync(this.category + "." + this.Name, this.@value);
            this.Changed = false;
            DelayedActions.Remove(this);
        }
    }
}
