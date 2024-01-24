using LegalLab.Extensions;
using LegalLab.Items;
using LegalLab.Models.Design;
using LegalLab.Models.Items;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about a role
	/// </summary>
	public class RoleInfo : OrderedItem, INamedItem, ITranslatable
	{
		private readonly ContractModel contractModel;
		private readonly DesignModel designModel;
		private readonly Contract contract;
		private readonly Property<string> name;
		private readonly Property<object> description;
		private readonly Property<string> descriptionAsMarkdown;
		private readonly Property<int> minCount;
		private readonly Property<int> maxCount;
		private readonly Property<bool> canRevoke;

		private readonly Command signAsRole;
		private readonly Command proposeForRole;
		private readonly Command removeRole;

		private readonly Role role;

		/// <summary>
		/// Contains information about a role
		/// </summary>
		/// <param name="ContractModel">Contract Model hosting the parameter</param>
		/// <param name="Role">Role</param>
		/// <param name="Roles">Collection of roles.</param>
		public RoleInfo(ContractModel ContractModel, Role Role, Property<RoleInfo[]> Roles)
			: this(ContractModel, null, ContractModel.Contract, Role, Roles)
		{
		}

		/// <summary>
		/// Contains information about a role
		/// </summary>
		/// <param name="DesignModel">Design Model hosting the parameter</param>
		/// <param name="Role">Role</param>
		/// <param name="Roles">Collection of roles.</param>
		public RoleInfo(DesignModel DesignModel, Role Role, Property<RoleInfo[]> Roles)
			: this(null, DesignModel, DesignModel.Contract, Role, Roles)
		{
		}

		/// <summary>
		/// Contains information about a role
		/// </summary>
		/// <param name="ContractModel">Contract Model hosting the parameter</param>
		/// <param name="DesignModel">Design Model hosting the parameter</param>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Role">Role</param>
		/// <param name="Roles">Collection of roles.</param>
		private RoleInfo(ContractModel ContractModel, DesignModel DesignModel, Contract Contract, Role Role, Property<RoleInfo[]> Roles)
			: base(Roles)
		{
			this.contractModel = ContractModel;
			this.designModel = DesignModel;
			this.contract = Contract;
			this.role = Role;

			string Language = this.designModel?.Language ?? this.contractModel?.Language ?? Contract.DefaultLanguage;

			this.name = new Property<string>(nameof(this.Name), Role.Name, this);
			this.description = new Property<object>(nameof(this.Description), Role.ToSimpleXAML(Language, this.contract).Result, this);
			this.descriptionAsMarkdown = new Property<string>(nameof(this.DescriptionAsMarkdown), Role.ToMarkdown(Language, this.contract, MarkdownType.ForEditing).Trim(), this);
			this.minCount = new Property<int>(nameof(this.MaxCount), Role.MinCount, this);
			this.maxCount = new Property<int>(nameof(this.MinCount), Role.MaxCount, this);
			this.canRevoke = new Property<bool>(nameof(this.CanRevoke), Role.CanRevoke, this);

			this.signAsRole = new Command(this.CanExecuteSignAsRole, this.ExecuteSignAsRole);
			this.proposeForRole = new Command(this.CanExecuteProposeForRole, this.ExecuteProposeForRole);
			this.removeRole = new Command(this.CanExecuteRemoveRole, this.ExecuteRemoveRole);
		}

		/// <summary>
		/// Role reference object.
		/// </summary>
		public Role Role => this.role;

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get => this.name.Value;
			set
			{
				this.Role.Name = value;
				this.name.Value = value;
			}
		}

		/// <summary>
		/// Description (formatted) of the role.
		/// </summary>
		public object Description
		{
			get => this.description.Value;
		}

		/// <summary>
		/// Description, as Markdown
		/// </summary>
		public string DescriptionAsMarkdown
		{
			get => this.descriptionAsMarkdown.Value;
			set
			{
				string Language = this.designModel?.Language ?? this.contractModel?.Language ?? this.contract.DefaultLanguage;
				HumanReadableText Text = value.ToHumanReadableText(Language).Result;

				if (Text is null)
					this.role.Descriptions = this.role.Descriptions.Remove(Language);
				else
					this.role.Descriptions = this.role.Descriptions.Append(Text);

				this.descriptionAsMarkdown.Value = value;
				this.description.Value = value.ToSimpleXAML(this.contract, Language).Result;
			}
		}

		/// <summary>
		/// Minimum number of signatures for this role required.
		/// </summary>
		public int MinCount
		{
			get => this.minCount.Value;
			set
			{
				this.role.MinCount = value;
				this.minCount.Value = value;
			}
		}

		/// <summary>
		/// Maximum number of signatures for this role.
		/// </summary>
		public int MaxCount
		{
			get => this.maxCount.Value;
			set
			{
				this.role.MaxCount = value;
				this.maxCount.Value = value;
			}
		}

		/// <summary>
		/// If the role can revoke the contract.
		/// </summary>
		public bool CanRevoke
		{
			get => this.canRevoke.Value;
			set
			{
				this.role.CanRevoke = value;
				this.canRevoke.Value = value;
			}
		}

		/// <inheritdoc/>
		public override void RaisePropertyChanged(string PropertyName)
		{
			base.RaisePropertyChanged(PropertyName);

			this.designModel?.RaisePropertyChanged(nameof(this.designModel.Roles));
		}

		internal void CanBeSignedChanged()
		{
			this.signAsRole.RaiseCanExecuteChanged();
			this.proposeForRole.RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Sign as role command
		/// </summary>
		public ICommand SignAsRole => this.signAsRole;

		/// <summary>
		/// If the sign as role command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteSignAsRole()
		{
			return this.contractModel?.CanBeSigned ?? false;
		}

		/// <summary>
		/// Proposes the contract.
		/// </summary>
		public async Task ExecuteSignAsRole()
		{
			try
			{
				if (this.contractModel is not null)
					await this.contractModel.SignAsRole(this.Name);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Propose for role command
		/// </summary>
		public ICommand ProposeForRole => this.proposeForRole;

		/// <summary>
		/// If the propose for role command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteProposeForRole()
		{
			return this.contractModel?.CanBeSigned ?? false;
		}

		/// <summary>
		/// Proposes the contract for a given role.
		/// </summary>
		public async Task ExecuteProposeForRole()
		{
			if (this.contractModel is not null)
				await this.contractModel.ProposeForRole(this.Name);
		}

		/// <summary>
		/// Remove role command
		/// </summary>
		public ICommand RemoveRole => this.removeRole;

		/// <summary>
		/// If the remove role command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteRemoveRole()
		{
			return this.designModel is not null;
		}

		/// <summary>
		/// Removes the role.
		/// </summary>
		public Task ExecuteRemoveRole()
		{
			this.designModel?.RemoveRole(this);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets associated texts to translate.
		/// </summary>
		/// <param name="Language">Language to translate from.</param>
		/// <returns>Array of translatable texts, or null if none.</returns>
		public string[] GetTranslatableTexts(string Language)
		{
			HumanReadableText Text = this.Role.Descriptions.Find(Language);
			if (Text is null)
				return null;
			else
				return new string[] { Text.GenerateMarkdown(this.contract, MarkdownType.ForEditing) };
		}

		/// <summary>
		/// Sets translated texts.
		/// </summary>
		/// <param name="Texts">Available translated texts.</param>
		/// <param name="Language">Language translated to.</param>
		public void SetTranslatableTexts(string[] Texts, string Language)
		{
			if (Texts.Length > 0)
				this.DescriptionAsMarkdown = Texts[0].Trim();
		}

	}
}
