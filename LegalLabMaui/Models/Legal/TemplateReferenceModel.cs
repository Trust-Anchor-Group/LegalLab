namespace LegalLabMaui.Models.Legal;

public class TemplateReferenceModel : Model
{
    private readonly Property<string> templateName;
    private readonly Property<string> contractId;

    public TemplateReferenceModel(string TemplateName, string ContractId)
    {
        this.templateName = new Property<string>(nameof(TemplateName), TemplateName, this);
        this.contractId = new Property<string>(nameof(ContractId), ContractId, this);
    }

    public string TemplateName { get => this.templateName.Value; set => this.templateName.Value = value; }
    public string ContractId { get => this.contractId.Value; set => this.contractId.Value = value; }

    public override string ToString() => this.TemplateName + ": " + this.ContractId;
}
