using LegalLab.Extensions;
using LegalLab.Models.Design;
using LegalLab.Models.Legal.Items;
using LegalLab.Models.Legal.Items.Parameters;
using LegalLab.Models.Standards;
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
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
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
		private readonly Property<string> language;
		private readonly Property<Iso__639_1.Record[]> languages;

		private readonly Command addPart;
		private readonly Command createContract;

		private readonly Dictionary<string, ParameterInfo> parametersByName = new();
		private readonly ContractsClient contracts;
		private readonly LegalModel legalModel;
		private readonly ContractsTab contractsTab;
		private Contract contract;
		private StackPanel humanReadableText = null;
		private StackPanel additionalCommands = null;
		private StackPanel languageOptions = null;
		private StackPanel parameterOptions = null;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="LegalModel">Legal Model</param>
		/// <param name="ContractsTab">Tab where contract is being displayed.</param>
		private ContractModel(ContractsClient Contracts, Contract Contract, LegalModel LegalModel, ContractsTab ContractsTab)
		{
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.generalInformation = new Property<GenInfo[]>(nameof(this.GeneralInformation), Array.Empty<GenInfo>(), this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), Array.Empty<RoleInfo>(), this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), Array.Empty<PartInfo>(), this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), Array.Empty<ParameterInfo>(), this);
			this.clientSignatures = new Property<ClientSignatureInfo[]>(nameof(this.ClientSignatures), Array.Empty<ClientSignatureInfo>(), this);
			this.serverSignatures = new Property<ServerSignatureInfo[]>(nameof(this.ServerSignatures), Array.Empty<ServerSignatureInfo>(), this);
			this.hasId = new Property<bool>(nameof(this.HasId), false, this);
			this.canBeSigned = new Property<bool>(nameof(this.CanBeSigned), false, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);
			this.qrCodeUri = new Property<string>(nameof(this.QrCodeUri), string.Empty, this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.templateName = new Property<string>(nameof(this.TemplateName), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), Contract.ContractId, this);
			this.language = new Property<string>(nameof(this.Language), "en", this);
			this.languages = new Property<Iso__639_1.Record[]>(nameof(this.Languages), Array.Empty<Iso__639_1.Record>(), this);

			this.addPart = new Command(this.ExecuteAddPart);
			this.createContract = new Command(this.CanExecuteCreateContract, this.ExecuteCreateContract);

			this.contractsTab = ContractsTab;
			this.legalModel = LegalModel;
			this.contracts = Contracts;

			this.TemplateName = Contract.ContractId;
		}

		/// <summary>
		/// Creates the contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="LegalModel">Legal Model</param>
		/// <param name="ContractsTab">Tab where contract is being displayed.</param>
		/// <returns>Contract object model.</returns>
		public static async Task<ContractModel> CreateAsync(ContractsClient Contracts, Contract Contract, LegalModel LegalModel, ContractsTab ContractsTab)
		{
			ContractModel Result = new(Contracts, Contract, LegalModel, ContractsTab);
			(string s, _) = Contract.ForMachines.ToPrettyXml();

			await Result.SetContract(Contract);
			Result.MachineReadable = s;

			return Result;
		}

		private async Task SetContract(Contract Contract)
		{
			this.contract = Contract;

			this.Languages = Contract.GetLanguages().ToIso639_1();
			this.Language = Contract.DefaultLanguage;

			if (string.IsNullOrEmpty(this.Language) && this.Languages.Length == 0)
			{
				this.Languages = new string[] { "en" }.ToIso639_1();
				this.Language = "en";
			}

			this.ContractId = Contract.ContractId;
			this.HasId = !string.IsNullOrEmpty(Contract.ContractId);
			this.CanBeSigned = Contract.State == ContractState.Approved || Contract.State == ContractState.BeingSigned;
			this.Uri = ContractsClient.ContractIdUriString(Contract.ContractId);
			this.QrCodeUri = "https://" + this.contracts.Client.Domain + "/QR/" + this.Uri;

			List<GenInfo> GenInfo = new()
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

			List<PartInfo> Parts = new();

			if (this.contract.Parts is not null)
			{
				foreach (Part Part in this.contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new();

			if (this.contract.Roles is not null)
			{
				foreach (Role Role in this.contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> Parameters = new();

			if (this.contract.Parameters is not null)
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

			List<ClientSignatureInfo> ClientSignatures = new();

			if (this.contract.ClientSignatures is not null)
			{
				foreach (ClientSignature ClientSignature in this.contract.ClientSignatures)
					ClientSignatures.Add(new ClientSignatureInfo(this.contracts, ClientSignature));
			}

			this.ClientSignatures = ClientSignatures.ToArray();

			if (this.contract.ServerSignature is null)
				this.ServerSignatures = Array.Empty<ServerSignatureInfo>();
			else
				this.ServerSignatures = new ServerSignatureInfo[] { new ServerSignatureInfo(this.contract.ServerSignature) };

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

		/// <summary>
		/// Referenced contract
		/// </summary>
		public Contract Contract => this.contract;

		/// <summary>
		/// Currently selected language.
		/// </summary>
		public string Language
		{
			get => this.language.Value;
			set => this.SetLanguage(value);
		}

		/// <summary>
		/// Available languages.
		/// </summary>
		public Iso__639_1.Record[] Languages
		{
			get => this.languages.Value;
			set => this.languages.Value = value;
		}

		private async void SetLanguage(string Language)
		{
			try
			{
				this.language.Value = Language;

				if (!string.IsNullOrEmpty(Language))
				{
					foreach (ParameterInfo PI in this.Parameters)
						PI.DescriptionAsMarkdown = (PI.Parameter.Descriptions.Find(Language)?.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

					foreach (RoleInfo RI in this.Roles)
						RI.DescriptionAsMarkdown = (RI.Role.Descriptions.Find(Language)?.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

					await this.PopulateHumanReadableText();

					if (!(this.languageOptions is null))
						await this.PopulateParameters(this.languageOptions, this.parameterOptions, this.additionalCommands, null);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

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

				await this.SetContract(Contract);

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
		/// <param name="Languages">StackPanel to language options</param>
		/// <param name="Parameters">StackPanel to populate with parameters</param>
		/// <param name="AdditionalCommands">StackPanel to populate with additional commends.</param>
		/// <param name="PresetValues">Optional preset values. Can be null.</param>
		public async Task PopulateParameters(StackPanel Languages, StackPanel Parameters, StackPanel AdditionalCommands,
			Dictionary<CaseInsensitiveString, object> PresetValues)
		{
			List<ParameterInfo> ParameterList = new();
			ParameterInfo ParameterInfo;

			this.languageOptions = Languages;
			this.languageOptions.DataContext = this;

			this.parameterOptions = Parameters;
			this.parameterOptions.Children.Clear();
			this.parameterOptions.DataContext = this;
			this.parametersByName.Clear();

			foreach (Parameter Parameter in this.contract.Parameters)
			{
				if (Parameter is BooleanParameter BP)
				{
					CheckBox CheckBox = new()
					{
						Tag = Parameter.Name,
						IsChecked = BP.Value.HasValue && BP.Value.Value,
						VerticalContentAlignment = VerticalAlignment.Center,
						Content = Parameter.GetLabel(),
						ToolTip = await Parameter.ToSimpleXAML(this.Language, this.contract),
						Margin = new Thickness(0, 10, 0, 0)
					};

					if ((PresetValues?.TryGetValue(Parameter.Name, out object PresetValue) ?? false) && PresetValue is bool b)
					{
						BP.Value = b;
						CheckBox.IsChecked = b;
					}

					CheckBox.Checked += this.Parameter_CheckedChanged;
					CheckBox.Unchecked += this.Parameter_CheckedChanged;

					this.parametersByName[Parameter.Name] = ParameterInfo = new BooleanParameterInfo(this.contract, BP, CheckBox, null, this.parameters);

					Parameters.Children.Add(CheckBox);
				}
				else
				{
					Label Label = new()
					{
						Content = Parameter.GetLabel(),
						Margin = new Thickness(0, 10, 0, 0)
					};

					TextBox TextBox = new()
					{
						Tag = Parameter.Name,
						Text = Parameter.ObjectValue?.ToString(),
						ToolTip = await Parameter.ToSimpleXAML(this.Language, this.contract)
					};

					if (PresetValues?.TryGetValue(Parameter.Name, out object PresetValue) ?? false)
						TextBox.Text = PresetValue?.ToString() ?? string.Empty;
					else
						PresetValue = null;

					TextBox.TextChanged += this.Parameter_TextChanged;

					if (Parameter is NumericalParameter NP)
					{
						if (PresetValue is decimal d)
							NP.Value = d;

						this.parametersByName[Parameter.Name] = ParameterInfo = new NumericalParameterInfo(this.contract, NP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is StringParameter SP)
					{
						if (PresetValue is string s)
							SP.Value = s;

						this.parametersByName[Parameter.Name] = ParameterInfo = new StringParameterInfo(this.contract, SP, TextBox,
							null, null, null, null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DateParameter DP)
					{
						if (PresetValue is DateTime TP)
							DP.Value = TP;

						this.parametersByName[Parameter.Name] = ParameterInfo = new DateParameterInfo(this.contract, DP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DateTimeParameter DTP)
					{
						if (PresetValue is DateTime TP)
							DTP.Value = TP;

						this.parametersByName[Parameter.Name] = ParameterInfo = new DateTimeParameterInfo(this.contract, DTP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is TimeParameter TP)
					{
						if (PresetValue is TimeSpan TS)
							TP.Value = TS;

						this.parametersByName[Parameter.Name] = ParameterInfo = new TimeParameterInfo(this.contract, TP, TextBox,
							null, null, null, null, null, this.parameters);
					}
					else if (Parameter is DurationParameter DrP)
					{
						if (PresetValue is Waher.Content.Duration D)
							DrP.Value = D;

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
			await this.PopulateHumanReadableText();

			this.additionalCommands = AdditionalCommands;
			this.additionalCommands.DataContext = this;
			this.additionalCommands.Visibility = Visibility.Visible;
			this.additionalCommands.InvalidateVisual();
		}

		private async void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not CheckBox CheckBox || !this.parametersByName.TryGetValue(CheckBox.Tag.ToString(), out ParameterInfo ParameterInfo))
					return;

				ParameterInfo.Value = CheckBox.IsChecked;

				await this.ValidateParameters();
				await this.PopulateHumanReadableText();
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
				if (sender is not TextBox TextBox || !this.parametersByName.TryGetValue(TextBox.Tag.ToString(), out ParameterInfo ParameterInfo))
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
					else if (ParameterInfo.Parameter is not CalcParameter)
						ParameterInfo.Value = TextBox.Text;

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}

				await this.PopulateHumanReadableText();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task ValidateParameters()
		{
			Variables Variables = new();
			bool Ok = true;

			Variables["Duration"] = this.contract.Duration;

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

			if (XamlReader.Parse(await this.contract.ToXAML(this.Language)) is StackPanel Panel)
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
			get => this.roles.Value;
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
		public bool ContractLoaded => this.contract is not null;

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

				Contract Contract = await this.contracts.SignContractAsync(this.contract, Role, false);

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
