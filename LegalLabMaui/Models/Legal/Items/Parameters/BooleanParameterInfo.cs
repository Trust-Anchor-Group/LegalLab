using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class BooleanParameterInfo : ParameterInfo
{
    public BooleanParameterInfo(Contract Contract, BooleanParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public bool BoolValue
    {
        get => this.Value is bool b && b;
        set { this.Value = value; }
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is BooleanParameter bp)
            bp.Value = value is bool b && b;
    }
}
