using LegalLabMaui.Models.Design;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

public class ContractReferenceParameterInfo : ParameterInfo
{
    private readonly Property<string> contractId;
    private readonly Property<string> localName;
    private readonly Property<string> @namespace;
    private readonly Property<string> templateId;
    private readonly Property<string> provider;
    private readonly Property<string> creatorRole;
    private readonly Property<bool> required;

    public ContractReferenceParameterInfo(Contract Contract, ContractReferenceParameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters)
    {
        this.contractId = new Property<string>(nameof(this.ContractId), Parameter.Value?.Value ?? string.Empty, this);
        this.localName = new Property<string>(nameof(this.LocalName), Parameter.LocalName, this);
        this.@namespace = new Property<string>(nameof(this.Namespace), Parameter.Namespace, this);
        this.templateId = new Property<string>(nameof(this.TemplateId), Parameter.TemplateId, this);
        this.provider = new Property<string>(nameof(this.Provider), Parameter.Provider, this);
        this.creatorRole = new Property<string>(nameof(this.CreatorRole), Parameter.CreatorRole, this);
        this.required = new Property<bool>(nameof(this.Required), Parameter.Required, this);
    }

    public string ContractId
    {
        get => this.contractId.Value;
        set
        {
            if (string.IsNullOrEmpty(value))
                this.contractId.Value = string.Empty;
            else if (!XmppClient.BareJidRegEx.IsMatch(value))
                throw new Exception("Invalid contract reference.");
            else
            {
                this.Parameter.SetValue(value);
                this.contractId.Value = value;
            }
        }
    }

    public string LocalName
    {
        get => this.localName.Value;
        set => this.localName.Value = value;
    }

    public string Namespace
    {
        get => this.@namespace.Value;
        set => this.@namespace.Value = value;
    }

    public string TemplateId
    {
        get => this.templateId.Value;
        set => this.templateId.Value = value;
    }

    public string Provider
    {
        get => this.provider.Value;
        set => this.provider.Value = value;
    }

    public string CreatorRole
    {
        get => this.creatorRole.Value;
        set => this.creatorRole.Value = value;
    }

    public bool Required
    {
        get => this.required.Value;
        set => this.required.Value = value;
    }

    public override void SetValue(string Value)
    {
        try
        {
            this.ClearInputError();
            this.ContractId = Value;
        }
        catch (Exception ex)
        {
            this.SetInputError(ex.Message);
        }
    }

    public override string EditableValue
    {
        get => this.ContractId;
        set => this.SetValue(value);
    }

    public override Task ExecuteRemoveParameter()
    {
        this.designModel?.RemoveContractReferenceParameter(this);
        return Task.CompletedTask;
    }

    protected override void SetParameterValue(object? value)
    {
        if (value is string s)
            this.Parameter.SetValue(s);
    }
}
