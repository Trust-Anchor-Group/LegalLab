using LegalLab.Extensions;
using System;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Template reference model
	/// </summary>
	public class TemplateReferenceModel : Model
	{
		private readonly Property<string> templateName;
		private readonly Property<string> contractId;

		/// <summary>
		/// Template reference model
		/// </summary>
		/// <param name="TemplateName">Template name</param>
		/// <param name="ContractId">Contract ID</param>
		public TemplateReferenceModel(string TemplateName, string ContractId)
		{
			this.templateName = new Property<string>(nameof(TemplateName), TemplateName, this);
			this.contractId = new Property<string>(nameof(ContractId), ContractId, this);
		}

		/// <summary>
		/// Template Name
		/// </summary>
		public string TemplateName
		{
			get => this.templateName.Value;
			set => this.templateName.Value = value;
		}

		/// <summary>
		/// Contract ID
		/// </summary>
		public string ContractId
		{
			get => this.contractId.Value;
			set => this.contractId.Value = value;
		}
	}
}
