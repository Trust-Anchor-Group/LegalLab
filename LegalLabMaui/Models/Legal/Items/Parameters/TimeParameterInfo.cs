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

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is TimeParameter tp && value is TimeSpan ts)
            tp.Value = ts;
    }
}
