using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class StringParameterInfo : ParameterInfo
{
    public StringParameterInfo(Contract Contract, StringParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public string StringValue
    {
        get => this.Value as string ?? string.Empty;
        set { this.Value = value; }
    }

    public override void SetValue(string Value)
    {
        this.StringValue = Value;
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is StringParameter sp)
            sp.Value = value as string ?? string.Empty;
    }
}
