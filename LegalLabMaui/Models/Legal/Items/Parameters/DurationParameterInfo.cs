using LegalLabMaui.Models.Design;
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

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is DurationParameter dp)
            dp.Value = value is Waher.Content.Duration dur ? dur : null;
    }
}
