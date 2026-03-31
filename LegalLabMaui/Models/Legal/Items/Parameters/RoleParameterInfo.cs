using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class RoleParameterInfo : ParameterInfo
{
    public RoleParameterInfo(Contract Contract, RoleParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    protected override void SetParameterValue(object? value) { }
}
