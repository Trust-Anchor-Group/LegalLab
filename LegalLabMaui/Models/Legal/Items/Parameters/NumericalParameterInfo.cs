using System.Globalization;
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

    public override void SetValue(string Value)
    {
        if (decimal.TryParse(Value, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal parsed) ||
            decimal.TryParse(Value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
        {
            this.ClearInputError();
            this.DecimalValue = parsed;
        }
        else
            this.SetInputError("Enter a valid number.");
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is NumericalParameter np && value is decimal d)
            np.Value = d;
    }
}
