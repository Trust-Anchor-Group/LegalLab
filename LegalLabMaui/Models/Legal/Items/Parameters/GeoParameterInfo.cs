using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Geo;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class GeoParameterInfo : ParameterInfo
{
    public GeoParameterInfo(Contract Contract, GeoParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public string GpsString
    {
        get => this.Value is GeoPosition gp ? $"{gp.Latitude},{gp.Longitude}" : string.Empty;
        set
        {
            string[] parts = value.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0].Trim(), out double lat) &&
                double.TryParse(parts[1].Trim(), out double lon))
                this.Value = new GeoPosition(lat, lon);
        }
    }

    public override string EditableValue
    {
        get => this.GpsString;
        set => this.SetValue(value);
    }

    public override void SetValue(string Value)
    {
        string before = this.GpsString;
        this.GpsString = Value;

        if (this.GpsString == before && !string.Equals(Value, before, StringComparison.Ordinal))
            this.SetInputError("Enter latitude and longitude separated by a comma.");
        else
            this.ClearInputError();
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is GeoParameter gp && value is GeoPosition pos)
            gp.Value = pos;
    }
}
