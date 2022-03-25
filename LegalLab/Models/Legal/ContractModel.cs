using LegalLab.Extensions;
using LegalLab.Models.Design;
using LegalLab.Models.Legal.Items;
using LegalLab.Models.Legal.Items.Parameters;
using LegalLab.Tabs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Settings;
using Waher.Script;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contract model
	/// </summary>
	public class ContractModel : Model, IPartsModel
	{
		private readonly Property<bool> parametersOk;
		private readonly Property<GenInfo[]> generalInformation;
		private readonly Property<RoleInfo[]> roles;
		private readonly Property<PartInfo[]> parts;
		private readonly Property<ParameterInfo[]> parameters;
		private readonly Property<ClientSignatureInfo[]> clientSignatures;
		private readonly Property<ServerSignatureInfo[]> serverSignatures;
		private readonly Property<bool> hasId;
		private readonly Property<bool> canBeSigned;
		private readonly Property<string> uri;
		private readonly Property<string> qrCodeUri;
		private readonly Property<string> machineReadable;
		private readonly Property<string> templateName;
		private readonly Property<string> contractId;

		private readonly Command addPart;
		private readonly Command createContract;

		private readonly Dictionary<string, ParameterInfo> parametersByName = new Dictionary<string, ParameterInfo>();
		private readonly ContractsClient contracts;
		private readonly LegalModel legalModel;
		private readonly ContractsTab contractsTab;
		private Contract contract;
		private StackPanel humanReadableText = null;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="LegalModel">Legal Model</param>
		/// <param name="ContractsTab">Tab where contract is being displayed.</param>
		public ContractModel(ContractsClient Contracts, Contract Contract, LegalModel LegalModel, ContractsTab ContractsTab)
		{
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.generalInformation = new Property<GenInfo[]>(nameof(this.GeneralInformation), new GenInfo[0], this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), new RoleInfo[0], this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), new PartInfo[0], this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), new ParameterInfo[0], this);
			this.clientSignatures = new Property<ClientSignatureInfo[]>(nameof(this.ClientSignatures), new ClientSignatureInfo[0], this);
			this.serverSignatures = new Property<ServerSignatureInfo[]>(nameof(this.ServerSignatures), new ServerSignatureInfo[0], this);
			this.hasId = new Property<bool>(nameof(this.HasId), false, this);
			this.canBeSigned = new Property<bool>(nameof(this.CanBeSigned), false, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);
			this.qrCodeUri = new Property<string>(nameof(this.QrCodeUri), string.Empty, this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.templateName = new Property<string>(nameof(this.TemplateName), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), Contract.ContractId, this);

			this.addPart = new Command(this.ExecuteAddPart);
			this.createContract = new Command(this.CanExecuteCreateContract, this.ExecuteCreateContract);

			this.contractsTab = ContractsTab;
			this.legalModel = LegalModel;
			this.contracts = Contracts;
			this.SetContract(Contract);

			this.TemplateName = Contract.ContractId;

			(string s, XmlElement E) = Contract.ForMachines.ToPrettyXml();

			this.MachineReadable = s;
		}

		private void SetContract(Contract Contract)
		{
			this.contract = Contract;

			this.ContractId = Contract.ContractId;
			this.HasId = !string.IsNullOrEmpty(Contract.ContractId);
			this.CanBeSigned = Contract.State == ContractState.Approved || Contract.State == ContractState.BeingSigned;
			this.Uri = ContractsClient.ContractIdUriString(Contract.ContractId);
			this.QrCodeUri = "https://" + this.contracts.Client.Domain + "/QR/" + this.Uri;

			List<GenInfo> GenInfo = new List<GenInfo>()
			{
				new GenInfo("Created:", this.contract.Created.ToString(CultureInfo.CurrentUICulture))
			};

			if (this.contract.Updated > DateTime.MinValue)
				GenInfo.Add(new GenInfo("Updated:", this.contract.Updated.ToString(CultureInfo.CurrentUICulture)));

			GenInfo.Add(new GenInfo("State:", this.contract.State.ToString()));
			GenInfo.Add(new GenInfo("Visibility:", this.contract.Visibility.ToString()));
			GenInfo.Add(new GenInfo("Duration:", this.contract.Duration.ToString()));
			GenInfo.Add(new GenInfo("From:", this.contract.From.ToString(CultureInfo.CurrentUICulture)));
			GenInfo.Add(new GenInfo("To:", this.contract.To.ToString(CultureInfo.CurrentUICulture)));
			GenInfo.Add(new GenInfo("Archiving Optional:", this.contract.ArchiveOptional.ToString()));
			GenInfo.Add(new GenInfo("Archiving Required:", this.contract.ArchiveRequired.ToString()));
			GenInfo.Add(new GenInfo("Can act as a template:", this.contract.CanActAsTemplate.ToYesNo()));
			GenInfo.Add(new GenInfo("Provider:", this.contract.Provider));
			GenInfo.Add(new GenInfo("Parts:", this.contract.PartsMode.ToString()));

			if (this.contract.SignAfter.HasValue)
				GenInfo.Add(new GenInfo("Sign after:", this.contract.SignAfter.Value.ToStringTZ()));

			if (this.contract.SignBefore.HasValue)
				GenInfo.Add(new GenInfo("Sign before:", this.contract.SignBefore.Value.ToStringTZ()));

			if (!string.IsNullOrEmpty(this.contract.TemplateId))
				GenInfo.Add(new GenInfo("Template ID:", this.contract.TemplateId));

			this.GeneralInformation = GenInfo.ToArray();

			List<PartInfo> Parts = new List<PartInfo>();

			if (!(this.contract.Parts is null))
			{
				foreach (Part Part in this.contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new List<RoleInfo>();

			if (!(this.contract.Roles is null))
			{
				foreach (Role Role in this.contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> Parameters = new List<ParameterInfo>();

			if (!(this.contract.Parameters is null))
			{
				foreach (Parameter Parameter in this.contract.Parameters)
				{
					if (this.parametersByName.TryGetValue(Parameter.Name, out ParameterInfo ParameterInfo))
					{
						Parameters.Add(ParameterInfo);
						ParameterInfo.ContractUpdated(this.contract);
					}
				}
			}

			this.Parameters = Parameters.ToArray();

			List<ClientSignatureInfo> ClientSignatures = new List<ClientSignatureInfo>();

			if (!(this.contract.ClientSignatures is null))
			{
				foreach (ClientSignature ClientSignature in this.contract.ClientSignatures)
					ClientSignatures.Add(new ClientSignatureInfo(this.contracts, ClientSignature));
			}

			this.ClientSignatures = ClientSignatures.ToArray();

			if (this.contract.ServerSignature is null)
				this.ServerSignatures = new ServerSignatureInfo[0];
			else
				this.ServerSignatures = new ServerSignatureInfo[] { new ServerSignatureInfo(this.contract.ServerSignature) };
		}

		/// <inheritdoc/>
		public override Task Start()
		{
			this.contracts.ContractUpdated += Contracts_ContractUpdated;
			this.contracts.ContractSigned += Contracts_ContractSigned;

			return base.Start();
		}

		/// <inheritdoc/>
		public override Task Stop()
		{
			this.contracts.ContractUpdated -= Contracts_ContractUpdated;
			this.contracts.ContractSigned -= Contracts_ContractSigned;

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
				this.SetContract(Contract);
			}
		}

		/// <summary>
		/// Referenced contract
		/// </summary>
		public Contract Contract => this.contract;

		/// <summary>
		/// If all parameters are OK or not
		/// </summary>
		public bool ParametersOk
		{
			get => this.parametersOk.Value;
			set
			{
				this.parametersOk.Value = value;
				this.createContract.RaiseCanExecuteChanged();
			}
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
				this.contractsTab.MachineReadableXmlEditor.Text = value;
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
		/// Proposes the temaplte.
		/// </summary>
		public async void ExecuteProposeTemplate()
		{
			try
			{
				if (MessageBox.Show("Are you sure you want to propose the loaded template to " + this.contracts.ComponentAddress + "?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}

				MainWindow.MouseHourglass();

				Contract Contract = await this.contracts.CreateContractAsync(this.contract.ForMachines, this.contract.ForHumans, this.contract.Roles,
					this.contract.Parts, this.contract.Parameters, this.contract.Visibility, this.contract.PartsMode, this.contract.Duration,
					this.contract.ArchiveRequired, this.contract.ArchiveOptional, this.contract.SignAfter, this.contract.SignBefore,
					this.contract.CanActAsTemplate);

				this.SetContract(Contract);

				await RuntimeSettings.SetAsync("Contract.Template." + this.TemplateName, Contract.ContractId);
				this.legalModel.ContractTemplateAdded(this.TemplateName, Contract);

				MainWindow.SuccessBox("Template successfully proposed.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Populates a <see cref="StackPanel"/> with parameter controls.
		/// </summary>
		/// <param name="Parameters">StackPanel to populate</param>
		public async Task PopulateParameters(StackPanel Parameters, StackPanel AdditionalCommands)
		{
			List<ParameterInfo> ParameterList = new List<ParameterInfo>();
			ParameterInfo ParameterInfo;

			Parameters.Children.Clear();
			this.parametersByName.Clear();

			Parameters.DataContext = this;

			foreach (Parameter Parameter in this.contract.Parameters)
			{
				if (Parameter is BooleanParameter BP)
				{
					CheckBox CheckBox = new CheckBox()
					{
						Tag = Parameter.Name,
						IsChecked = BP.Value.HasValue && BP.Value.Value,
						VerticalContentAlignment = VerticalAlignment.Center,
						Content = Parameter.GetLabel(),
						ToolTip = await Parameter.ToSimpleXAML(this.contract.DefaultLanguage, this.contract),
						Margin = new Thickness(0, 10, 0, 0)
					};

					CheckBox.Checked += Parameter_CheckedChanged;
					CheckBox.Unchecked += Parameter_CheckedChanged;

					this.parametersByName[Parameter.Name] = ParameterInfo = new BooleanParameterInfo(this.contract, BP, CheckBox, null, this.parameters);

					Parameters.Children.Add(CheckBox);
				}
				else
				{
					Label Label = new Label()
					{
						Content = Parameter.GetLabel(),
						Margin = new Thickness(0, 10, 0, 0)
					};

					TextBox TextBox = new TextBox()
					{
						Tag = Parameter.Name,
						Text = Parameter.ObjectValue?.ToString(),
						ToolTip = await Parameter.ToSimpleXAML(this.contract.DefaultLanguage, this.contract)
					};

					TextBox.TextChanged += Parameter_TextChanged;

					if (Parameter is NumericalParameter NP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new NumericalParameterInfo(this.contract, NP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is StringParameter SP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new StringParameterInfo(this.contract, SP, TextBox,
							null, null, null, null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DateParameter DP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new DateParameterInfo(this.contract, DP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DateTimeParameter DTP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new DateTimeParameterInfo(this.contract, DTP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is TimeParameter TP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new TimeParameterInfo(this.contract, TP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DurationParameter DrP)
					{
						this.parametersByName[Parameter.Name] = ParameterInfo = new DurationParameterInfo(this.contract, DrP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is CalcParameter CP)
					{
						TextBox.IsReadOnly = true;

						this.parametersByName[Parameter.Name] = ParameterInfo = new CalcParameterInfo(this.contract, CP, TextBox,
							null, this.parameters);
					}
					else
						continue;

					Parameters.Children.Add(Label);
					Parameters.Children.Add(TextBox);
				}

				ParameterList.Add(ParameterInfo);
			}

			this.Parameters = ParameterList.ToArray();

			await this.ValidateParameters();
			await PopulateHumanReadableText();

			AdditionalCommands.DataContext = this;
			AdditionalCommands.Visibility = Visibility.Visible;
			AdditionalCommands.InvalidateVisual();
		}

		private async void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(sender is CheckBox CheckBox) || !this.parametersByName.TryGetValue(CheckBox.Tag.ToString(), out ParameterInfo ParameterInfo))
					return;

				ParameterInfo.Value = CheckBox.IsChecked;

				await this.ValidateParameters();
				await PopulateHumanReadableText();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_TextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(sender is TextBox TextBox) || !this.parametersByName.TryGetValue(TextBox.Tag.ToString(), out ParameterInfo ParameterInfo))
					return;

				try
				{
					if (ParameterInfo.Parameter is NumericalParameter && decimal.TryParse(TextBox.Text, out decimal d))
						ParameterInfo.Value = d;
					else if (ParameterInfo.Parameter is BooleanParameter && bool.TryParse(TextBox.Text, out bool b))
						ParameterInfo.Value = b;
					else if (ParameterInfo.Parameter is DateParameter && DateTime.TryParse(TextBox.Text, out DateTime TP))
					{
						if (TP.TimeOfDay != TimeSpan.Zero)
							throw new Exception("Date only.");

						ParameterInfo.Value = TP;
					}
					else if (ParameterInfo.Parameter is DateTimeParameter && DateTime.TryParse(TextBox.Text, out TP))
						ParameterInfo.Value = TP;
					else if (ParameterInfo.Parameter is TimeParameter && TimeSpan.TryParse(TextBox.Text, out TimeSpan TS))
						ParameterInfo.Value = TS;
					else if (ParameterInfo.Parameter is DurationParameter && Waher.Content.Duration.TryParse(TextBox.Text, out Waher.Content.Duration Dr))
						ParameterInfo.Value = Dr;
					else if (!(ParameterInfo.Parameter is CalcParameter))
						ParameterInfo.Value = TextBox.Text;

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}

				await PopulateHumanReadableText();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task ValidateParameters()
		{
			Variables Variables = new Variables();
			bool Ok = true;

			foreach (ParameterInfo P in this.parametersByName.Values)
				P.Parameter.Populate(Variables);

			foreach (ParameterInfo P in this.parametersByName.Values)
			{
				if (await P.ValidateParameter(Variables))
					P.Control.Background = null;
				else
				{
					P.Control.Background = Brushes.Salmon;
					Ok = false;
				}
			}

			this.ParametersOk = Ok;
		}

		private async Task PopulateHumanReadableText()
		{
			if (this.humanReadableText is null)
				return;

			this.humanReadableText.Children.Clear();

			if (XamlReader.Parse(await this.contract.ToXAML(this.contract.DefaultLanguage)) is StackPanel Panel)
			{
				LinkedList<UIElement> Elements = new LinkedList<UIElement>();

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
			get => this.roles.Value;
			set => this.roles.Value = value;
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
				this.contract.Parts = value.ToParts();
			}
		}

		/// <summary>
		/// Parameters defined the contract.
		/// </summary>
		public ParameterInfo[] Parameters
		{
			get => this.parameters.Value;
			set => this.parameters.Value = value;
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
		public bool ContractLoaded => !(this.contract is null);

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

			if (this.contract.PartsMode != ContractParts.ExplicitlyDefined)
			{
				this.contract.PartsMode = ContractParts.ExplicitlyDefined;

				foreach (GenInfo GI in this.GeneralInformation)
				{
					if (GI.Name == "Parts:")
						GI.Value = this.contract.PartsMode.ToString();
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
			int i = Array.IndexOf<PartInfo>(Parts, Part);
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
		/// <returns></returns>
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

				Contract Contract = await this.contracts.CreateContractAsync(TemplateId, this.contract.Parts, this.contract.Parameters,
					this.contract.Visibility, this.Parts.Length > 0 ? ContractParts.ExplicitlyDefined : ContractParts.Open,
					this.contract.Duration, this.contract.ArchiveRequired, this.contract.ArchiveOptional, this.contract.SignAfter,
					this.contract.SignBefore, false);

				this.SetContract(Contract);

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

				Contract Contract = await this.contracts.SignContractAsync(this.contract, Role, false);

				this.SetContract(Contract);

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
		public void ProposeForRole(string Role)
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

				this.contracts.SendContractProposal(this.contract.ContractId, Role, BareJid);

				MainWindow.SuccessBox("Proposal successfully sent.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

	}
}
