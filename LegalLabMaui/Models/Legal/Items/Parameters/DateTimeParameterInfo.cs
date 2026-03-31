using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class DateTimeParameterInfo : ParameterInfo
{
    public DateTimeParameterInfo(Contract Contract, DateTimeParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public DateTime DateTimeValue
    {
        get => this.Value is DateTime dt ? dt : DateTime.Now;
        set { this.Value = value; }
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is DateTimeParameter dtp && value is DateTime dt)
            dtp.Value = dt;
    }
}
