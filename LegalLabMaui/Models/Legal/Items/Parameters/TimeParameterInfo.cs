using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class TimeParameterInfo : ParameterInfo
{
    public TimeParameterInfo(Contract Contract, TimeParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public TimeSpan TimeValue
    {
        get => this.Value is TimeSpan ts ? ts : TimeSpan.Zero;
        set { this.Value = value; }
    }

    public string TimeString
    {
        get => this.TimeValue.ToString(@"hh\:mm\:ss");
        set { if (TimeSpan.TryParse(value, out TimeSpan ts)) this.TimeValue = ts; }
    }

    public override string EditableValue
    {
        get => this.TimeString;
        set => this.SetValue(value);
    }

    public override void SetValue(string Value)
    {
        string before = this.TimeString;
        this.TimeString = Value;

        if (this.TimeString == before && !string.Equals(Value, before, StringComparison.Ordinal))
            this.SetInputError("Enter a valid time, for example 14:30:00.");
        else
            this.ClearInputError();
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is TimeParameter tp && value is TimeSpan ts)
            tp.Value = ts;
    }
}
