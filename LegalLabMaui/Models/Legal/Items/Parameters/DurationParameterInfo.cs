using LegalLabMaui.Models.Design;
using Waher.Content;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class DurationParameterInfo : ParameterInfo
{
    public DurationParameterInfo(Contract Contract, DurationParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public string DurationString
    {
        get => this.Value?.ToString() ?? string.Empty;
        set { this.Value = string.IsNullOrEmpty(value) ? null : (object)value; }
    }

    public override string EditableValue
    {
        get => this.DurationString;
        set => this.SetValue(value);
    }

    public override void SetValue(string Value)
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            this.ClearInputError();
            this.Value = null;
        }
        else if (Duration.TryParse(Value, out Duration parsed))
        {
            this.ClearInputError();
            this.Value = parsed;
        }
        else
            this.SetInputError("Enter a valid duration, for example P7D or PT12H.");
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is DurationParameter dp)
            dp.Value = value is Waher.Content.Duration dur ? dur : null;
    }
}
