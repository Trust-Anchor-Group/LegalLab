using LegalLabMaui.Converters;
using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Legal.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Persistence;
using Waher.Runtime.Geo;
using Waher.Runtime.IO;

namespace LegalLabMaui.Models.Legal
{
	/// <summary>
	/// Contract model
	/// </summary>
	public class ContractModel : ContractParametersModel, IPartsModel
	{
		private readonly Property<GenInfo[]> generalInformation;
		private readonly Property<RoleInfo[]> roles;
		private readonly Property<PartInfo[]> parts;
		private readonly Property<AttachmentInfo[]> attachments;
		private readonly Property<ClientSignatureInfo[]> clientSignatures;
		private readonly Property<ServerSignatureInfo[]> serverSignatures;
		private readonly Property<bool> hasId;
		private readonly Property<bool> canBeSigned;
		private readonly Property<bool> canUploadAttachment;
		private readonly Property<bool> canDownloadAttachment;
		private readonly Property<string> uri;
		private readonly Property<string> qrCodeUri;
		private readonly Property<string> machineReadable;
		private readonly Property<string> templateName;
		private readonly Property<string> contractId;
		private readonly Property<string> humanReadableText;
		private readonly Property<string> createContractFeedback;

		private readonly Command addPart;
		private readonly Command createContract;
		private readonly Command explainCreateContract;
		private readonly Command removeTemplate;
		private readonly Command removeContract;
		private readonly Command uploadAttachment;

		private readonly ContractsClient contracts;
		private readonly LegalModel legalModel;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="DesignModel">Design Model</param>
		private ContractModel(ContractsClient Contracts, Contract Contract, DesignModel DesignModel)
			: base(Contract.Parameters, Contract, "en", DesignModel)
		{
			this.generalInformation = new Property<GenInfo[]>(nameof(this.GeneralInformation), [], this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), [], this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), [], this);
			this.attachments = new Property<AttachmentInfo[]>(nameof(this.Attachments), [], this);
			this.clientSignatures = new Property<ClientSignatureInfo[]>(nameof(this.ClientSignatures), [], this);
			this.serverSignatures = new Property<ServerSignatureInfo[]>(nameof(this.ServerSignatures), [], this);
			this.hasId = new Property<bool>(nameof(this.HasId), false, this);
			this.canBeSigned = new Property<bool>(nameof(this.CanBeSigned), false, this);
			this.canUploadAttachment = new Property<bool>(nameof(this.CanUploadAttachment), false, this);
			this.canDownloadAttachment = new Property<bool>(nameof(this.CanDownloadAttachment), false, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);
			this.qrCodeUri = new Property<string>(nameof(this.QrCodeUri), string.Empty, this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.templateName = new Property<string>(nameof(this.TemplateName), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), Contract.ContractId, this);
			this.humanReadableText = new Property<string>(nameof(this.HumanReadableText), string.Empty, this);
			this.createContractFeedback = new Property<string>(nameof(this.CreateContractFeedback), string.Empty, this);

			this.ParameterOptions = [];

			this.addPart = new Command(this.ExecuteAddPart);
			this.createContract = new Command(this.CanExecuteCreateContract, this.ExecuteCreateContract);
			this.explainCreateContract = new Command(this.ExecuteExplainCreateContract);
			this.removeTemplate = new Command(this.CanExecuteRemoveTemplate, this.ExecuteRemoveTemplate);
			this.removeContract = new Command(this.CanExecuteRemoveContract, this.ExecuteRemoveContract);
			this.uploadAttachment = new Command(this.CanExecuteUploadAttachment, this.ExecuteUploadAttachment);

			this.legalModel = DesignModel.Network?.Legal ?? AppService.NetworkModel.Legal;
			this.contracts = Contracts;

