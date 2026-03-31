using System.ComponentModel;
using System.Threading.Tasks;

namespace LegalLabMaui.Models;

public interface IModel : INotifyPropertyChanged
{
    void RaisePropertyChanged(string PropertyName);
    Task Start();
    Task Stop();
    bool Started { get; }
}
