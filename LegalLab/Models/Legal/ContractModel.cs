using LegalLab.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Settings;
using Waher.Script;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contract model
	/// </summary>
	public class ContractModel : Model
	{
		private readonly Property<bool> parametersOk;
		private readonly Property<GenInfo[]> generalInformation;
		private readonly Property<RoleInfo[]> roles;
		private readonly Property<GenInfo[]> parts;
		private readonly Property<ParameterInfo[]> parameters;
		private readonly Property<ClientSignatureInfo[]> clientSignatures;
		private readonly Property<ServerSignatureInfo[]> serverSignatures;
		private readonly Property<bool> hasId;
		private readonly Property<string> uri;
		private readonly Property<string> qrCodeUri;
		private readonly Property<string> machineReadable;
		private readonly Property<string> templateName;
		private readonly Property<string> contractId;

		private readonly Command proposeTemplate;
		private readonly Command createContract;

		private readonly Dictionary<string, ParameterInfo> parametersByName = new Dictionary<string, ParameterInfo>();
		private readonly ContractsClient contracts;
		private readonly LegalModel legalModel;
		private Contract contract;
		private StackPanel humanReadableText = null;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="Contract">Contract</param>
		/// <param name="LegalModel">Legal Model</param>
		public ContractModel(ContractsClient Contracts, Contract Contract, LegalModel LegalModel)
		{
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.generalInformation = new Property<GenInfo[]>(nameof(this.GeneralInformation), new GenInfo[0], this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), new RoleInfo[0], this);
			this.parts = new Property<GenInfo[]>(nameof(this.Parts), new GenInfo[0], this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), new ParameterInfo[0], this);
			this.clientSignatures = new Property<ClientSignatureInfo[]>(nameof(this.ClientSignatures), new ClientSignatureInfo[0], this);
			this.serverSignatures = new Property<ServerSignatureInfo[]>(nameof(this.ServerSignatures), new ServerSignatureInfo[0], this);
			this.hasId = new Property<bool>(nameof(this.HasId), false, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);
			this.qrCodeUri = new Property<string>(nameof(this.QrCodeUri), string.Empty, this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.templateName = new Property<string>(nameof(this.TemplateName), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), Contract.ContractId, this);

			this.proposeTemplate = new Command(this.CanExecuteProposeTemplate, this.ExecuteProposeTemplate);
			this.createContract = new Command(this.CanExecuteCreateContract, this.ExecuteCreateContract);

			this.legalModel = LegalModel;
			this.contracts = Contracts;
			this.SetContract(Contract);

			this.TemplateName = Contract.ContractId;

			StringBuilder sb = new StringBuilder();
			Contract.NormalizeXml(Contract.ForMachines, sb, ContractsClient.NamespaceSmartContracts);

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(sb.ToString());
			sb.Clear();

			XmlWriterSettings Settings = XML.WriterSettings(true, true);
			using XmlWriter w = XmlWriter.Create(sb, Settings);

			this.contract.ForMachines.WriteTo(w);
			w.Flush();
			this.MachineReadable = sb.ToString().Replace("&#xD;\n", "\n").Replace("\n\t", "\n").Replace("\t", "    ");
		}

		private void SetContract(Contract Contract)
		{
			this.contract = Contract;

			this.ContractId = Contract.ContractId;
			this.HasId = !string.IsNullOrEmpty(Contract.ContractId);
			this.Uri = ContractsClient.ContractIdUriString(Contract.ContractId);
			this.QrCodeUri = "https://" + this.contracts.Client.Domain + "/QR/" + this.Uri;

			List<GenInfo> Info = new List<GenInfo>()
			{
				new GenInfo("Created:", this.contract.Created.ToString(CultureInfo.CurrentUICulture))
			};

			if (this.contract.Updated > DateTime.MinValue)
				Info.Add(new GenInfo("Updated:", this.contract.Updated.ToString(CultureInfo.CurrentUICulture)));

			Info.Add(new GenInfo("State:", this.contract.State.ToString()));
			Info.Add(new GenInfo("Visibility:", this.contract.Visibility.ToString()));
			Info.Add(new GenInfo("Duration:", this.contract.Duration.ToString()));
			Info.Add(new GenInfo("From:", this.contract.From.ToString(CultureInfo.CurrentUICulture)));
			Info.Add(new GenInfo("To:", this.contract.To.ToString(CultureInfo.CurrentUICulture)));
			Info.Add(new GenInfo("Archiving Optional:", this.contract.ArchiveOptional.ToString()));
			Info.Add(new GenInfo("Archiving Required:", this.contract.ArchiveRequired.ToString()));
			Info.Add(new GenInfo("Can act as a template:", this.contract.CanActAsTemplate.ToYesNo()));
			Info.Add(new GenInfo("Provider:", this.contract.Provider));
			Info.Add(new GenInfo("Parts:", this.contract.PartsMode.ToString()));

			if (this.contract.SignAfter.HasValue)
				Info.Add(new GenInfo("Sign after:", this.contract.SignAfter.Value.ToStringTZ()));

			if (this.contract.SignBefore.HasValue)
				Info.Add(new GenInfo("Sign before:", this.contract.SignBefore.Value.ToStringTZ()));

			if (!string.IsNullOrEmpty(this.contract.TemplateId))
				Info.Add(new GenInfo("Template ID:", this.contract.TemplateId));

			this.GeneralInformation = Info.ToArray();
			Info.Clear();

			if (!(this.contract.Parts is null))
			{
				foreach (Part Part in this.contract.Parts)
					Info.Add(new GenInfo(Part.LegalId, Part.Role));
			}

			this.Parts = Info.ToArray();

			List<RoleInfo> Roles = new List<RoleInfo>();

			if (!(this.contract.Roles is null))
			{
				foreach (Role Role in this.contract.Roles)
					Roles.Add(new RoleInfo(this.contract, Role));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> Parameters = new List<ParameterInfo>();

			if (!(this.contract.Parameters is null))
			{
				foreach (Parameter Parameter in this.contract.Parameters)
				{
					if (this.parametersByName.TryGetValue(Parameter.Name, out ParameterInfo ParameterInfo))
						Parameters.Add(ParameterInfo);
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

		/// <summary>
		/// If all parameters are OK or not
		/// </summary>
		public bool ParametersOk
		{
			get => this.parametersOk.Value;
			set
			{
				this.parametersOk.Value = value;
				this.proposeTemplate.RaiseCanExecuteChanged();
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
		/// Template Name
		/// </summary>
		public string TemplateName
		{
			get => this.templateName.Value;
			set
			{
				this.templateName.Value = value;
				this.proposeTemplate.RaiseCanExecuteChanged();
			}
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
		/// Propose template command
		/// </summary>
		public ICommand ProposeTemplate => this.proposeTemplate;

		/// <summary>
		/// If the propose template command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteProposeTemplate()
		{
			return (this.ParametersOk || this.contract.PartsMode == ContractParts.TemplateOnly) && !string.IsNullOrEmpty(this.TemplateName);
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
		public void PopulateParameters(StackPanel Parameters, StackPanel AdditionalCommands)
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
						Content = GetLabel(Parameter),
						ToolTip = XamlReader.Parse(Parameter.ToXAML(this.contract.DefaultLanguage, this.contract)),
						Margin = new Thickness(0, 10, 0, 0)
					};

					CheckBox.Checked += Parameter_CheckedChanged;
					CheckBox.Unchecked += Parameter_CheckedChanged;

					this.parametersByName[Parameter.Name] = ParameterInfo = new ParameterInfo(this.contract, Parameter, CheckBox);

					Parameters.Children.Add(CheckBox);
				}
				else
				{
					Label Label = new Label()
					{
						Content = GetLabel(Parameter),
						Margin = new Thickness(0, 10, 0, 0)
					};

					TextBox TextBox = new TextBox()
					{
						Tag = Parameter.Name,
						Text = Parameter.ObjectValue?.ToString(),
						ToolTip = XamlReader.Parse(Parameter.ToXAML(this.contract.DefaultLanguage, this.contract))
					};

					TextBox.TextChanged += Parameter_TextChanged;

					this.parametersByName[Parameter.Name] = ParameterInfo = new ParameterInfo(this.contract, Parameter, TextBox);

					Parameters.Children.Add(Label);
					Parameters.Children.Add(TextBox);
				}

				ParameterList.Add(ParameterInfo);
			}

			this.Parameters = ParameterList.ToArray();

			this.ValidateParameters();
			PopulateHumanReadableText();

			AdditionalCommands.DataContext = this;
			AdditionalCommands.Visibility = Visibility.Visible;
			AdditionalCommands.InvalidateVisual();
		}

		private static string GetLabel(Parameter P)
		{
			if (string.IsNullOrEmpty(P.Guide))
				return P.Name + ":";
			else
				return P.Name + " (" + P.Guide + "):";
		}

		private void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!(sender is CheckBox CheckBox) || !this.parametersByName.TryGetValue(CheckBox.Tag.ToString(), out ParameterInfo ParameterInfo))
				return;

			ParameterInfo.Value = CheckBox.IsChecked;

			this.ValidateParameters();
			PopulateHumanReadableText();
		}

		private void Parameter_TextChanged(object sender, RoutedEventArgs e)
		{
			if (!(sender is TextBox TextBox) || !this.parametersByName.TryGetValue(TextBox.Tag.ToString(), out ParameterInfo ParameterInfo))
				return;

			try
			{
				if (ParameterInfo.Parameter is NumericalParameter && double.TryParse(TextBox.Text, out double d))
					ParameterInfo.Value = d;
				else if (ParameterInfo.Parameter is BooleanParameter && bool.TryParse(TextBox.Text, out bool b))
					ParameterInfo.Value = b;
				else
					ParameterInfo.Value = TextBox.Text;

				TextBox.Background = null;
				this.ValidateParameters();
			}
			catch (Exception)
			{
				TextBox.Background = Brushes.Salmon;
			}

			PopulateHumanReadableText();
		}

		private void ValidateParameters()
		{
			Variables Variables = new Variables();
			bool Ok = true;

			foreach (ParameterInfo P in this.parametersByName.Values)
				P.Parameter.Populate(Variables);

			foreach (ParameterInfo P in this.parametersByName.Values)
			{
				if (P.Parameter.IsParameterValid(Variables))
					P.Control.Background = null;
				else
				{
					P.Control.Background = Brushes.Salmon;
					Ok = false;
				}
			}

			this.ParametersOk = Ok;
		}

		private void PopulateHumanReadableText()
		{
			if (this.humanReadableText is null)
				return;

			this.humanReadableText.Children.Clear();

			if (XamlReader.Parse(this.contract.ToXAML(this.contract.DefaultLanguage)) is StackPanel Panel)
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
		public GenInfo[] Parts
		{
			get => this.parts.Value;
			set => this.parts.Value = value;
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
		public void PopulateContract(StackPanel ContractLayout, StackPanel HumanReadableText)
		{
			this.humanReadableText = HumanReadableText;
			this.PopulateHumanReadableText();

			ContractLayout.DataContext = this;
			ContractLayout.Visibility = Visibility.Visible;

			// TODO: Attachments
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
		public async void ExecuteCreateContract()
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
					this.contract.Visibility, this.contract.PartsMode, this.contract.Duration, this.contract.ArchiveRequired,
					this.contract.ArchiveOptional, this.contract.SignAfter, this.contract.SignBefore, false);

				this.SetContract(Contract);

				MainWindow.SuccessBox("Contract successfully created.");
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

	}
}
