using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class CalcParameterInfo : ParameterInfo
{
    public CalcParameterInfo(Contract Contract, CalcParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public override bool CanEditValue => false;

    protected override void SetParameterValue(object? value) { /* Calculated - read-only */ }
}
