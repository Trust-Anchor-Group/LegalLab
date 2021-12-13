using LegalLab.Extensions;
using LegalLab.Models.Legal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Contract model
	/// </summary>
	public class DesignModel : Model
	{
		private readonly Property<Waher.Content.Duration> archiveOptional;
		private readonly Property<Waher.Content.Duration> archiveRequired;
		private readonly Property<Waher.Content.Duration> duration;
		private readonly Property<ContractVisibility> visibility;
		private readonly Property<ContractParts> partsMode;
		private readonly Property<string> defaultLanguage;
		private readonly Property<DateTime?> signBefore;
		private readonly Property<DateTime?> signAfter;
		private readonly Property<string> contractId;
		private readonly Property<bool> parametersOk;
		private readonly Property<RoleInfo[]> roles;
		private readonly Property<PartInfo[]> parts;
		private readonly Property<ParameterInfo[]> parameters;
		private readonly Property<string> machineReadable;
		private readonly Property<string> forMachinesLocalName;
		private readonly Property<string> forMachinesNamespace;
		private readonly Property<XmlElement> forMachines;

		private readonly Command addRole;
		private readonly Command addPart;

		private readonly Dictionary<string, ParameterInfo> parametersByName = new Dictionary<string, ParameterInfo>();
		private StackPanel humanReadableText = null;
		private Contract contract;

		/// <summary>
		/// Design model
		/// </summary>
		public DesignModel()
		{
			this.visibility = new Property<ContractVisibility>(nameof(this.Visibility), ContractVisibility.Public, this);
			this.partsMode = new Property<ContractParts>(nameof(this.PartsMode), ContractParts.TemplateOnly, this);
			this.archiveOptional = new Property<Waher.Content.Duration>(nameof(this.ArchiveOptional), Waher.Content.Duration.Zero, this);
			this.archiveRequired = new Property<Waher.Content.Duration>(nameof(this.ArchiveRequired), Waher.Content.Duration.Zero, this);
			this.duration = new Property<Waher.Content.Duration>(nameof(this.Duration), Waher.Content.Duration.Zero, this);
			this.defaultLanguage = new Property<string>(nameof(this.DefaultLanguage), "en", this);
			this.signBefore = new Property<DateTime?>(nameof(this.SignBefore), null, this);
			this.signAfter = new Property<DateTime?>(nameof(this.SignAfter), null, this);
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), new RoleInfo[0], this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), new PartInfo[0], this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), new ParameterInfo[0], this);
			this.machineReadable = new Property<string>(nameof(this.MachineReadable), string.Empty, this);
			this.forMachines = new Property<XmlElement>(nameof(this.ForMachines), null, this);
			this.forMachinesLocalName = new Property<string>(nameof(this.ForMachinesLocalName), string.Empty, this);
			this.forMachinesNamespace = new Property<string>(nameof(this.ForMachinesNamespace), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), string.Empty, this);

			this.addRole = new Command(this.ExecuteAddRole);
			this.addPart = new Command(this.ExecuteAddPart);

			this.GenerateContract();
		}

		private void GenerateContract()
		{
			this.SetContract(new Contract()
			{
				ArchiveOptional = this.ArchiveOptional,
				ArchiveRequired = this.ArchiveRequired,
				Attachments = new Attachment[0],
				CanActAsTemplate = true,
				ClientSignatures = new ClientSignature[0],
				ContractId = this.ContractId,
				Duration = this.Duration,
				ForHumans = new HumanReadableText[0],
				ForMachines = this.ForMachines,
				Parameters = new Parameter[0],
				Parts = new Part[0],
				PartsMode = ContractParts.TemplateOnly,
				Roles = new Role[0],
				ServerSignature = null,
				SignAfter = this.SignAfter,
				SignBefore = this.SignBefore,
				TemplateId = string.Empty,
				Visibility = ContractVisibility.PublicSearchable
			});
		}

		public void SetContract(Contract Contract)
		{
			this.contract = Contract;
			this.ContractId = Contract.ContractId;
			this.ArchiveOptional = Contract.ArchiveOptional;
			this.ArchiveRequired = Contract.ArchiveRequired;
			this.Duration = Contract.Duration;
			this.DefaultLanguage = Contract.DefaultLanguage;
			this.SignBefore = Contract.SignBefore;
			this.SignAfter = Contract.SignAfter;

			List<PartInfo> Parts = new List<PartInfo>();

			if (!(Contract.Parts is null))
			{
				foreach (Part Part in Contract.Parts)
					Parts.Add(new PartInfo(Part.LegalId, Part.Role, this));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new List<RoleInfo>();

			if (!(Contract.Roles is null))
			{
				foreach (Role Role in Contract.Roles)
					Roles.Add(new RoleInfo(this, Role));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> Parameters = new List<ParameterInfo>();

			if (!(Contract.Parameters is null))
			{
				foreach (Parameter Parameter in Contract.Parameters)
				{
					if (this.parametersByName.TryGetValue(Parameter.Name, out ParameterInfo ParameterInfo))
						Parameters.Add(ParameterInfo);
				}
			}

			this.Parameters = Parameters.ToArray();

			if (Contract.ForMachines is null)
			{
				this.MachineReadable = string.Empty;
				this.ForMachines = null;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				Contract.NormalizeXml(Contract.ForMachines, sb, ContractsClient.NamespaceSmartContracts);

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(sb.ToString());
				sb.Clear();

				XmlWriterSettings Settings = XML.WriterSettings(true, true);
				using XmlWriter w = XmlWriter.Create(sb, Settings);

				Contract.ForMachines.WriteTo(w);
				w.Flush();
				this.MachineReadable = sb.ToString().Replace("&#xD;\n", "\n").Replace("\n\t", "\n").Replace("\t", "    ");
				this.ForMachines = Doc.DocumentElement;
			}
		}

		/// <summary>
		/// Current contract
		/// </summary>
		public Contract Contract => this.contract;

		/// <inheritdoc/>
		public override Task Start()
		{
			MainWindow.currentInstance.DesignTab.DataContext = this;

			return base.Start();
		}

		/// <summary>
		/// Contract visibility
		/// </summary>
		public ContractVisibility Visibility
		{
			get => this.visibility.Value;
			set => this.visibility.Value = value;
		}

		/// <summary>
		/// Contract visibilities.
		/// </summary>
		public string[] Visibilities => Enum.GetNames(typeof(ContractVisibility));

		/// <summary>
		/// Contract parts mode
		/// </summary>
		public ContractParts PartsMode
		{
			get => this.partsMode.Value;
			set => this.partsMode.Value = value;
		}

		/// <summary>
		/// Contract parts modes
		/// </summary>
		public string[] PartsModes => Enum.GetNames(typeof(ContractParts));

		/// <summary>
		/// Optional archiving time
		/// </summary>
		public Waher.Content.Duration ArchiveOptional
		{
			get => this.archiveOptional.Value;
			set => this.archiveOptional.Value = value;
		}

		/// <summary>
		/// Required archiving time
		/// </summary>
		public Waher.Content.Duration ArchiveRequired
		{
			get => this.archiveRequired.Value;
			set => this.archiveRequired.Value = value;
		}

		/// <summary>
		/// Duration of contract, once signed
		/// </summary>
		public Waher.Content.Duration Duration
		{
			get => this.duration.Value;
			set => this.duration.Value = value;
		}

		/// <summary>
		/// Default language of contract
		/// </summary>
		public string DefaultLanguage
		{
			get => this.defaultLanguage.Value;
			set => this.defaultLanguage.Value = value;
		}

		/// <summary>
		/// If contract must be signed before a spceific time.
		/// </summary>
		public DateTime? SignBefore
		{
			get => this.signBefore.Value;
			set => this.signBefore.Value = value;
		}

		/// <summary>
		/// If contract must be signed after a spceific time.
		/// </summary>
		public DateTime? SignAfter
		{
			get => this.signAfter.Value;
			set => this.signAfter.Value = value;
		}

		/// <summary>
		/// If all parameters are OK or not
		/// </summary>
		public bool ParametersOk
		{
			get => this.parametersOk.Value;
			set => this.parametersOk.Value = value;
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
		/// Parsed Machine-Readable XML in the contract
		/// </summary>
		public XmlElement ForMachines
		{
			get => this.forMachines.Value;
			set
			{
				this.forMachines.Value = value;
				this.forMachinesLocalName.Value = value?.LocalName;
				this.forMachinesNamespace.Value = value?.NamespaceURI;
			}
		}

		/// <summary>
		/// Local Name of top element of Machine-Readable XML in the contract
		/// </summary>
		public string ForMachinesLocalName
		{
			get => this.forMachinesLocalName.Value;
		}

		/// <summary>
		/// Namespace of top element of Machine-Readable XML in the contract
		/// </summary>
		public string ForMachinesNamespace
		{
			get => this.forMachinesNamespace.Value;
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
						ToolTip = Parameter.ToSimpleXAML(this.DefaultLanguage, this.contract),
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
						ToolTip = Parameter.ToSimpleXAML(this.DefaultLanguage, this.contract)
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
			AdditionalCommands.Visibility = System.Windows.Visibility.Visible;
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

			if (XamlReader.Parse(this.contract.ToXAML(this.DefaultLanguage)) is StackPanel Panel)
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
		/// Displays the contents of the contract
		/// </summary>
		/// <param name="ContractLayout">Where to layout the contract</param>
		/// <param name="HumanReadableText">Control where human-readable content is placed</param>
		public void PopulateContract(StackPanel ContractLayout, StackPanel HumanReadableText)
		{
			this.humanReadableText = HumanReadableText;
			this.PopulateHumanReadableText();

			ContractLayout.DataContext = this;
			ContractLayout.Visibility = System.Windows.Visibility.Visible;
		}

		/// <summary>
		/// Command for adding a role to the design.
		/// </summary>
		public ICommand AddRole => this.addRole;

		/// <summary>
		/// Adds a role to the design.
		/// </summary>
		public void ExecuteAddRole()
		{
			RoleInfo[] Roles = this.Roles;
			int c = Roles.Length;

			Array.Resize<RoleInfo>(ref Roles, c + 1);
			Roles[c] = new RoleInfo(this, new Role()
			{
				Name = "New Role",
				Descriptions = new HumanReadableText[] { "Enter role description as *Markdown*".ToHumanReadableText() },
				MinCount = 1,
				MaxCount = 1,
				CanRevoke = false
			});

			this.Roles = Roles;
		}

		/// <summary>
		/// Removes a role from the design
		/// </summary>
		/// <param name="Role"></param>
		public void RemoveRole(RoleInfo Role)
		{
			RoleInfo[] Roles = this.Roles;
			int i = Array.IndexOf<RoleInfo>(Roles, Role);
			if (i < 0)
				return;

			int c = Roles.Length;

			if (i < c - 1)
				Array.Copy(Roles, i + 1, Roles, i, c - i - 1);

			Array.Resize<RoleInfo>(ref Roles, c - 1);

			this.Roles = Roles;
		}

		/// <summary>
		/// Command for adding a part to the design.
		/// </summary>
		public ICommand AddPart => this.addPart;

		/// <summary>
		/// Adds a part to the design.
		/// </summary>
		public void ExecuteAddPart()
		{
			PartInfo[] Parts = this.Parts;
			int c = Parts.Length;

			Array.Resize<PartInfo>(ref Parts, c + 1);
			Parts[c] = new PartInfo(string.Empty, string.Empty, this);

			this.Parts = Parts;
		}

		/// <summary>
		/// Removes a part from the design
		/// </summary>
		/// <param name="Part"></param>
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

	}
}
