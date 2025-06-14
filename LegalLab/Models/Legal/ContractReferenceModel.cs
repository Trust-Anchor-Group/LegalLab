namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contract reference model
	/// </summary>
	public class ContractReferenceModel : Model
	{
		private readonly Property<string> contractName;
		private readonly Property<string> contractId;

		/// <summary>
		/// Contract reference model
		/// </summary>
		/// <param name="ContractName">Contract name</param>
		/// <param name="ContractId">Contract ID</param>
		public ContractReferenceModel(string ContractName, string ContractId)
		{
			this.contractName = new Property<string>(nameof(ContractName), ContractName, this);
			this.contractId = new Property<string>(nameof(ContractId), ContractId, this);
		}

		/// <summary>
		/// Contract Name
		/// </summary>
		public string ContractName
		{
			get => this.contractName.Value;
			set => this.contractName.Value = value;
		}

		/// <summary>
		/// Contract ID
		/// </summary>
		public string ContractId
		{
			get => this.contractId.Value;
			set => this.contractId.Value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.ContractName + ": " + this.ContractId;
		}
	}
}
