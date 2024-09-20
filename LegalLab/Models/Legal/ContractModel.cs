using LegalLab.Converters;
using LegalLab.Extensions;
using LegalLab.Models.Design;
using LegalLab.Models.Design.AvalonExtensions;
using LegalLab.Models.Legal.Items;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Persistence;

namespace LegalLab.Models.Legal
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

		private readonly Command addPart;
		private readonly Command createContract;
		private readonly Command uploadAttachment;

		private readonly NonScrollingTextEditor xmlEditor = null;
		private readonly ContractsClient contracts;
		private readonly LegalModel legalModel;
		private StackPanel humanReadableText = null;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="DesignModel">Design Model</param>
		/// <param name="XmlEditor">XML Editor</param>
		private ContractModel(ContractsClient Contracts, Contract Contract, DesignModel DesignModel, NonScrollingTextEditor XmlEditor)
			: base(Contract.Parameters, Contract, "en", DesignModel)
		{
			this.generalInformation = new Property<GenInfo[]>(nameof(this.GeneralInformation), Array.Empty<GenInfo>(), this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), Array.Empty<RoleInfo>(), this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), Array.Empty<PartInfo>(), this);
			this.attachments = new Property<AttachmentInfo[]>(nameof(this.Attachments), Array.Empty<AttachmentInfo>(), this);
			this.clientSignatures = new Property<ClientSignatureInfo[]>(nameof(this.ClientSignatures), Array.Empty<ClientSignatureInfo>(), this);
			this.serverSignatures = new Property<ServerSignatureInfo[]>(nameof(this.ServerSignatures), Array.Empty<ServerSignatureInfo>(), this);
			this.hasId = new Property<bool>(nameof(this.HasId), false, this);
			this.canBeSigned = new Property<bool>(nameof(this.CanBeSigned), false, this);
			this.canUploadAttachment = new Property<bool>(nameof(this.CanUploadAttachment), false, this);
			this.canDownloadAttachment = new Property<bool>(nameof(this.CanDownloadAttachment), false, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);
			this.qrCodeUri = new Property<string>(nameof(this.QrCodeUri), string.Empty, this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.templateName = new Property<string>(nameof(this.TemplateName), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), Contract.ContractId, this);

			this.ParameterOptions = new ObservableCollection<ContractOption>();

			this.addPart = new Command(this.ExecuteAddPart);
			this.createContract = new Command(this.CanExecuteCreateContract, this.ExecuteCreateContract);
			this.uploadAttachment = new Command(this.CanExecuteUploadAttachment, this.ExecuteUploadAttachment);

			this.xmlEditor = XmlEditor;
			this.legalModel = DesignModel.Network.Legal;
			this.contracts = Contracts;

			this.TemplateName = Contract.ContractId;
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
		/// <param name="XmlEditor">XML Editor</param>
		/// <returns>Contract object model.</returns>
		public static async Task<ContractModel> CreateAsync(ContractsClient Contracts, Contract Contract, DesignModel DesignModel,
			NonScrollingTextEditor XmlEditor)
		{
			ContractModel Result = new(Contracts, Contract, DesignModel, XmlEditor);
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
			this.QrCodeUri = "https://" + Domain + "/QR/" + this.Uri;

			List<GenInfo> GenInfo = new()
			{
				new GenInfo("Created:", this.Contract.Created.ToString(CultureInfo.CurrentUICulture))
			};

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

			this.GeneralInformation = GenInfo.ToArray();

			List<PartInfo> Parts = new();

			if (this.Contract.Parts is not null)
			{
				foreach (Part Part in this.Contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new();

			if (this.Contract.Roles is not null)
			{
				foreach (Role Role in this.Contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = Roles.ToArray();

			List<AttachmentInfo> Attachments = new();

			if (this.Contract.Attachments is not null)
			{
				foreach (Attachment Attachment in this.Contract.Attachments)
					Attachments.Add(new AttachmentInfo(this, Attachment));
			}

			this.Attachments = Attachments.ToArray();

			List<ClientSignatureInfo> ClientSignatures = new();

			if (this.Contract.ClientSignatures is not null)
			{
				foreach (ClientSignature ClientSignature in this.Contract.ClientSignatures)
					ClientSignatures.Add(new ClientSignatureInfo(this.contracts, ClientSignature));
			}

			this.ClientSignatures = ClientSignatures.ToArray();

			if (this.Contract.ServerSignature is null)
				this.ServerSignatures = Array.Empty<ServerSignatureInfo>();
			else
				this.ServerSignatures = new ServerSignatureInfo[] { new(this.Contract.ServerSignature) };

			await this.PopulateHumanReadableText();
		}

		/// <inheritdoc/>
		public override Task Start()
		{
			this.contracts.ContractUpdated += this.Contracts_ContractUpdated;
			this.contracts.ContractSigned += this.Contracts_ContractSigned;

			return base.Start();
		}

		/// <inheritdoc/>
		public override Task Stop()
		{
			this.contracts.ContractUpdated -= this.Contracts_ContractUpdated;
			this.contracts.ContractSigned -= this.Contracts_ContractSigned;

			return base.Stop();
		}

		private Task Contracts_ContractSigned(object Sender, ContractSignedEventArgs e)
		{
			return this.CheckReload(e);
		}

		private Task Contracts_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			return this.CheckReload(e);
		}

		private async Task CheckReload(ContractReferenceEventArgs e)
		{
			if (e.ContractId == this.ContractId)
			{
				Contract Contract = await this.contracts.GetContractAsync(this.ContractId);
				MainWindow.UpdateGui(async () =>
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
						RI.DescriptionAsMarkdown = (RI.Role.Descriptions.Find(Language)?.GenerateMarkdown(this.Contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

					await this.PopulateHumanReadableText();
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				MainWindow.ErrorBox(ex.Message);

				return false;
			}
		}

		protected override void ParametersOkChanged()
		{
			this.createContract.RaiseCanExecuteChanged();
		}

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
			set
			{
				this.machineReadable.Value = value;

				if (this.xmlEditor is not null)
					this.xmlEditor.Text = value;
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
		/// The different parameter options available to choose from when creating a contract.
		/// </summary>
		public ObservableCollection<ContractOption> ParameterOptions { get; }

		protected override Task ParametersChanged()
		{
			return this.PopulateHumanReadableText();
		}

		private async Task PopulateHumanReadableText()
		{
			if (this.humanReadableText is null)
				return;

			this.humanReadableText.Children.Clear();

			if (XamlReader.Parse(await this.Contract.ToXAML(this.Language)) is StackPanel Panel)
			{
				LinkedList<UIElement> Elements = new();

				foreach (UIElement Item in Panel.Children)
					Elements.AddLast(Item);

				Panel.Children.Clear();

				foreach (UIElement Item in Elements)
					this.humanReadableText.Children.Add(Item);
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
			get => this.roles?.Value ?? Array.Empty<RoleInfo>();
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
					return Array.Empty<string>();

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
		/// Displays the contents of the contract
		/// </summary>
		/// <param name="ContractLayout">Where to layout the contract</param>
		/// <param name="HumanReadableText">Control where human-readable content is placed</param>
		public async Task PopulateContract(StackPanel ContractLayout, StackPanel HumanReadableText)
		{
			this.humanReadableText = HumanReadableText;
			await this.PopulateHumanReadableText();

			ContractLayout.DataContext = this;
			ContractLayout.Visibility = Visibility.Visible;

			// TODO: Attachments
		}

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
		/// If the create contract command can be exeucted.
		/// </summary>
		/// <returns>If command can be executed.</returns>
		public bool CanExecuteCreateContract()
		{
			return this.ParametersOk;
		}

		/// <summary>
		/// Proposes the contract.
		/// </summary>
		public async Task ExecuteCreateContract()
		{
			try
			{
				if (MessageBox.Show("Are you sure you want to create the contract on " + this.contracts.ComponentAddress + "?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}

				MainWindow.MouseHourglass();

				string TemplateId = this.legalModel.Template.ContractId;

				Contract Contract = await this.contracts.CreateContractAsync(TemplateId, this.Contract.Parts, this.Contract.Parameters,
					this.Contract.Visibility, this.Parts.Length > 0 ? ContractParts.ExplicitlyDefined : ContractParts.Open,
					this.Contract.Duration, this.Contract.ArchiveRequired, this.Contract.ArchiveOptional, this.Contract.SignAfter,
					this.Contract.SignBefore, false);

				await this.SetContract(Contract);

				MainWindow.SuccessBox("Contract successfully created.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Signs the contract, as the given role
		/// </summary>
		/// <param name="Role">Role to sign the contract as.</param>
		public async Task SignAsRole(string Role)
		{
			try
			{
				if (MessageBox.Show("Are you sure you want to sign the contract as " + Role + "?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}

				MainWindow.MouseHourglass();

				Contract Contract = await this.contracts.SignContractAsync(this.Contract, Role, false);

				await this.SetContract(Contract);

				MainWindow.SuccessBox("Contract successfully signed.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
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
					BareJid = MainWindow.PromptUser("Recommend contract", "Recommend this contract to be signed by (Bare JID):", BareJid);
					if (string.IsNullOrEmpty(BareJid))
						return;

					if (XmppClient.BareJidRegEx.IsMatch(BareJid))
						break;
					else
						MainWindow.ErrorBox("Not a Bare JID.");
				}

				await this.contracts.AuthorizeAccessToContractAsync(this.Contract.ContractId, BareJid, true);
				await this.contracts.SendContractProposal(this.Contract, Role, BareJid);

				MainWindow.SuccessBox("Proposal successfully sent.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Method called (from main thread) when contract options are made available.
		/// </summary>
		/// <param name="Page">Page currently being viewed</param>
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
					if (ParameterName.StartsWith("Max(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(")"))
					{
						if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
							continue;

						Info.Parameter.SetMaxValue(Parameter.Value, true);
					}
					else if (ParameterName.StartsWith("Min(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(")"))
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

						if (Info.Control is TextBox Entry)
							Entry.Text = MoneyToString.ToString(Parameter.Value);
						else if (Info.Control is CheckBox CheckBox)
						{
							if (Parameter.Value is bool b)
								CheckBox.IsChecked = b;
							else if (Parameter.Value is int i)
								CheckBox.IsChecked = i != 0;
							else if (Parameter.Value is double d)
								CheckBox.IsChecked = d != 0;
							else if (Parameter.Value is decimal d2)
								CheckBox.IsChecked = d2 != 0;
							else if (Parameter.Value is string s && CommonTypes.TryParse(s, out b))
								CheckBox.IsChecked = b;
							else
							{
								Log.Warning("Invalid option value.",
									new KeyValuePair<string, object>("Parameter", ParameterName),
									new KeyValuePair<string, object>("Value", Parameter.Value),
									new KeyValuePair<string, object>("Type", Parameter.Value?.GetType().FullName ?? string.Empty));
							}
						}
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

			if (Info.Control is not TextBox Entry)
			{
				Log.Warning("Options not displayed. Parameter control not of a type that allows a selection control to be created.");
				return;
			}

			this.ParameterOptions.Clear();

			ContractOption SelectedOption = null;

			foreach (IDictionary<CaseInsensitiveString, object> Option in Options)
			{
				string Name = MoneyToString.ToString(Option[PrimaryKey]);
				ContractOption ContractOption = new(Name, Option);

				this.ParameterOptions.Add(ContractOption);

				if (Name == Entry.Text)
					SelectedOption = ContractOption;
			}

			ComboBox Picker = new()
			{
				Tag = Info.Parameter.Name,
				ItemsSource = this.ParameterOptions,
				ToolTip = Info.Parameter.Guide
			};

			int ControlIndex;

			if (Info.Control?.Parent is StackPanel Panel &&
				(ControlIndex = Panel.Children.IndexOf(Info.Control)) >= 0)
			{
				Panel.Children.Remove(Info.Control);
				Panel.Children.Insert(ControlIndex, Picker);
			}
			else
				Log.Warning("Options not displayed. Primary Key Entry not found.");

			Picker.SelectionChanged += this.Parameter_OptionSelectionChanged;
			Info.Control = Picker;

			if (SelectedOption is not null)
				Picker.SelectedItem = SelectedOption;
		}

		private async void Parameter_OptionSelectionChanged(object Sender, EventArgs e)
		{
			if (Sender is not ComboBox Picker)
				return;

			if (Picker.SelectedItem is not ContractOption Option)
				return;

			try
			{
				foreach (KeyValuePair<CaseInsensitiveString, object> P in Option.Option)
				{
					string ParameterName = P.Key;

					try
					{
						if (ParameterName.StartsWith("Max(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(")"))
						{
							if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
								continue;

							Info.Parameter.SetMaxValue(P.Value, true);
						}
						else if (ParameterName.StartsWith("Min(", StringComparison.CurrentCultureIgnoreCase) && ParameterName.EndsWith(")"))
						{
							if (!this.parametersByName.TryGetValue(ParameterName[4..^1].Trim(), out ParameterInfo Info))
								continue;

							Info.Parameter.SetMinValue(P.Value, true);
						}
						else
						{
							if (!this.parametersByName.TryGetValue(ParameterName, out ParameterInfo Info))
								continue;

							TextBox Entry = Info.Control as TextBox;

							if (Info.Parameter is StringParameter SP)
							{
								string s = MoneyToString.ToString(P.Value);

								SP.Value = s;

								if (Entry is not null)
								{
									Entry.Text = s;
									Entry.Background = Info.Protection.DefaultBrush();
								}
							}
							else if (Info.Parameter is NumericalParameter NP)
							{
								try
								{
									NP.Value = Waher.Script.Expression.ToDecimal(P.Value);

									if (Entry is not null)
										Entry.Background = Info.Protection.DefaultBrush();
									
									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								catch (Exception ex)
								{
									if (Entry is not null)
										Entry.Background = Brushes.Salmon;

									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString() + ": " + ex.Message);
								}
							}
							else if (Info.Parameter is BooleanParameter BP)
							{
								CheckBox CheckBox = Info.Control as CheckBox;

								try
								{
									if (P.Value is bool b2)
										BP.Value = b2;
									else if (P.Value is string s && CommonTypes.TryParse(s, out b2))
										BP.Value = b2;
									else
									{
										if (CheckBox is not null)
											CheckBox.Background = Brushes.Salmon;

										Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());

										continue;
									}

									if (CheckBox is not null)
										CheckBox.Background = Info.Protection.DefaultBrush();

									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								catch (Exception ex)
								{
									if (CheckBox is not null)
										CheckBox.Background = Brushes.Salmon;

									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString() + ": " + ex.Message);
								}
							}
							else if (Info.Parameter is DateTimeParameter DTP)
							{
								if (P.Value is DateTime TP ||
									(P.Value is string s && (DateTime.TryParse(s, out TP) || XML.TryParse(s, out TP))))
								{
									DTP.Value = TP;

									if (Entry is not null)
										Entry.Background = Info.Protection.DefaultBrush();

									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
								{
									if (Entry is not null)
										Entry.Background = Brushes.Salmon;

									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
								}
							}
							else if (Info.Parameter is TimeParameter TSP)
							{
								if (P.Value is TimeSpan TS ||
									(P.Value is string s && TimeSpan.TryParse(s, out TS)))
								{
									TSP.Value = TS;

									if (Entry is not null)
										Entry.Background = Info.Protection.DefaultBrush();

									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
								{
									if (Entry is not null)
										Entry.Background = Brushes.Salmon;

									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
								}
							}
							else if (Info.Parameter is DurationParameter DP)
							{
								if (P.Value is Waher.Content.Duration D ||
									(P.Value is string s && Waher.Content.Duration.TryParse(s, out D)))
								{
									DP.Value = D;

									if (Entry is not null)
										Entry.Background = Info.Protection.DefaultBrush();

									Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
								}
								else
								{
									if (Entry is not null)
										Entry.Background = Brushes.Salmon;

									Log.Error("Unable to set parameter " + Info.Parameter.Name + " to " + P.Value?.ToString());
								}
							}
							else if (Info.Parameter is ContractReferenceParameter CRP)
							{
								string s = MoneyToString.ToString(P.Value);

								CRP.Value = s;

								if (Entry is not null)
								{
									Entry.Text = s;
									Entry.Background = Info.Protection.DefaultBrush();
								}

								Log.Informational("Parameter " + Info.Parameter.Name + " set to " + P.Value?.ToString());
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
			Dictionary<CaseInsensitiveString, Dictionary<string, bool>> ByKeyAndValue = new();
			LinkedList<CaseInsensitiveString> Keys = new();
			int c = Options.Length;

			foreach (IDictionary<CaseInsensitiveString, object> Option in Options)
			{
				foreach (KeyValuePair<CaseInsensitiveString, object> P in Option)
				{
					if (!ByKeyAndValue.TryGetValue(P.Key, out Dictionary<string, bool> Values))
					{
						Values = new Dictionary<string, bool>();
						ByKeyAndValue[P.Key] = Values;
						Keys.AddLast(P.Key);
					}

					Values[MoneyToString.ToString(P.Value)] = true;
				}
			}

			foreach (CaseInsensitiveString Key in Keys)
			{
				if (ByKeyAndValue[Key].Count == c &&
					this.parametersByName.TryGetValue(Key, out ParameterInfo Info) &&
					Info.Control is TextBox)
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
				OpenFileDialog Dialog = new()
				{
					CheckFileExists = true,
					CheckPathExists = true,
					Filter = "All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Upload Attachment"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return;

				if (!InternetContent.TryGetContentType(Path.GetExtension(Dialog.FileName), out string ContentType))
					throw new Exception("File type not recognized.");

				MainWindow.MouseHourglass();

				string FileName = Path.GetFileName(Dialog.FileName);
				byte[] Data = await Resources.ReadAllBytesAsync(Dialog.FileName);
				long Size = Data.Length;
				byte[] Signature = await this.contracts.SignAsync(Data, SignWith.CurrentKeys);

				HttpFileUploadEventArgs Slot = await this.legalModel.FileUpload.RequestUploadSlotAsync(FileName, ContentType, Size, true);
				if (!Slot.Ok)
					throw Slot.StanzaError;

				await Slot.PUT(Data, ContentType, 30000);

				Contract Contract = await this.contracts.AddContractAttachmentAsync(this.Contract.ContractId, Slot.GetUrl, Signature);

				await this.SetContract(Contract);

				MainWindow.SuccessBox("File successfully uploaded.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

	}
}
