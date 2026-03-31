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

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is GeoParameter gp && value is GeoPosition pos)
            gp.Value = pos;
    }
}
