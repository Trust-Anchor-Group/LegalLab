namespace LegalLabMaui.Models.Legal;

public class ContractReferenceModel : Model
{
    private readonly Property<string> contractName;
    private readonly Property<string> contractId;

    public ContractReferenceModel(string ContractName, string ContractId)
    {
        this.contractName = new Property<string>(nameof(ContractName), ContractName, this);
        this.contractId = new Property<string>(nameof(ContractId), ContractId, this);
    }

    public string ContractName { get => this.contractName.Value; set => this.contractName.Value = value; }
    public string ContractId { get => this.contractId.Value; set => this.contractId.Value = value; }

    public override string ToString() => this.ContractName + ": " + this.ContractId;
}
