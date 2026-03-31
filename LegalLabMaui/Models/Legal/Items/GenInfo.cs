namespace LegalLabMaui.Models.Legal.Items;

public class GenInfo(string Name, string Value)
{
    public string Name { get; internal set; } = Name;
    public string Value { get; internal set; } = Value;
}
