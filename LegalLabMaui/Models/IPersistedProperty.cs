using System.Threading.Tasks;

namespace LegalLabMaui.Models;

public interface IPersistedProperty : IProperty
{
    Task Load();
    Task Save();
}
