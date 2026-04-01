using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class RoleReferenceParameterInfo : ParameterInfo
{
    private readonly Property<string> role;
    private readonly Property<string> property;
    private readonly Property<int> index;
    private readonly Property<bool> required;

    public RoleReferenceParameterInfo(Contract Contract, RoleParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters)
    {
        this.role = new Property<string>(nameof(this.Role), Parameter.Role, this);
        this.property = new Property<string>(nameof(this.Property), Parameter.Property, this);
        this.index = new Property<int>(nameof(this.Index), Parameter.Index, this);
        this.required = new Property<bool>(nameof(this.Required), Parameter.Required, this);

        if (this.designModel is not null)
            this.designModel.PropertyChanged += this.DesignModel_PropertyChanged;
    }

    public string Role
    {
        get => this.role.Value;
        set => this.role.Value = value;
    }

    public int Index
    {
        get => this.index.Value;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(this.Index), "Role index must be positive.");
            this.index.Value = value;
        }
    }

    public string Property
    {
        get => this.property.Value;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentOutOfRangeException(nameof(this.Property), "The property reference cannot be empty.");
            this.property.Value = value;
        }
    }

    public bool Required
    {
        get => this.required.Value;
        set => this.required.Value = value;
    }

    public string[] Roles => this.designModel?.RoleNames ?? [];

    private void DesignModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.Roles))
            this.RaisePropertyChanged(nameof(this.Roles));
    }

    public override bool CanEditValue => false;

    public override void SetValue(string Value)
    {
        throw new Exception("Role reference parameter cannot be set.");
    }

    public override Task ExecuteRemoveParameter()
    {
        this.designModel?.RemoveRoleReferenceParameter(this);
        return Task.CompletedTask;
    }

    protected override void SetParameterValue(object? value) { }
}
