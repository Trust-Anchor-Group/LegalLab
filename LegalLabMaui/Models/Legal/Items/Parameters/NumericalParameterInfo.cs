using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class NumericalParameterInfo : ParameterInfo
{
    public NumericalParameterInfo(Contract Contract, NumericalParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public decimal DecimalValue
    {
        get => this.Value is decimal d ? d : 0m;
        set { this.Value = value; }
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is NumericalParameter np && value is decimal d)
            np.Value = d;
    }
}
