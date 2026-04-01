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

    public override string EditableValue
    {
        get => this.DateTimeValue.ToString("yyyy-MM-dd HH:mm:ss");
        set => this.SetValue(value);
    }

    public override void SetValue(string Value)
    {
        if (DateTime.TryParse(Value, out DateTime parsed))
        {
            this.ClearInputError();
            this.DateTimeValue = parsed;
        }
        else
            this.SetInputError("Enter a valid date and time.");
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is DateTimeParameter dtp && value is DateTime dt)
            dtp.Value = dt;
    }
}
