using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class BooleanParameterInfo : ParameterInfo
{
    public BooleanParameterInfo(Contract Contract, BooleanParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters) { }

    public bool BoolValue
    {
        get => this.Value is bool b && b;
        set { this.Value = value; }
    }

    public override string EditableValue
    {
        get => this.BoolValue ? "True" : "False";
        set => this.SetValue(value);
    }

    public override void SetValue(string Value)
    {
        if (bool.TryParse(Value, out bool parsed))
        {
            this.ClearInputError();
            this.BoolValue = parsed;
        }
        else if (string.Equals(Value, "1", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(Value, "yes", StringComparison.OrdinalIgnoreCase))
        {
            this.ClearInputError();
            this.BoolValue = true;
        }
        else if (string.Equals(Value, "0", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(Value, "no", StringComparison.OrdinalIgnoreCase))
        {
            this.ClearInputError();
            this.BoolValue = false;
        }
        else
            this.SetInputError("Enter True/False, Yes/No, or 1/0.");
    }

    protected override void SetParameterValue(object? value)
    {
        if (this.Parameter is BooleanParameter bp)
            bp.Value = value is bool b && b;
    }
}
