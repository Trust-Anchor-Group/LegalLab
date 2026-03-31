using System.ComponentModel;

namespace LegalLabMaui.Models;

public interface IProperty : INotifyPropertyChanged
{
    string Name { get; }
    IModel Model { get; }
    object UntypedValue { get; set; }
}
