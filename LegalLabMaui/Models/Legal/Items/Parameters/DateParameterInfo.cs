using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class DateParameterInfo : ParameterInfo
{
    public DateParameterInfo(Contract Contract, DateParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public DateTime DateValue
    {
        get => this.Value is DateTime dt ? dt : DateTime.Today;
        set { this.Value = value; }
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is DateParameter dp && value is DateTime dt)
            dp.Value = dt;
    }
}
