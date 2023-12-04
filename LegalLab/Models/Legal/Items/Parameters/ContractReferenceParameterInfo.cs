using LegalLab.Extensions;
using LegalLab.Models.Design;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a contract reference parameter
	/// </summary>
	public class ContractReferenceParameterInfo : ParameterInfo
	{
		private readonly Property<object> label;
		private readonly Property<string> labelAsMarkdown;
		private readonly Property<string> contractId;
		private readonly Property<string> localName;
		private readonly Property<string> @namespace;
		private readonly Property<string> templateId;
		private readonly Property<string> provider;
		private readonly Property<string> creatorRole;
		private readonly Property<bool> required;

		/// <summary>
		/// Contains information about a contract reference parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public ContractReferenceParameterInfo(Contract Contract, ContractReferenceParameter Parameter, Control Control, DesignModel DesignModel, IProperty Parameters)
			: base(Contract, Parameter, Control, DesignModel, Parameters)
		{
			string Language = DesignModel?.Language ?? Contract.DefaultLanguage;

			this.label = new Property<object>(nameof(this.Label), Parameter.ToSimpleXAML(Language, Contract).Result, this);
			this.labelAsMarkdown = new Property<string>(nameof(this.LabelAsMarkdown), Parameter.ToMarkdown(Language, Contract, MarkdownType.ForEditing).Trim(), this);
			this.contractId = new Property<string>(nameof(this.Value), Parameter.Value?.Value, this);
			this.localName = new Property<string>(nameof(this.LocalName), Parameter.LocalName, this);
			this.@namespace = new Property<string>(nameof(this.Namespace), Parameter.Namespace, this);
			this.templateId = new Property<string>(nameof(this.TemplateId), Parameter.TemplateId, this);
			this.provider = new Property<string>(nameof(this.Provider), Parameter.Provider, this);
			this.creatorRole = new Property<string>(nameof(this.CreatorRole), Parameter.CreatorRole, this);
			this.required = new Property<bool>(nameof(this.Required), Parameter.Required, this);
		}

		/// <summary>
		/// Label (formatted) of the parameter.
		/// </summary>
		public object Label
		{
			get => this.label.Value;
			set => this.label.Value = value;
		}

		/// <summary>
		/// Label, as Markdown
		/// </summary>
		public string LabelAsMarkdown
		{
			get => this.labelAsMarkdown.Value;
			set
			{
				string Language = this.designModel?.Language ?? this.Contract.DefaultLanguage;
				Waher.Networking.XMPP.Contracts.HumanReadable.Label Label = value.ToHumanReadableLabel(Language).Result;
				ContractReferenceParameter ContractReferenceParameter = (ContractReferenceParameter)this.Parameter;

				if (Label is null)
					ContractReferenceParameter.Labels = ContractReferenceParameter.Labels.Remove(Language);
				else
					ContractReferenceParameter.Labels = ContractReferenceParameter.Labels.Append(Label);

				this.labelAsMarkdown.Value = value;
				this.label.Value = value.ToSimpleXAML(this.Contract, Language).Result;
			}
		}

		/// <summary>
		/// ID of referenced contract.
		/// </summary>
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

		/// <summary>
		/// Local name of root of machine-readable section.
		/// </summary>
		public string LocalName
		{
			get => this.localName.Value;
			set => this.localName.Value = value;
		}

		/// <summary>
		/// Namespace of root of machine-readable section.
		/// </summary>
		public string Namespace
		{
			get => this.@namespace.Value;
			set => this.@namespace.Value = value;
		}

		/// <summary>
		/// Contract ID of template used to create contract.
		/// </summary>
		public string TemplateId
		{
			get => this.templateId.Value;
			set => this.templateId.Value = value;
		}

		/// <summary>
		/// Provider of contract.
		/// </summary>
		public string Provider
		{
			get => this.provider.Value;
			set => this.provider.Value = value;
		}

		/// <summary>
		/// Role of creator of current contract, in referenced contract.
		/// </summary>
		public string CreatorRole
		{
			get => this.creatorRole.Value;
			set => this.creatorRole.Value = value;
		}

		/// <summary>
		/// If property is required.
		/// </summary>
		public bool Required
		{
			get => this.required.Value;
			set => this.required.Value = value;
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			this.ContractId = Value;
		}

		/// <summary>
		/// Removes the parameter.
		/// </summary>
		public override Task ExecuteRemoveParameter()
		{
			this.designModel?.RemoveContractReferenceParameter(this);
			return Task.CompletedTask;
		}
	}
}
