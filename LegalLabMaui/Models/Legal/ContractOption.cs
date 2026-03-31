using Waher.Persistence;

namespace LegalLabMaui.Models.Legal;

public class ContractOption(string Name, IDictionary<CaseInsensitiveString, object> Option)
{
    public IDictionary<CaseInsensitiveString, object> Option { get; } = Option;
    public string Name { get; } = Name;

    public override string ToString() => this.Name;
}