			this.TemplateName = Contract.ContractId;
			this.UpdateCreateContractFeedback();
		}

		/// <summary>
		/// Reference to the current contracts client.
		/// </summary>
		public ContractsClient ContractsClient => this.contracts;

		/// <summary>
		/// Creates the contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="DesignModel">Design Model</param>
		/// <returns>Contract object model.</returns>
		public static async Task<ContractModel> CreateAsync(ContractsClient Contracts, Contract Contract, DesignModel DesignModel)
		{
			ContractModel Result = new(Contracts, Contract, DesignModel);
			(string s, _) = Contract.ForMachines.ToPrettyXml();

			await Result.SetContract(Contract);
			Result.MachineReadable = s;

			return Result;
		}

		/// <summary>
		/// Sets the current contract.
		/// </summary>
		/// <param name="Contract">Contract object.</param>
		public override async Task SetContract(Contract Contract)
		{
			await base.SetContract(Contract);

			string Domain = this.contracts.Client.Domain;
			if (string.IsNullOrEmpty(Domain))
				Domain = "lab.tagroot.io";

			this.ContractId = Contract.ContractId;
			this.HasId = !string.IsNullOrEmpty(Contract.ContractId);
			this.CanBeSigned = Contract.State == ContractState.Approved || Contract.State == ContractState.BeingSigned;

			this.CanUploadAttachment = Contract.State == ContractState.Approved &&
				this.legalModel.FileUpload is not null &&
				this.legalModel.FileUpload.MaxFileSize.HasValue;

			this.CanDownloadAttachment = (Contract.Attachments?.Length ?? 0) > 0;

			this.Uri = ContractsClient.ContractIdUriString(Contract.ContractId);
			this.QrCodeUri = "https://" + Domain + "/QR/" + this.Uri + "?w=400&h=400&q=2";

			List<GenInfo> GenInfo =
			[
				new GenInfo("Created:", this.Contract.Created.ToString(CultureInfo.CurrentUICulture))
			];

			if (this.Contract.Updated > DateTime.MinValue)
				GenInfo.Add(new GenInfo("Updated:", this.Contract.Updated.ToString(CultureInfo.CurrentUICulture)));

			GenInfo.Add(new GenInfo("State:", this.Contract.State.ToString()));
			GenInfo.Add(new GenInfo("Visibility:", this.Contract.Visibility.ToString()));
			GenInfo.Add(new GenInfo("Duration:", this.Contract.Duration.ToString()));
			GenInfo.Add(new GenInfo("From:", this.Contract.From.ToString(CultureInfo.CurrentUICulture)));
			GenInfo.Add(new GenInfo("To:", this.Contract.To.ToString(CultureInfo.CurrentUICulture)));
			GenInfo.Add(new GenInfo("Archiving Optional:", this.Contract.ArchiveOptional.ToString()));
			GenInfo.Add(new GenInfo("Archiving Required:", this.Contract.ArchiveRequired.ToString()));
			GenInfo.Add(new GenInfo("Can act as a template:", this.Contract.CanActAsTemplate.ToYesNo()));
			GenInfo.Add(new GenInfo("Provider:", this.Contract.Provider));
			GenInfo.Add(new GenInfo("Parts:", this.Contract.PartsMode.ToString()));

			if (this.Contract.SignAfter.HasValue)
				GenInfo.Add(new GenInfo("Sign after:", this.Contract.SignAfter.Value.ToStringTZ()));

			if (this.Contract.SignBefore.HasValue)
				GenInfo.Add(new GenInfo("Sign before:", this.Contract.SignBefore.Value.ToStringTZ()));

			if (!string.IsNullOrEmpty(this.Contract.TemplateId))
				GenInfo.Add(new GenInfo("Template ID:", this.Contract.TemplateId));

			this.GeneralInformation = [.. GenInfo];

			List<PartInfo> Parts = [];

			if (this.Contract.Parts is not null)
			{
				foreach (Part Part in this.Contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = [.. Parts];

			List<RoleInfo> Roles = [];

			if (this.Contract.Roles is not null)
			{
				foreach (Role Role in this.Contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = [.. Roles];

			List<AttachmentInfo> Attachments = [];

			if (this.Contract.Attachments is not null)
			{
				foreach (Attachment Attachment in this.Contract.Attachments)
					Attachments.Add(new AttachmentInfo(
						Attachment,
						!string.IsNullOrEmpty(Attachment.Url),
						false,
						info =>
						{
							AppService.OpenUrl(new Uri(Attachment.Url));
							return Task.CompletedTask;
						},
						info => Task.CompletedTask));
			}

			this.Attachments = [.. Attachments];

			List<ClientSignatureInfo> ClientSignatures = [];

			if (this.Contract.ClientSignatures is not null)
			{
				foreach (ClientSignature ClientSignature in this.Contract.ClientSignatures)
					ClientSignatures.Add(new ClientSignatureInfo(ClientSignature));
			}

			this.ClientSignatures = [.. ClientSignatures];

			if (this.Contract.ServerSignature is null)
				this.ServerSignatures = [];
			else
				this.ServerSignatures = [new(this.Contract.ServerSignature)];

			await this.PopulateHumanReadableText();
			this.UpdateCreateContractFeedback();
		}

		/// <inheritdoc/>
		public override Task Start()
		{
			this.legalModel.PropertyChanged += this.LegalModel_PropertyChanged;
			this.contracts.ContractUpdated += this.Contracts_ContractUpdated;
			this.contracts.ContractSigned += this.Contracts_ContractSigned;

			return base.Start();
		}

		/// <inheritdoc/>
		public override Task Stop()
		{
			this.legalModel.PropertyChanged -= this.LegalModel_PropertyChanged;
			this.contracts.ContractUpdated -= this.Contracts_ContractUpdated;
			this.contracts.ContractSigned -= this.Contracts_ContractSigned;

			return base.Stop();
		}

		private void LegalModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(LegalModel.Templates):
					this.RaisePropertyChanged(nameof(this.Templates));
					this.RaisePropertyChanged(nameof(this.SelectedTemplate));
					break;

				case nameof(LegalModel.ContractTemplateName):
					this.RaisePropertyChanged(nameof(this.SelectedTemplate));
					this.UpdateCreateContractFeedback();
					break;

				case nameof(LegalModel.ExistingContracts):
					this.RaisePropertyChanged(nameof(this.ExistingContracts));
					this.RaisePropertyChanged(nameof(this.SelectedExistingContract));
					break;

				case nameof(LegalModel.ExistingContractId):
					this.RaisePropertyChanged(nameof(this.SelectedExistingContract));
					break;

				case nameof(LegalModel.AutoSignProposals):
					this.RaisePropertyChanged(nameof(this.AutoSignProposals));
					break;

				case nameof(LegalModel.IsTemplate):
					this.RaisePropertyChanged(nameof(this.IsTemplate));
					this.removeTemplate.RaiseCanExecuteChanged();
					this.UpdateCreateContractFeedback();
					break;

				case nameof(LegalModel.IsContract):
					this.RaisePropertyChanged(nameof(this.IsContract));
					this.removeContract.RaiseCanExecuteChanged();
					break;
			}
		}

		private async Task Contracts_ContractSigned(object Sender, ContractSignedEventArgs e)
		{
			Contract Contract = e.Contract ?? await this.contracts.GetContractAsync(this.ContractId);

			if (this.legalModel is not null)
				await this.legalModel.Contracts_ContractUpdated(Sender, e, Contract);

			await this.CheckUpdatedView(Contract);
		}

		private async Task Contracts_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			Contract Contract = await this.contracts.GetContractAsync(this.ContractId);

			if (this.legalModel is not null)
				await this.legalModel.Contracts_ContractUpdated(Sender, e, Contract);

			await this.CheckUpdatedView(Contract);
		}

		private async Task CheckUpdatedView(Contract Contract)
		{
			if (Contract.ContractId == this.ContractId)
			{
				await AppService.UpdateGui(async () =>
				{
					await this.SetContract(Contract);
				});
			}
		}

		protected override async Task<bool> SetLanguage(string Language)
		{
			if (!await base.SetLanguage(Language))
				return false;

			try
			{
				if (!string.IsNullOrEmpty(Language))
				{
					foreach (RoleInfo RI in this.Roles)
					{
						HumanReadableText Text = RI.Role.Descriptions.Find(Language, "en");

						if (Text is null)
							RI.DescriptionAsMarkdown = string.Empty;
						else
							RI.DescriptionAsMarkdown = (await Text.GenerateMarkdown(this.Contract, MarkdownType.ForEditing) ?? string.Empty).Trim();
					}

					await this.PopulateHumanReadableText();
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				AppService.ErrorBox(ex.Message);

				return false;
			}
		}

		protected override void ParametersOkChanged()
		{
			this.UpdateCreateContractFeedback();
		}

		public string CreateContractFeedback
		{
			get => this.createContractFeedback.Value;
			set => this.createContractFeedback.Value = value;
		}

		public bool CreateContractAvailable => this.CanExecuteCreateContract();

		public bool CreateContractBlocked => !this.CreateContractAvailable;

		/// <summary>
		/// If contract has an ID
		/// </summary>
		public bool HasId
		{
			get => this.hasId.Value;
			set => this.hasId.Value = value;
		}

		/// <summary>
		/// If contract can be signed
		/// </summary>
		public bool CanBeSigned
		{
			get => this.canBeSigned.Value;
			set
			{
				this.canBeSigned.Value = value;

				foreach (RoleInfo Role in this.Roles)
					Role.CanBeSignedChanged();
			}
		}

		/// <summary>
		/// If contract can receive new attachments.
		/// </summary>
		public bool CanUploadAttachment
		{
			get => this.canUploadAttachment.Value;
			set => this.canUploadAttachment.Value = value;
		}

		/// <summary>
		/// If the user can download attachments.
		/// </summary>
		public bool CanDownloadAttachment
		{
			get => this.canDownloadAttachment.Value;
			set => this.canDownloadAttachment.Value = value;
		}

		/// <summary>
		/// URI to contract
		/// </summary>
		public string Uri
		{
			get => this.uri.Value;
			set => this.uri.Value = value;
		}

		/// <summary>
		/// URI to QR-Code of contract
		/// </summary>
		public string QrCodeUri
		{
			get => this.qrCodeUri.Value;
			set => this.qrCodeUri.Value = value;
		}

		/// <summary>
		/// Machine-Readable XML in the contract
		/// </summary>
		public string MachineReadable
		{
			get => this.machineReadable.Value;
			set => this.machineReadable.Value = value;
		}

		/// <summary>
		/// Human-readable text (plain-text representation) of the contract.
		/// </summary>
		public string HumanReadableText
		{
			get => this.humanReadableText?.Value ?? string.Empty;
			set
			{
				if (this.humanReadableText is not null)
					this.humanReadableText.Value = value;
			}
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
		/// ID of contract
		/// </summary>
		public string ContractId
		{
			get => this.contractId.Value;
			set => this.contractId.Value = value;
		}

		/// <summary>
		/// If contract proposals should be auto-signed.
		/// </summary>
		public bool AutoSignProposals
		{
			get => this.legalModel.AutoSignProposals;
			set => this.legalModel.AutoSignProposals = value;
		}

		public bool IsTemplate => this.legalModel.IsTemplate;

		public bool IsContract => this.legalModel.IsContract;

		public TemplateReferenceModel[] Templates => this.legalModel.Templates;

		public TemplateReferenceModel? SelectedTemplate
		{
			get => Array.Find(this.legalModel.Templates, reference => reference.TemplateName == this.legalModel.ContractTemplateName);
			set => this.legalModel.ContractTemplateName = value?.TemplateName ?? string.Empty;
		}

		public ContractReferenceModel[] ExistingContracts => this.legalModel.ExistingContracts;

		public ContractReferenceModel? SelectedExistingContract
		{
			get => Array.Find(this.legalModel.ExistingContracts, reference => reference.ContractId == this.legalModel.ExistingContractId);
			set => this.legalModel.ExistingContractId = value?.ContractId ?? string.Empty;
		}

		/// <summary>
		/// The different parameter options available to choose from when creating a contract.
		/// </summary>
		public ObservableCollection<ContractOption> ParameterOptions { get; }

		protected override Task ParametersChanged()
		{
			return this.PopulateHumanReadableText();
		}

		private async Task PopulateHumanReadableText()
		{
			try
			{
				// Generate Markdown representation of the human-readable contract content
				string Markdown = await this.Contract.ToMarkdown(this.Language, MarkdownType.ForRendering);
				this.HumanReadableText = Markdown?.Trim() ?? string.Empty;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.HumanReadableText = string.Empty;
			}
		}

		/// <summary>
		/// General information about the contract.
		/// </summary>
		public GenInfo[] GeneralInformation
		{
			get => this.generalInformation.Value;
			set => this.generalInformation.Value = value;
		}

		/// <summary>
		/// Roles defined the contract.
		/// </summary>
		public RoleInfo[] Roles
		{
			get => this.roles?.Value ?? [];
			set => this.roles.Value = value;
		}

		/// <summary>
		/// Role names
		/// </summary>
		public string[] RoleNames
		{
			get
			{
				RoleInfo[] Roles = this.Roles;

				if (Roles is null)
					return [];

				int i, c = Roles.Length;
				string[] Result = new string[c];

				for (i = 0; i < c; i++)
					Result[i] = Roles[i].Name;

				return Result;
			}
		}

		/// <summary>
		/// Parts defined the contract.
		/// </summary>
		public PartInfo[] Parts
		{
			get => this.parts.Value;
			set
			{
				this.parts.Value = value;
				this.Contract.Parts = value.ToParts();
			}
		}

		/// <summary>
		/// Attachments defined the contract.
		/// </summary>
		public AttachmentInfo[] Attachments
		{
			get => this.attachments.Value;
			set => this.attachments.Value = value;
		}

		/// <summary>
		/// Client Signatures defined the contract.
		/// </summary>
		public ClientSignatureInfo[] ClientSignatures
		{
			get => this.clientSignatures.Value;
			set => this.clientSignatures.Value = value;
		}

		/// <summary>
		/// Server Signatures defined the contract.
		/// </summary>
		public ServerSignatureInfo[] ServerSignatures
		{
			get => this.serverSignatures.Value;
			set => this.serverSignatures.Value = value;
		}

		/// <summary>
		/// If a contract is loaded.
		/// </summary>
		public bool ContractLoaded => this.Contract is not null;

		/// <summary>
		/// Command for adding a part to the contract.
		/// </summary>
		public ICommand AddPart => this.addPart;

		/// <summary>
		/// Adds a part to the contract.
		/// </summary>
		public Task ExecuteAddPart()
		{
			PartInfo[] Parts = this.Parts;
			int c = Parts.Length;

			Array.Resize<PartInfo>(ref Parts, c + 1);
			Parts[c] = new PartInfo(new Part()
			{
				LegalId = string.Empty,
				Role = string.Empty
			}, this, this.parts);

			this.Parts = Parts;

			if (this.Contract.PartsMode != ContractParts.ExplicitlyDefined)
			{
				this.Contract.PartsMode = ContractParts.ExplicitlyDefined;

				foreach (GenInfo GI in this.GeneralInformation)
				{
					if (GI.Name == "Parts:")
						GI.Value = this.Contract.PartsMode.ToString();
				}

				this.RaisePropertyChanged(nameof(this.GeneralInformation));
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Removes a part from the design
		/// </summary>
		/// <param name="Part">Part to remove</param>
		public void RemovePart(PartInfo Part)
		{
			PartInfo[] Parts = this.Parts;
			int i = Array.IndexOf(Parts, Part);

			if (i < 0)
				return;

			int c = Parts.Length;

			if (i < c - 1)
				Array.Copy(Parts, i + 1, Parts, i, c - i - 1);

			Array.Resize<PartInfo>(ref Parts, c - 1);

			this.Parts = Parts;
		}

		/// <summary>
		/// Create contract command
		/// </summary>
		public ICommand CreateContract => this.createContract;

		/// <summary>
		/// Displays the current create-contract availability details.
		/// </summary>
		public ICommand ExplainCreateContract => this.explainCreateContract;

		/// <summary>
		/// If the create contract command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteCreateContract()
		{
			return this.ParametersOk && this.legalModel.Template is not null && this.legalModel.IsTemplate;
		}

		private void UpdateCreateContractFeedback()
		{
			this.createContract.RaiseCanExecuteChanged();
			this.CreateContractFeedback = this.GetCreateContractFeedback();
			this.RaisePropertyChanged(nameof(this.CreateContractAvailable));
			this.RaisePropertyChanged(nameof(this.CreateContractBlocked));
		}

		private string GetCreateContractFeedback()
		{
			if (this.legalModel.Template is null)
				return "Select a template to create a contract from it.";

			if (!this.legalModel.IsTemplate)
				return "Create Contract is only available while viewing a template, not an existing contract.";

			List<string> InvalidParameters = [];

			foreach (ParameterInfo Parameter in this.Parameters)
			{
				if (!string.IsNullOrEmpty(Parameter.ErrorText) || Parameter.ErrorReason.HasValue)
					InvalidParameters.Add(string.IsNullOrEmpty(Parameter.Name) ? "unnamed parameter" : Parameter.Name);
			}

			if (InvalidParameters.Count > 0)
				return "Fix invalid parameters before creating the contract: " + string.Join(", ", InvalidParameters) + ".";

			if (!this.ParametersOk)
				return "One or more parameters are incomplete or invalid.";

			return "Ready to create a contract from the selected template.";
		}

		private Task ExecuteExplainCreateContract()
		{
			string Title = this.CreateContractAvailable ? "Create Contract" : "Create Contract Unavailable";
			return AppService.MessageBox(this.CreateContractFeedback, Title);
		}

		/// <summary>
		/// Creates a contract.
		/// </summary>
		public async Task ExecuteCreateContract()
		{
			try
			{
				bool Confirmed = await AppService.MessageBox(
					"Are you sure you want to create the contract on " + this.contracts.ComponentAddress + "?",
					"Confirm", true);

				if (!Confirmed)
					return;

				AppService.MouseHourglass();

				string TemplateId = this.legalModel.Template.ContractId;

				Contract Contract = await this.contracts.CreateContractAsync(TemplateId, this.Contract.Parts, this.Contract.Parameters,
					this.Contract.Visibility, this.Parts.Length > 0 ? ContractParts.ExplicitlyDefined : ContractParts.Open,
					this.Contract.Duration, this.Contract.ArchiveRequired, this.Contract.ArchiveOptional, this.Contract.SignAfter,
					this.Contract.SignBefore, false);

				await this.SetContract(Contract);

				AppService.SuccessBox("Contract successfully created.");
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Remove template command
		/// </summary>
		public ICommand RemoveTemplate => this.removeTemplate;

		/// <summary>
		/// If the remove template command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteRemoveTemplate()
		{
			return this.legalModel.Template is not null && this.legalModel.IsTemplate;
		}

		/// <summary>
		/// Removed the template.
		/// </summary>
		public async Task ExecuteRemoveTemplate()
		{
			try
			{
				bool Confirmed = await AppService.MessageBox(
					"Are you sure you want to remove the selected template? (Only the reference to the template will be removed.)",
					"Confirm", true);

				if (!Confirmed)
					return;

				AppService.MouseHourglass();

				string TemplateId = this.legalModel.Template.ContractId;

				await this.legalModel.RemoveContract(TemplateId);

				AppService.MouseDefault();
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Remove contract command
		/// </summary>
		public ICommand RemoveContract => this.removeContract;

		/// <summary>
		/// If the remove contract command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteRemoveContract()
		{
			return this.legalModel.Template is not null &&
				this.legalModel.IsContract &&
				!string.IsNullOrEmpty(this.legalModel.ExistingContractId);
		}

		/// <summary>
		/// Removed the contract.
		/// </summary>
		public async Task ExecuteRemoveContract()
		{
			try
			{
				bool Confirmed = await AppService.MessageBox(
					"Are you sure you want to remove the selected contract? (Only the reference to the contract will be removed.)",
					"Confirm", true);

				if (!Confirmed)
					return;

				AppService.MouseHourglass();

				string ContractId = this.legalModel.ExistingContractId;

				await this.legalModel.RemoveContract(ContractId);

				AppService.MouseDefault();
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Signs the contract, as the given role
		/// </summary>
		/// <param name="Role">Role to sign the contract as.</param>
		public Task SignAsRole(string Role)
		{
			return this.SignAsRole(Role, true);
		}

		/// <summary>
		/// Signs the contract, as the given role
		/// </summary>
		/// <param name="Role">Role to sign the contract as.</param>
		/// <param name="ConfirmWithUser">If a confirmation dialog with the user should be shown.</param>
		public async Task SignAsRole(string Role, bool ConfirmWithUser)
		{
			try
			{
				if (ConfirmWithUser)
				{
					bool Confirmed = await AppService.MessageBox(
						"Are you sure you want to sign the contract as " + Role + "?",
						"Confirm", true);

					if (!Confirmed)
						return;
				}

				AppService.MouseHourglass();

				Contract Contract = await this.contracts.SignContractAsync(this.Contract, Role, false);

				await this.SetContract(Contract);

				if (ConfirmWithUser)
					AppService.SuccessBox("Contract successfully signed.");
			}
			catch (Exception ex)
			{
				if (ConfirmWithUser)
					AppService.ErrorBox(ex.Message);
				else
					Log.Exception(ex);
			}
		}

		/// <summary>
		/// Recommends the contract to someone else.
		/// </summary>
		/// <param name="Role">Role to propose the contract as.</param>
		public async Task ProposeForRole(string Role)
		{
			try
			{
				string BareJid = string.Empty;

				while (true)
				{
					BareJid = await AppService.PromptUser("Recommend contract",
						"Recommend this contract to be signed by (Bare JID):", BareJid) ?? string.Empty;

					if (string.IsNullOrEmpty(BareJid))
						return;

					if (XmppClient.BareJidRegEx.IsMatch(BareJid))
						break;
					else
						AppService.ErrorBox("Not a Bare JID.");
				}

				await this.contracts.AuthorizeAccessToContractAsync(this.Contract.ContractId, BareJid, true);
				await this.contracts.SendContractProposal(this.Contract, Role, BareJid);

				AppService.SuccessBox("Proposal successfully sent.");
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Method called (from main thread) when contract options are made available.
		/// </summary>
		/// <param name="Options">Available options, as dictionaries with contract parameters.</param>
		public async Task ShowContractOptions(IDictionary<CaseInsensitiveString, object>[] Options)
		{
			if (Options.Length == 0)
				return;

			if (Options.Length == 1)
				this.ShowSingleContractOptions(Options[0]);
			else
				this.ShowMultipleContractOptions(Options);

			await this.ValidateParameters();
		}

		private void ShowSingleContractOptions(IDictionary<CaseInsensitiveString, object> Option)
		{
			foreach (KeyValuePair<CaseInsensitiveString, object> Parameter in Option)
			{
				string ParameterName = Parameter.Key;

				try
				{
					if (ParameterName.StartsWith("Max(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(')'))
					{
						if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
							continue;

						Info.Parameter.SetMaxValue(Parameter.Value, true);
					}
					else if (ParameterName.StartsWith("Min(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(')'))
					{
						if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
							continue;

						Info.Parameter.SetMinValue(Parameter.Value, true);
					}
					else
					{
						if (!this.parametersByName.TryGetValue(ParameterName, out ParameterInfo Info))
							continue;

						Info.Parameter.SetValue(Parameter.Value);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("Invalid option value. Exception: " + ex.Message,
						new KeyValuePair<string, object>("Parameter", ParameterName),
						new KeyValuePair<string, object>("Value", Parameter.Value),
						new KeyValuePair<string, object>("Type", Parameter.Value?.GetType().FullName ?? string.Empty));

					continue;
				}
			}
		}

		private void ShowMultipleContractOptions(IDictionary<CaseInsensitiveString, object>[] Options)
		{
			CaseInsensitiveString PrimaryKey = this.GetPrimaryKey(Options);

			if (CaseInsensitiveString.IsNullOrEmpty(PrimaryKey))
			{
				Log.Warning("Options not displayed. No primary key could be established. Using only first option.");

				foreach (IDictionary<CaseInsensitiveString, object> Option in Options)
				{
					this.ShowSingleContractOptions(Option);
					break;
				}

				return;
			}

			if (!this.parametersByName.TryGetValue(PrimaryKey, out ParameterInfo Info))
			{
				Log.Warning("Options not displayed. Primary key not available in contract.");
				return;
			}

			this.ParameterOptions.Clear();

			foreach (IDictionary<CaseInsensitiveString, object> Option in Options)
			{
				string Name = MoneyToString.ToString(Option[PrimaryKey]);
				ContractOption ContractOption = new(Name, Option);
				this.ParameterOptions.Add(ContractOption);
			}

			// In MAUI, the picker selection is handled by the view binding to ParameterOptions.
			// The parameter value update is handled via ShowSingleContractOptions when selected.
		}

		private async void Parameter_OptionSelectionChanged(ContractOption Option)
		{
			try
			{
				foreach (KeyValuePair<CaseInsensitiveString, object> P in Option.Option)
				{
					string ParameterName = P.Key;

					try
					{
						if (ParameterName.StartsWith("Max(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(')'))
						{
							if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
								continue;

							Info.Parameter.SetMaxValue(P.Value, true);
						}
						else if (ParameterName.StartsWith("Min(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(')'))
						{
							if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
								continue;

							Info.Parameter.SetMinValue(P.Value, true);
						}
						else
						{
							if (!this.parametersByName.TryGetValue(ParameterName, out ParameterInfo Info))
								continue;

							if (Info.Parameter is StringParameter SP)
							{
								string s = MoneyToString.ToString(P.Value);
								SP.Value = s;
							}
							else if (Info.Parameter is NumericalParameter NP)
							{
								try
								{
									NP.Value = Waher.Script.Expression.ToDecimal(P.Value);
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								catch (Exception ex)
								{
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString() + ": " + ex.Message);
								}
							}
							else if (Info.Parameter is BooleanParameter BP)
							{
								try
								{
									if (P.Value is bool b2)
										BP.Value = b2;
									else if (P.Value is string s && CommonTypes.TryParse(s, out b2))
										BP.Value = b2;
									else
										Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());

									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								catch (Exception ex)
								{
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString() + ": " + ex.Message);
								}
							}
							else if (Info.Parameter is DateTimeParameter DTP)
							{
								if (P.Value is DateTime TP ||
									(P.Value is string s && (DateTime.TryParse(s, out TP) || XML.TryParse(s, out TP))))
								{
									DTP.Value = TP;
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
							}
							else if (Info.Parameter is TimeParameter TSP)
							{
								if (P.Value is TimeSpan TS ||
									(P.Value is string s && TimeSpan.TryParse(s, out TS)))
								{
									TSP.Value = TS;
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
							}
							else if (Info.Parameter is DurationParameter DP)
							{
								if (P.Value is Waher.Content.Duration D ||
									(P.Value is string s && Waher.Content.Duration.TryParse(s, out D)))
								{
									DP.Value = D;
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
							}
							else if (Info.Parameter is ContractReferenceParameter CRP)
							{
								string s = MoneyToString.ToString(P.Value);
								CRP.Value = s;
								Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
							}
							else if (Info.Parameter is GeoParameter GP)
							{
								if (P.Value is GeoPosition Position ||
									(P.Value is string s && GeoPosition.TryParse(s, out Position)))
								{
									GP.Value = Position;
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
							}
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}

				await this.ValidateParameters();
				await this.PopulateHumanReadableText();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private CaseInsensitiveString GetPrimaryKey(IDictionary<CaseInsensitiveString, object>[] Options)
		{
			Dictionary<CaseInsensitiveString, Dictionary<string, bool>> ByKeyAndValue = [];
			LinkedList<CaseInsensitiveString> Keys = [];
			int c = Options.Length;

			foreach (IDictionary<CaseInsensitiveString, object> Option in Options)
			{
				foreach (KeyValuePair<CaseInsensitiveString, object> P in Option)
				{
					if (!ByKeyAndValue.TryGetValue(P.Key, out Dictionary<string, bool> Values))
					{
						Values = [];
						ByKeyAndValue[P.Key] = Values;
						Keys.AddLast(P.Key);
					}

					Values[MoneyToString.ToString(P.Value)] = true;
				}
			}

			foreach (CaseInsensitiveString Key in Keys)
			{
				if (ByKeyAndValue[Key].Count == c &&
					this.parametersByName.TryGetValue(Key, out ParameterInfo Info))
				{
					return Key;
				}
			}

			return CaseInsensitiveString.Empty;
		}

		/// <summary>
		/// Command for uploading an attachment to a contract.
		/// </summary>
		public ICommand UploadAttachment => this.uploadAttachment;

		/// <summary>
		/// If the upload attachment command can be executed.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteUploadAttachment()
		{
			return this.canUploadAttachment.Value;
		}

		/// <summary>
		/// Uploads an attachment to a contract.
		/// </summary>
		public async Task ExecuteUploadAttachment()
		{
			try
			{
				FileResult PickResult = await FilePicker.PickAsync(new PickOptions
				{
					PickerTitle = "Upload Attachment"
				});

				if (PickResult is null)
					return;

				if (!InternetContent.TryGetContentType(Path.GetExtension(PickResult.FullPath), out string ContentType))
					throw new Exception("File type not recognized.");

				AppService.MouseHourglass();

				string FileName = Path.GetFileName(PickResult.FullPath);
				byte[] Data = await Files.ReadAllBytesAsync(PickResult.FullPath);
				long Size = Data.Length;
				byte[] Signature = await this.contracts.SignAsync(Data, SignWith.CurrentKeys);

				HttpFileUploadEventArgs Slot = await this.legalModel.FileUpload.RequestUploadSlotAsync(FileName, ContentType, Size, true);
				if (!Slot.Ok)
					throw Slot.StanzaError;

				await Slot.PUT(Data, ContentType, 30000);

				Contract Contract = await this.contracts.AddContractAttachmentAsync(this.Contract.ContractId, Slot.GetUrl, Signature);

				await this.SetContract(Contract);

				AppService.SuccessBox("File successfully uploaded.");
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}
	}
}
