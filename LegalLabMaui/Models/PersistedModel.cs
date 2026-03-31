using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegalLabMaui.Models;

public abstract class PersistedModel : Model
{
    private readonly LinkedList<IPersistedProperty> properties = new();

    protected void Add(IPersistedProperty Property)
    {
        this.properties.AddLast(Property);
    }

    public async Task Load()
    {
        await AppService.WaitForDB();

        foreach (IPersistedProperty property in this.properties)
            await property.Load();
    }

    public async Task Save()
    {
        foreach (IPersistedProperty property in this.properties)
            await property.Save();
    }
}
