using LegalLab.Converters;
using LegalLab.Dialogs.AddLanguage;
using LegalLab.Extensions;
using LegalLab.Items;
using LegalLab.Models.Legal;
using LegalLab.Models.Legal.Items;
using LegalLab.Models.Legal.Items.Parameters;
using LegalLab.Models.Network;
using LegalLab.Models.Standards;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using TAG.Content.Microsoft;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;
using Waher.Script;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Contract model
	/// </summary>
	[Singleton]
	public class DesignModel : ConnectionSensitiveModel, IPartsModel, ITranslatable
	{
		private const string defaultParameterDescription = "Enter parameter description as **Markdown**";

		private readonly Property<Waher.Content.Duration?> archiveOptional;
		private readonly Property<Waher.Content.Duration?> archiveRequired;
		private readonly Property<Waher.Content.Duration?> duration;
		private readonly Property<ContractVisibility> visibility;
		private readonly Property<ContractParts> partsMode;
		private readonly Property<string> language;
		private readonly Property<Iso__639_1.Record[]> languages;
		private readonly Property<DateTime?> signBefore;
		private readonly Property<DateTime?> signAfter;
		private readonly Property<string> contractId;
		private readonly Property<bool> parametersOk;
		private readonly Property<RoleInfo[]> roles;
		private readonly Property<PartInfo[]> parts;
		private readonly Property<ParameterInfo[]> parameters;
		private readonly Property<RoleParameterInfo[]> roleParameters;
		private readonly DelayedActionProperty<string> machineReadable;
		private readonly Property<string> forMachinesLocalName;
		private readonly Property<string> forMachinesNamespace;
		private readonly Property<XmlElement> forMachines;
		private readonly DelayedActionProperty<string> humanReadableMarkdown;
		private readonly Property<object> humanReadable;
		private readonly PersistedProperty<string> openAiKey;

		private readonly Command addRole;
		private readonly Command addPart;
		private readonly Command addNumericParameter;
		private readonly Command addStringParameter;
		private readonly Command addBooleanParameter;
		private readonly Command addDateParameter;
		private readonly Command addDateTimeParameter;
		private readonly Command addTimeParameter;
		private readonly Command addDurationParameter;
		private readonly Command addCalcParameter;
		private readonly Command addRoleReference;
		private readonly Command @new;
		private readonly Command load;
		private readonly Command import;
		private readonly Command save;
		private readonly Command propose;
		private readonly Command addLanguage;
		private readonly Command removeLanguage;

		private Contract contract;
		private string lastLanguage = string.Empty;

		/// <summary>
		/// Design model
		/// </summary>
		public DesignModel()
		{
			this.visibility = new Property<ContractVisibility>(nameof(this.Visibility), ContractVisibility.Public, this);
			this.partsMode = new Property<ContractParts>(nameof(this.PartsMode), ContractParts.TemplateOnly, this);
			this.archiveOptional = new Property<Waher.Content.Duration?>(nameof(this.ArchiveOptional), Waher.Content.Duration.Zero, this);
			this.archiveRequired = new Property<Waher.Content.Duration?>(nameof(this.ArchiveRequired), Waher.Content.Duration.Zero, this);
			this.duration = new Property<Waher.Content.Duration?>(nameof(this.Duration), Waher.Content.Duration.Zero, this);
			this.language = new Property<string>(nameof(this.Language), "en", this);
			this.languages = new Property<Iso__639_1.Record[]>(nameof(this.Languages), Array.Empty<Iso__639_1.Record>(), this);
			this.signBefore = new Property<DateTime?>(nameof(this.SignBefore), null, this);
			this.signAfter = new Property<DateTime?>(nameof(this.SignAfter), null, this);
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), Array.Empty<RoleInfo>(), this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), Array.Empty<PartInfo>(), this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), Array.Empty<ParameterInfo>(), this);
			this.roleParameters = new Property<RoleParameterInfo[]>(nameof(this.RoleParameters), Array.Empty<RoleParameterInfo>(), this);
			this.machineReadable = new DelayedActionProperty<string>(nameof(this.MachineReadable), TimeSpan.FromSeconds(1), string.Empty, this);
			this.forMachines = new Property<XmlElement>(nameof(this.ForMachines), null, this);
			this.forMachinesLocalName = new Property<string>(nameof(this.ForMachinesLocalName), string.Empty, this);
			this.forMachinesNamespace = new Property<string>(nameof(this.ForMachinesNamespace), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), string.Empty, this);
			this.humanReadableMarkdown = new DelayedActionProperty<string>(nameof(this.HumanReadableMarkdown), TimeSpan.FromSeconds(1), true, string.Empty, this);
			this.humanReadable = new Property<object>(nameof(this.HumanReadable), null, this);

			this.roles.PropertyChanged += this.Roles_PropertyChanged;
			this.parameters.PropertyChanged += this.Parameters_PropertyChanged;
			this.roleParameters.PropertyChanged += this.Parameters_PropertyChanged;
			this.parts.PropertyChanged += this.Parts_PropertyChanged;

			this.Add(this.openAiKey = new PersistedProperty<string>("Design", nameof(this.OpenAiKey), true, string.Empty, this));

			this.machineReadable.OnAction += this.NormalizeMachineReadableXml;
			this.humanReadableMarkdown.OnAction += this.RenderHumanReadableMarkdown;

			this.addRole = new Command(this.ExecuteAddRole);
			this.addPart = new Command(this.ExecuteAddPart);
			this.addNumericParameter = new Command(this.ExecuteAddNumericParameter);
			this.addStringParameter = new Command(this.ExecuteAddStringParameter);
			this.addBooleanParameter = new Command(this.ExecuteAddBooleanParameter);
			this.addDateParameter = new Command(this.ExecuteAddDateParameter);
			this.addDateTimeParameter = new Command(this.ExecuteAddDateTimeParameter);
			this.addTimeParameter = new Command(this.ExecuteAddTimeParameter);
			this.addDurationParameter = new Command(this.ExecuteAddDurationParameter);
			this.addCalcParameter = new Command(this.ExecuteAddCalcParameter);
			this.addRoleReference = new Command(this.ExecuteAddRoleReference);
			this.@new = new Command(this.ExecuteNewContract);
			this.load = new Command(this.ExecuteLoadContract);
			this.import = new Command(this.ExecuteImportContract);
			this.save = new Command(this.ExecuteSaveContract);
			this.propose = new Command(this.CanExecuteProposeContract, this.ExecuteProposeContract);
			this.addLanguage = new Command(this.ExecuteAddLanguage);
			this.removeLanguage = new Command(this.CanExecuteRemoveLanguage, this.ExecuteRemoveLanguage);

			this.GenerateContract().Wait();
		}

		private async Task GenerateContract()
		{
			await this.SetContract(new Contract()
			{
				ArchiveOptional = this.ArchiveOptional,
				ArchiveRequired = this.ArchiveRequired,
				Attachments = Array.Empty<Attachment>(),
				CanActAsTemplate = true,
				ClientSignatures = Array.Empty<ClientSignature>(),
				ContractId = this.ContractId,
				Duration = this.Duration,
				ForHumans = Array.Empty<HumanReadableText>(),
				ForMachines = this.ForMachines,
				Parameters = Array.Empty<Parameter>(),
				Parts = Array.Empty<Part>(),
				PartsMode = ContractParts.TemplateOnly,
				Roles = Array.Empty<Role>(),
				ServerSignature = null,
				SignAfter = this.SignAfter,
				SignBefore = this.SignBefore,
				TemplateId = string.Empty,
				Visibility = ContractVisibility.PublicSearchable
			});
		}

		public async Task SetContract(Contract Contract)
		{
			this.contract = Contract;
			this.ContractId = Contract.ContractId;
			this.ArchiveOptional = Contract.ArchiveOptional;
			this.ArchiveRequired = Contract.ArchiveRequired;
			this.Duration = Contract.Duration;
			this.SignBefore = Contract.SignBefore;
			this.SignAfter = Contract.SignAfter;
			this.Visibility = Contract.Visibility;
			this.PartsMode = Contract.PartsMode;

			List<PartInfo> Parts = new();

			if (Contract.Parts is not null)
			{
				foreach (Part Part in Contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new();

			if (Contract.Roles is not null)
			{
				foreach (Role Role in Contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> ParameterList = new();
			List<RoleParameterInfo> RoleParameterList = new();
			ParameterInfo ParameterInfo;

			foreach (Parameter Parameter in this.contract.Parameters)
			{
				if (Parameter is BooleanParameter BP)
					ParameterInfo = this.GetParameterInfo(BP);
				else if (Parameter is NumericalParameter NP)
					ParameterInfo = this.GetParameterInfo(NP);
				else if (Parameter is StringParameter SP)
					ParameterInfo = this.GetParameterInfo(SP);
				else if (Parameter is DateParameter DP)
					ParameterInfo = this.GetParameterInfo(DP);
				else if (Parameter is DateTimeParameter DTP)
					ParameterInfo = this.GetParameterInfo(DTP);
				else if (Parameter is TimeParameter TP)
					ParameterInfo = this.GetParameterInfo(TP);
				else if (Parameter is DurationParameter DrP)
					ParameterInfo = this.GetParameterInfo(DrP);
				else if (Parameter is CalcParameter CP)
					ParameterInfo = this.GetParameterInfo(CP);
				else if (Parameter is RoleParameter RP)
				{
					RoleParameterList.Add(this.GetParameterInfo(RP));
					continue;
				}
				else
					continue;

				ParameterList.Add(ParameterInfo);
			}

			this.Parameters = ParameterList.ToArray();
			this.RoleParameters = RoleParameterList.ToArray();

			await this.ValidateParameters();

			(string s, XmlElement E) = Contract.ForMachines.ToPrettyXml();

			this.MachineReadable = s.Replace("\n\t", "\n");
			this.ForMachines = E;

			this.Languages = Contract.GetLanguages().ToIso639_1();
			this.Language = Contract.DefaultLanguage;

			if (string.IsNullOrEmpty(this.Language) && this.Languages.Length == 0)
			{
				this.Languages = new string[] { "en" }.ToIso639_1();
				this.Language = "en";
			}

			this.HumanReadableMarkdown = Contract.ToMarkdown(this.Language, MarkdownType.ForEditing)?.Trim() ?? string.Empty;
		}

		/// <summary>
		/// Current contract
		/// </summary>
		public Contract Contract => this.contract;

		/// <inheritdoc/>
		public override async Task Start()
		{
			await base.Start();

			MainWindow.currentInstance.DesignTab.DataContext = this;
			MainWindow.currentInstance.DesignTab.OpenAiKey.Password = this.OpenAiKey;
		}

		/// <summary>
		/// Contract visibility
		/// </summary>
		public ContractVisibility Visibility
		{
			get => this.visibility.Value;
			set
			{
				this.visibility.Value = value;
				this.contract.Visibility = value;
			}
		}

		/// <summary>
		/// Contract visibilities.
		/// </summary>
		public static string[] Visibilities => Enum.GetNames(typeof(ContractVisibility));

		/// <summary>
		/// Contract parts mode
		/// </summary>
		public ContractParts PartsMode
		{
			get => this.partsMode.Value;
			set
			{
				this.partsMode.Value = value;
				this.contract.PartsMode = value;

				this.propose.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Contract parts modes
		/// </summary>
		public static string[] PartsModes => Enum.GetNames(typeof(ContractParts));

		/// <summary>
		/// Optional archiving time
		/// </summary>
		public Waher.Content.Duration? ArchiveOptional
		{
			get => this.archiveOptional.Value;
			set
			{
				this.archiveOptional.Value = value;
				this.contract.ArchiveOptional = value;
			}
		}

		/// <summary>
		/// Required archiving time
		/// </summary>
		public Waher.Content.Duration? ArchiveRequired
		{
			get => this.archiveRequired.Value;
			set
			{
				this.archiveRequired.Value = value;
				this.contract.ArchiveRequired = value;
			}
		}

		/// <summary>
		/// Duration of contract, once signed
		/// </summary>
		public Waher.Content.Duration? Duration
		{
			get => this.duration.Value;
			set
			{
				this.duration.Value = value;
				this.contract.Duration = value;
			}
		}

		/// <summary>
		/// Currently selected language.
		/// </summary>
		public string Language
		{
			get => this.language.Value;
			set => this.SetLanguage(value);
		}

		private async void SetLanguage(string Language)
		{
			try
			{
				this.language.Value = Language;
				this.removeLanguage.RaiseCanExecuteChanged();

				if (this.lastLanguage != Language)
				{
					string FromLanguage = this.lastLanguage;

					if (string.IsNullOrEmpty(FromLanguage) || string.IsNullOrEmpty(Language))
					{
						if (!string.IsNullOrEmpty(Language))
						{
							foreach (ParameterInfo PI in this.AllParameterInfos)
								PI.DescriptionAsMarkdown = (PI.Parameter.Descriptions.Find(Language)?.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

							foreach (RoleInfo RI in this.Roles)
								RI.DescriptionAsMarkdown = (RI.Role.Descriptions.Find(Language)?.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

							this.HumanReadableMarkdown = (this.contract.ForHumans.Find(Language)?.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();

							this.lastLanguage = Language;
						}
					}
					else
					{
						this.lastLanguage = Language;

						List<TranslationItem> Items = new();

						foreach (ParameterInfo PI in this.AllParameterInfos)
							Add(Items, PI, FromLanguage, Language);

						foreach (RoleInfo RI in this.Roles)
							Add(Items, RI, FromLanguage, Language);

						Add(Items, this, FromLanguage, Language);

						if (Items.Count > 0)
						{
							List<string> AllTexts = new();

							foreach (TranslationItem Item in Items)
								AllTexts.AddRange(Item.Original);

							string[] AllTranslated = await Translator.Translate(AllTexts.ToArray(), FromLanguage, Language, this.OpenAiKey);

							MainWindow.UpdateGui(() =>
							{
								int i = 0;
								int c;

								foreach (TranslationItem Item in Items)
								{
									string[] Translated = new string[c = Item.Original.Length];

									Array.Copy(AllTranslated, i, Translated, 0, c);
									i += c;

									Item.Object.SetTranslatableTexts(Translated, Language);
								}

								this.removeLanguage.RaiseCanExecuteChanged();

								return Task.CompletedTask;
							});
						}
					}

					this.removeLanguage.RaiseCanExecuteChanged();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private static void Add(List<TranslationItem> Items, ITranslatable Translatable, string From, string To)
		{
			string[] Texts = Translatable.GetTranslatableTexts(To);
			if (Texts is not null)
				Translatable.SetTranslatableTexts(Texts, To);
			else
			{
				Texts = Translatable.GetTranslatableTexts(From);
				if (Texts is not null)
				{
					Items.Add(new TranslationItem()
					{
						Object = Translatable,
						Original = Texts
					});
				}
			}
		}

		private class TranslationItem
		{
			public ITranslatable Object;
			public string[] Original;
		}

		/// <summary>
		/// Gets associated texts to translate.
		/// </summary>
		/// <param name="Language">Language to translate from.</param>
		/// <returns>Array of translatable texts, or null if none.</returns>
		public string[] GetTranslatableTexts(string Language)
		{
			HumanReadableText Text = this.contract.ForHumans.Find(Language);
			if (Text is null)
				return null;
			else
				return new string[] { Text.GenerateMarkdown(this.Contract, MarkdownType.ForEditing) };
		}

		/// <summary>
		/// Sets translated texts.
		/// </summary>
		/// <param name="Texts">Available translated texts.</param>
		/// <param name="Language">Language translated to.</param>
		public void SetTranslatableTexts(string[] Texts, string Language)
		{
			if (Texts.Length > 0)
				this.HumanReadableMarkdown = Texts[0].Trim();
		}

		/// <summary>
		/// Available languages.
		/// </summary>
		public Iso__639_1.Record[] Languages
		{
			get => this.languages.Value;
			set
			{
				this.languages.Value = value;
				this.removeLanguage.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// If contract must be signed before a spceific time.
		/// </summary>
		public DateTime? SignBefore
		{
			get => this.signBefore.Value;
			set
			{
				this.signBefore.Value = value;
				this.contract.SignBefore = value;
			}
		}

		/// <summary>
		/// If contract must be signed after a spceific time.
		/// </summary>
		public DateTime? SignAfter
		{
			get => this.signAfter.Value;
			set
			{
				this.signAfter.Value = value;
				this.contract.SignAfter = value;
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
				this.propose.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Machine-Readable XML in the contract
		/// </summary>
		public string MachineReadable
		{
			get => this.machineReadable.Value;
			set => this.machineReadable.Value = value;
		}

		private void NormalizeMachineReadableXml(object sender, EventArgs e)
		{
			MainWindow.UpdateGui(() =>
			{
				try
				{
					this.contract.ForMachines = this.machineReadable.Value.NormalizeXml();
					this.ForMachines = this.contract.ForMachines;
				}
				catch (Exception ex)
				{
					this.contract.ForMachines = null;
					Log.Critical(ex);
				}

				this.propose.RaiseCanExecuteChanged();

				return Task.CompletedTask;
			});
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
			set
			{
				this.contractId.Value = value;
				this.contract.ContractId = value;

				this.propose.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Human-readable markdown
		/// </summary>
		public string HumanReadableMarkdown
		{
			get => this.humanReadableMarkdown.Value;
			set => this.humanReadableMarkdown.Value = value;
		}

		/// <summary>
		/// Human-readable markdown
		/// </summary>
		public object HumanReadable
		{
			get => this.humanReadable.Value;
			set => this.humanReadable.Value = value;
		}

		private void RenderHumanReadableMarkdown(object sender, EventArgs e)
		{
			MainWindow.UpdateGui(async () =>
			{
				try
				{
					HumanReadableText Text = await this.HumanReadableMarkdown.ToHumanReadableText(this.Language);
					if (Text is null)
					{
						this.contract.ForHumans = this.contract.ForHumans.Remove(this.Language);
						this.HumanReadable = null;
					}
					else
					{
						this.contract.ForHumans = this.contract.ForHumans.Append(Text);
						this.HumanReadable = XamlReader.Parse(await Text.GenerateXAML(this.Contract));
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			});
		}

		/// <summary>
		/// Validates parameters.
		/// </summary>
		public async Task ValidateParameters()
		{
			Variables Variables = new();
			bool Ok = true;

			Variables["Duration"] = this.Duration;

			foreach (ParameterInfo P in this.AllParameterInfos)
				P.Parameter.Populate(Variables);

			foreach (ParameterInfo P in this.AllParameterInfos)
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
		/// Tries to get information about a given role.
		/// </summary>
		/// <param name="Name">Name of role.</param>
		/// <param name="Role">Role reference, if found, null otherwise.</param>
		/// <returns>If a role was found with the given name.</returns>
		public bool TryGetRole(string Name, out RoleInfo Role)
		{
			Role = null;

			if (this.roles?.Value is null)
				return false;

			foreach (RoleInfo R in this.roles.Value)
			{
				if (R.Name == Name)
				{
					Role = R;
					return true;
				}
			}

			return false;
		}

		private void Roles_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.contract.Roles = this.roles.Value.ToRoles();
		}

		/// <summary>
		/// Parts defined the contract.
		/// </summary>
		public PartInfo[] Parts
		{
			get => this.parts.Value;
			set => this.parts.Value = value;
		}

		private void Parts_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.contract.Parts = this.parts.Value.ToParts();
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
		/// Role reference Parameters defined the contract.
		/// </summary>
		public RoleParameterInfo[] RoleParameters
		{
			get => this.roleParameters.Value;
			set => this.roleParameters.Value = value;
		}

		/// <summary>
		/// An array of both <see cref="Parameters"/> and <see cref="RoleParameters"/>.
		/// </summary>
		public ParameterInfo[] AllParameterInfos
		{
			get
			{
				List<ParameterInfo> Parameters = new();

				Parameters.AddRange(this.parameters.Value);
				Parameters.AddRange(this.roleParameters.Value);

				return Parameters.ToArray();
			}
		}

		/// <summary>
		/// An array of both <see cref="Parameters"/> and <see cref="RoleParameters"/>.
		/// </summary>
		public Parameter[] AllParameters
		{
			get
			{
				List<Parameter> Parameters = new();

				Parameters.AddRange(this.parameters.Value.ToParameters());
				Parameters.AddRange(this.roleParameters.Value.ToParameters());

				return Parameters.ToArray();
			}
		}

		private void Parameters_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.contract.Parameters = this.AllParameters;
		}

		/// <summary>
		/// Command for adding a role to the design.
		/// </summary>
		public ICommand AddRole => this.addRole;

		/// <summary>
		/// Adds a role to the design.
		/// </summary>
		public async Task ExecuteAddRole()
		{
			RoleInfo[] Roles = this.Roles;
			int c = Roles.Length;

			Array.Resize(ref Roles, c + 1);
			Roles[c] = new RoleInfo(this, new Role()
			{
				Name = FindNewName("Role", this.Roles),
				Descriptions = new HumanReadableText[] { await "Enter role description as **Markdown**".ToHumanReadableText("en") },
				MinCount = 1,
				MaxCount = 1,
				CanRevoke = false
			}, this.roles);

			this.Roles = Roles;

			foreach (ParameterInfo RoleRef in this.roleParameters.Value)
			{
				if (RoleRef is RoleParameterInfo RoleRefInfo)
					RoleRefInfo.RaisePropertyChanged(nameof(RoleRefInfo.Roles));
			}
		}

		private static string FindNewName(string ProposedName, INamedItem[] NamedItems)
		{
			int i = 1;
			string Result;

			while (true)
			{
				string Suffix = i.ToString();
				bool Found = false;

				Result = ProposedName + Suffix;

				foreach (INamedItem Item in NamedItems)
				{
					if (Item.Name == Result)
					{
						Found = true;
						break;
					}
				}

				if (Found)
					i++;
				else
					return Result;
			}
		}

		/// <summary>
		/// Removes a role from the design
		/// </summary>
		/// <param name="Role">Role to remove</param>
		public void RemoveRole(RoleInfo Role)
		{
			RoleInfo[] Roles = this.Roles;
			int i = Array.IndexOf(Roles, Role);
			if (i < 0)
				return;

			int c = Roles.Length;

			if (i < c - 1)
				Array.Copy(Roles, i + 1, Roles, i, c - i - 1);

			Array.Resize(ref Roles, c - 1);

			this.Roles = Roles;
		}

		/// <summary>
		/// Command for adding a part to the design.
		/// </summary>
		public ICommand AddPart => this.addPart;

		/// <summary>
		/// Adds a part to the design.
		/// </summary>
		public Task ExecuteAddPart()
		{
			PartInfo[] Parts = this.Parts;
			int c = Parts.Length;

			Array.Resize(ref Parts, c + 1);
			Parts[c] = new PartInfo(new Part()
			{
				LegalId = string.Empty,
				Role = string.Empty
			}, this, this.parts);

			this.Parts = Parts;
			this.PartsMode = ContractParts.ExplicitlyDefined;

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

			Array.Resize(ref Parts, c - 1);

			this.Parts = Parts;
		}

		/// <summary>
		/// Command for adding a numeric parameter
		/// </summary>
		public ICommand AddNumericParameter => this.addNumericParameter;

		/// <summary>
		/// Adds a numeric parameter to the design.
		/// </summary>
		public Task ExecuteAddNumericParameter()
		{
			return this.ExecuteAddNumericParameter("Numeric", null, defaultParameterDescription);
		}

		private async Task ExecuteAddNumericParameter(string Name, decimal? Value, params string[] MarkdownDescription)
		{
			NumericalParameter NP = new()
			{
				Name = FindNewName(Name, this.AllParameterInfos),
				Descriptions = await MarkdownDescription.ToHumanReadableText("en"),
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = Value
			};

			this.AddParameter(this.GetParameterInfo(NP));
		}

		private ParameterInfo GetParameterInfo(NumericalParameter NP)
		{
			TextBox ValueControl = new();
			Binding Binding = new("Value")
			{
				Converter = new MoneyToString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.SetBinding(TextBox.TextProperty, "Value");
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			Binding = new("Value")
			{
				Converter = new MoneyToString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.SetBinding(TextBox.TextProperty, "Min");
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			Binding = new Binding("Value")
			{
				Converter = new MoneyToString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.SetBinding(TextBox.TextProperty, "Max");
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			ParameterInfo ParameterInfo = new NumericalParameterInfo(this.contract, NP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		/// <summary>
		/// Command for adding a string parameter
		/// </summary>
		public ICommand AddStringParameter => this.addStringParameter;

		/// <summary>
		/// Adds a string parameter to the design.
		/// </summary>
		public Task ExecuteAddStringParameter()
		{
			return this.ExecuteAddStringParameter("String", null, null, defaultParameterDescription);
		}

		private async Task ExecuteAddStringParameter(string Name, string RegEx, int? MaxLength, params string[] MarkdownDescription)
		{
			StringParameter SP = new()
			{
				Name = FindNewName(Name, this.AllParameterInfos),
				Descriptions = await MarkdownDescription.ToHumanReadableText("en"),
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = null,
				MinLength = null,
				MaxLength = MaxLength,
				RegEx = RegEx
			};

			this.AddParameter(this.GetParameterInfo(SP));
		}

		private ParameterInfo GetParameterInfo(StringParameter SP)
		{
			TextBox ValueControl = new();
			ValueControl.SetBinding(TextBox.TextProperty, "Value");
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			MinControl.SetBinding(TextBox.TextProperty, "Min");
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			MaxControl.SetBinding(TextBox.TextProperty, "Max");
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			TextBox MinLengthControl = new();
			MinLengthControl.SetBinding(TextBox.TextProperty, "MinLength");
			MinLengthControl.TextChanged += this.Parameter_MinLengthTextChanged;

			TextBox MaxLengthControl = new();
			MaxLengthControl.SetBinding(TextBox.TextProperty, "MaxLength");
			MaxLengthControl.TextChanged += this.Parameter_MaxLengthTextChanged;

			TextBox RegExControl = new();
			RegExControl.SetBinding(TextBox.TextProperty, "RegEx");
			RegExControl.TextChanged += this.Parameter_RegExTextChanged;

			ParameterInfo ParameterInfo = new StringParameterInfo(this.contract, SP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, MinLengthControl, MaxLengthControl, RegExControl, this, this.parameters);

			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private async void Parameter_TextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not ParameterInfo ParameterInfo)
					return;

				try
				{
					ParameterInfo.SetValue(TextBox.Text);

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}

				this.RenderHumanReadableMarkdown(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MinTextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not RangedParameterInfo ParameterInfo)
					return;

				try
				{
					ParameterInfo.SetMin(TextBox.Text);

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MaxTextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not RangedParameterInfo ParameterInfo)
					return;

				try
				{
					ParameterInfo.SetMax(TextBox.Text);

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MinLengthTextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not StringParameterInfo ParameterInfo)
					return;

				try
				{
					string Value = TextBox.Text;

					if (string.IsNullOrEmpty(Value))
						ParameterInfo.MinLength = null;
					else
						ParameterInfo.MinLength = int.Parse(Value);

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MaxLengthTextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not StringParameterInfo ParameterInfo)
					return;

				try
				{
					string Value = TextBox.Text;

					if (string.IsNullOrEmpty(Value))
						ParameterInfo.MinLength = null;
					else
						ParameterInfo.MinLength = int.Parse(Value);

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_RegExTextChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not TextBox TextBox || TextBox.Tag is not StringParameterInfo ParameterInfo)
					return;

				try
				{
					ParameterInfo.RegEx = TextBox.Text;

					TextBox.Background = null;
					await this.ValidateParameters();
				}
				catch (Exception)
				{
					TextBox.Background = Brushes.Salmon;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Command for adding a boolean parameter
		/// </summary>
		public ICommand AddBooleanParameter => this.addBooleanParameter;

		/// <summary>
		/// Adds a boolean parameter to the design.
		/// </summary>
		public Task ExecuteAddBooleanParameter()
		{
			return this.ExecuteAddBooleanParameter("Boolean", null, defaultParameterDescription);
		}

		private async Task ExecuteAddBooleanParameter(string Name, bool? Value, params string[] MarkdownDescription)
		{
			BooleanParameter BP = new()
			{
				Name = FindNewName(Name, this.AllParameterInfos),
				Descriptions = await MarkdownDescription.ToHumanReadableText("en"),
				Expression = string.Empty,
				Guide = string.Empty,
				Value = Value
			};

			this.AddParameter(this.GetParameterInfo(BP));
		}

		private ParameterInfo GetParameterInfo(BooleanParameter BP)
		{
			CheckBox CheckBox = new()
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center
			};

			CheckBox.SetBinding(CheckBox.IsCheckedProperty, "Value");
			CheckBox.Checked += this.Parameter_CheckedChanged;
			CheckBox.Unchecked += this.Parameter_CheckedChanged;

			ParameterInfo ParameterInfo = new BooleanParameterInfo(this.contract, BP, CheckBox, this, this.parameters);
			CheckBox.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private async void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not CheckBox CheckBox || CheckBox.Tag is not ParameterInfo ParameterInfo)
					return;

				ParameterInfo.Value = CheckBox.IsChecked;

				await this.ValidateParameters();
				this.RenderHumanReadableMarkdown(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MinIncludedCheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not CheckBox CheckBox || CheckBox.Tag is not RangedParameterInfo ParameterInfo)
					return;

				ParameterInfo.MinIncluded = CheckBox.IsChecked ?? false;

				await this.ValidateParameters();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void Parameter_MaxIncludedCheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not CheckBox CheckBox || CheckBox.Tag is not RangedParameterInfo ParameterInfo)
					return;

				ParameterInfo.MaxIncluded = CheckBox.IsChecked ?? false;

				await this.ValidateParameters();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Command for adding a date parameter
		/// </summary>
		public ICommand AddDateParameter => this.addDateParameter;

		/// <summary>
		/// Adds a date parameter to the design.
		/// </summary>
		public Task ExecuteAddDateParameter()
		{
			return this.ExecuteAddDateParameter("Date", null, defaultParameterDescription);
		}

		private async Task ExecuteAddDateParameter(string Name, DateTime? Value, params string[] MarkdownDescription)
		{
			DateParameter DP = new()
			{
				Name = FindNewName(Name, this.AllParameterInfos),
				Descriptions = await MarkdownDescription.ToHumanReadableText("en"),
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = Value
			};

			this.AddParameter(this.GetParameterInfo(DP));
		}

		private ParameterInfo GetParameterInfo(DateParameter DP)
		{
			TextBox ValueControl = new();
			Binding Binding = new("Value")
			{
				Converter = new DateToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			Binding = new("Min")
			{
				Converter = new DateToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			Binding = new Binding("Max")
			{
				Converter = new DateToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			ParameterInfo ParameterInfo = new DateParameterInfo(this.contract, DP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		/// <summary>
		/// Command for adding a date and time parameter
		/// </summary>
		public ICommand AddDateTimeParameter => this.addDateTimeParameter;

		/// <summary>
		/// Adds a date and time parameter to the design.
		/// </summary>
		public async Task ExecuteAddDateTimeParameter()
		{
			DateTimeParameter DP = new()
			{
				Name = FindNewName("DateTime", this.AllParameterInfos),
				Descriptions = new HumanReadableText[] { await defaultParameterDescription.ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = null
			};

			this.AddParameter(this.GetParameterInfo(DP));
		}

		private ParameterInfo GetParameterInfo(DateTimeParameter DP)
		{
			TextBox ValueControl = new();
			Binding Binding = new("Value")
			{
				Converter = new DateTimeToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			Binding = new Binding("Min")
			{
				Converter = new DateTimeToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			Binding = new Binding("Max")
			{
				Converter = new DateTimeToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			ParameterInfo ParameterInfo = new DateTimeParameterInfo(this.contract, DP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		/// <summary>
		/// Command for adding a time parameter
		/// </summary>
		public ICommand AddTimeParameter => this.addTimeParameter;

		/// <summary>
		/// Adds a time parameter to the design.
		/// </summary>
		public Task ExecuteAddTimeParameter()
		{
			return this.ExecuteAddTimeParameter("Time", null, defaultParameterDescription);
		}

		private async Task ExecuteAddTimeParameter(string Name, TimeSpan? Value, params string[] MarkdownDescription)
		{
			TimeParameter DP = new()
			{
				Name = FindNewName(Name, this.AllParameterInfos),
				Descriptions = await MarkdownDescription.ToHumanReadableText("en"),
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = Value
			};

			this.AddParameter(this.GetParameterInfo(DP));
		}

		private ParameterInfo GetParameterInfo(TimeParameter DP)
		{
			TextBox ValueControl = new();
			Binding Binding = new("Value")
			{
				Converter = new TimeToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			Binding = new Binding("Min")
			{
				Converter = new TimeToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			Binding = new("Min")
			{
				Converter = new TimeToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			ParameterInfo ParameterInfo = new TimeParameterInfo(this.contract, DP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		/// <summary>
		/// Command for adding a duration parameter
		/// </summary>
		public ICommand AddDurationParameter => this.addDurationParameter;

		/// <summary>
		/// Adds a duration parameter to the design.
		/// </summary>
		public async Task ExecuteAddDurationParameter()
		{
			DurationParameter DP = new()
			{
				Name = FindNewName("Duration", this.AllParameterInfos),
				Descriptions = new HumanReadableText[] { await defaultParameterDescription.ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = null
			};

			this.AddParameter(this.GetParameterInfo(DP));
		}

		private ParameterInfo GetParameterInfo(DurationParameter DP)
		{
			TextBox ValueControl = new();
			Binding Binding = new("Value")
			{
				Converter = new DurationToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += this.Parameter_TextChanged;

			TextBox MinControl = new();
			Binding = new Binding("Min")
			{
				Converter = new DurationToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += this.Parameter_MinTextChanged;

			TextBox MaxControl = new();
			Binding = new Binding("Max")
			{
				Converter = new DurationToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += this.Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += this.Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += this.Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += this.Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += this.Parameter_MaxIncludedCheckedChanged;

			ParameterInfo ParameterInfo = new DurationParameterInfo(this.contract, DP, ValueControl, MinControl, MinIncludedControl,
				MaxControl, MaxIncludedControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;
			MinControl.Tag = ParameterInfo;
			MaxControl.Tag = ParameterInfo;
			MinIncludedControl.Tag = ParameterInfo;
			MaxIncludedControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		/// <summary>
		/// Command for adding a calculation parameter
		/// </summary>
		public ICommand AddCalcParameter => this.addCalcParameter;

		/// <summary>
		/// Adds a calculation parameter to the design.
		/// </summary>
		public async Task ExecuteAddCalcParameter()
		{
			CalcParameter CP = new()
			{
				Name = FindNewName("Calc", this.AllParameterInfos),
				Descriptions = new HumanReadableText[] { await defaultParameterDescription.ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty
			};

			this.AddParameter(this.GetParameterInfo(CP));
		}

		/// <summary>
		/// Command for adding a role reference parameter
		/// </summary>
		public ICommand AddRoleReference => this.addRoleReference;

		/// <summary>
		/// Adds a role reference parameter to the design.
		/// </summary>
		public async Task ExecuteAddRoleReference()
		{
			RoleParameter RP = new()
			{
				Name = FindNewName("RoleRef", this.AllParameterInfos),
				Descriptions = new HumanReadableText[] { await defaultParameterDescription.ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Index = 1
			};

			this.AddRoleParameter(this.GetParameterInfo(RP));
		}

		private ParameterInfo GetParameterInfo(CalcParameter CP)
		{
			TextBox ValueControl = new()
			{
				IsReadOnly = true
			};
			Binding Binding = new("Value")
			{
				Converter = new MoneyToString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);

			ParameterInfo ParameterInfo = new CalcParameterInfo(this.contract, CP, ValueControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private RoleParameterInfo GetParameterInfo(RoleParameter RP)
		{
			TextBox ValueControl = new()
			{
				IsReadOnly = true
			};
			Binding Binding = new("Value")
			{
				Converter = new MoneyToString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);

			RoleParameterInfo ParameterInfo = new(this.contract, RP, ValueControl, this, this.roleParameters);
			ValueControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private void AddParameter(ParameterInfo Parameter)
		{
			ParameterInfo[] Parameters = this.Parameters;
			int c = Parameters.Length;

			Array.Resize(ref Parameters, c + 1);
			Parameters[c] = Parameter;

			this.Parameters = Parameters;
		}

		/// <summary>
		/// Removes a parameter from the design
		/// </summary>
		/// <param name="Parameter">Parameter to remove</param>
		public void RemoveParameter(ParameterInfo Parameter)
		{
			ParameterInfo[] Parameters = this.Parameters;
			int i = Array.IndexOf(Parameters, Parameter);
			if (i < 0)
				return;

			int c = Parameters.Length;

			if (i < c - 1)
				Array.Copy(Parameters, i + 1, Parameters, i, c - i - 1);

			Array.Resize(ref Parameters, c - 1);

			this.Parameters = Parameters;
		}

		private void AddRoleParameter(RoleParameterInfo Parameter)
		{
			RoleParameterInfo[] Parameters = this.RoleParameters;
			int c = Parameters.Length;

			Array.Resize(ref Parameters, c + 1);
			Parameters[c] = Parameter;

			this.RoleParameters = Parameters;
		}

		/// <summary>
		/// Removes a role reference parameter from the design
		/// </summary>
		/// <param name="Parameter">Parameter to remove</param>
		public void RemoveRoleParameter(RoleParameterInfo Parameter)
		{
			RoleParameterInfo[] Parameters = this.RoleParameters;
			int i = Array.IndexOf(Parameters, Parameter);
			if (i < 0)
				return;

			int c = Parameters.Length;

			if (i < c - 1)
				Array.Copy(Parameters, i + 1, Parameters, i, c - i - 1);

			Array.Resize(ref Parameters, c - 1);

			this.RoleParameters = Parameters;
		}

		/// <summary>
		/// New command
		/// </summary>
		public ICommand New => this.@new;

		private Task ExecuteNewContract()
		{
			return this.ExecuteNewContract(true);
		}

		private async Task ExecuteNewContract(bool QueryUser)
		{
			try
			{
				if (QueryUser &&
					MessageBox.Show("Are you sure you want clear the form and lose all data that has not been saved?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}

				this.Visibility = ContractVisibility.Public;
				this.PartsMode = ContractParts.TemplateOnly;
				this.ArchiveOptional = Waher.Content.Duration.Zero;
				this.ArchiveRequired = Waher.Content.Duration.Zero;
				this.Duration = Waher.Content.Duration.Zero;
				this.Language = "en";
				this.Languages = Array.Empty<Iso__639_1.Record>();
				this.SignBefore = null;
				this.SignAfter = null;
				this.Roles = Array.Empty<RoleInfo>();
				this.Parts = Array.Empty<PartInfo>();
				this.Parameters = Array.Empty<ParameterInfo>();
				this.RoleParameters = Array.Empty<RoleParameterInfo>();
				this.MachineReadable = string.Empty;
				this.ForMachines = null;
				this.ContractId = string.Empty;
				this.HumanReadableMarkdown = string.Empty;
				this.HumanReadable = null;

				await this.GenerateContract();
			}
			catch (Exception ex)
			{
				Log.Critical(ex.Message);
			}
		}

		/// <summary>
		/// Load command
		/// </summary>
		public ICommand LoadCommand => this.load;

		private async Task ExecuteLoadContract()
		{
			try
			{
				OpenFileDialog Dialog = new()
				{
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "xml",
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Load Smart Contract"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return;

				XmlDocument Doc = new()
				{
					PreserveWhitespace = true
				};

				Doc.Load(Dialog.FileName);

				ParsedContract Parsed = await Waher.Networking.XMPP.Contracts.Contract.Parse(Doc);
				Contract Contract = Parsed.Contract
					?? throw new InvalidOperationException("Not a valid Smart Contract file.");

				if (!Contract.CanActAsTemplate)
					throw new InvalidOperationException("Contract is not a template.");

				await this.SetContract(Contract);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Save command
		/// </summary>
		public ICommand SaveCommand => this.save;

		private Task ExecuteSaveContract()
		{
			try
			{
				SaveFileDialog Dialog = new()
				{
					AddExtension = true,
					CheckFileExists = false,
					CheckPathExists = true,
					CreatePrompt = false,
					DefaultExt = "xml",
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					OverwritePrompt = true,
					Title = "Save Smart Contract"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return Task.CompletedTask;

				string Xml = this.contract.ToXml();
				(string Pretty, XmlElement Parsed) = Xml.ToPrettyXml();

				File.WriteAllText(Dialog.FileName, Pretty);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Import command
		/// </summary>
		public ICommand ImportCommand => this.import;

		private async Task ExecuteImportContract()
		{
			try
			{
				OpenFileDialog Dialog = new()
				{
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "docx",
					Filter = "Microsoft Word Files (*.docx)|*.docx",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Import from Microsoft Word"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (!Result.HasValue || !Result.Value)
					return;

				string Markdown = WordUtilities.ExtractAsMarkdown(Dialog.FileName, out string Language);
				await this.ExecuteNewContract(false);

				if (ContractUtilities.ExtractParameters(ref Markdown, out Dictionary<string, ParameterInformation> HeaderInfo))
				{
					foreach (KeyValuePair<string, ParameterInformation> P in HeaderInfo)
					{
						switch (P.Value.Type)
						{
							case ParameterType.String:
							default:
								await this.ExecuteAddStringParameter(P.Value.Name, null, P.Value.MaxLength,
									GetDescriptions(P.Value.Values));
								break;

							case ParameterType.Boolean:
								if ((P.Value.Values?.Count ?? 0) > 0 && CommonTypes.TryParse(P.Value.Values[0], out bool b))
									await this.ExecuteAddBooleanParameter(P.Value.Name, b, defaultParameterDescription);
								else
									await this.ExecuteAddBooleanParameter(P.Value.Name, null, defaultParameterDescription);
								break;

							case ParameterType.StringWithOptions:
								StringBuilder RegEx = new();
								bool First = true;

								foreach (OptionInformation Option in P.Value.Options)
								{
									if (First)
										First = false;
									else
										RegEx.Append('|');

									RegEx.Append(CommonTypes.RegexStringEncode(Option.Value));
									RegEx.Append(".*");
								}

								RegEx.Append("|.*");

								await this.ExecuteAddStringParameter(P.Value.Name, RegEx.ToString(), P.Value.MaxLength,
									GetDescriptions(P.Value.Values));
								break;

							case ParameterType.ListOfOptions:
								RegEx = new();
								First = true;

								RegEx.Append("^(");

								foreach (OptionInformation Option in P.Value.Options)
								{
									if (First)
										First = false;
									else
										RegEx.Append('|');

									RegEx.Append(CommonTypes.RegexStringEncode(Option.Value));
								}

								RegEx.Append(")$");

								await this.ExecuteAddStringParameter(P.Value.Name, RegEx.ToString(), P.Value.MaxLength,
									GetDescriptions(P.Value.Values));
								break;

							case ParameterType.Date:
								if ((P.Value.Values?.Count ?? 0) > 0 && DateTime.TryParse(P.Value.Values[0], out DateTime TP))
									await this.ExecuteAddDateParameter(P.Value.Name, TP, defaultParameterDescription);
								else
									await this.ExecuteAddDateParameter(P.Value.Name, null, defaultParameterDescription);
								break;

							case ParameterType.Time:
								if ((P.Value.Values?.Count ?? 0) > 0 && TimeSpan.TryParse(P.Value.Values[0], out TimeSpan TS))
									await this.ExecuteAddTimeParameter(P.Value.Name, TS, defaultParameterDescription);
								else
									await this.ExecuteAddTimeParameter(P.Value.Name, null, defaultParameterDescription);
								break;

							case ParameterType.Number:
								if ((P.Value.Values?.Count ?? 0) > 0 && CommonTypes.TryParse(P.Value.Values[0], out decimal d))
									await this.ExecuteAddNumericParameter(P.Value.Name, d, defaultParameterDescription);
								else
									await this.ExecuteAddNumericParameter(P.Value.Name, null, defaultParameterDescription);
								break;
						}
					}
				}

				if (string.IsNullOrEmpty(Language))
					Language = this.Language;
				else
				{
					this.Languages = new string[] { Language }.ToIso639_1();
					this.Language = Language;
				}

				HumanReadableText Text = await Markdown.ToHumanReadableText(Language);
				Markdown = Text.GenerateMarkdown(this.Contract);

				StringBuilder sb = new StringBuilder();
				Text.Serialize(sb);

				XmlDocument Doc = new();
				Doc.LoadXml(sb.ToString());

				LinkedList<XmlElement> Elements = new LinkedList<XmlElement>();
				Elements.AddLast(Doc.DocumentElement);

				while (Elements.First is not null)
				{
					XmlElement E = Elements.First.Value;
					Elements.RemoveFirst();

					if (E.LocalName == "parameter")
					{
						string ParameterName = XML.Attribute(E, "name");
						if (!string.IsNullOrEmpty(ParameterName))
							Markdown = Markdown.Replace("`" + ParameterName + "`", "[%" + ParameterName + "]");
					}

					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2)
							Elements.AddLast(E2);
					}
				}

				this.HumanReadableMarkdown = Markdown;
				this.MachineReadable = "<Nop xmlns=\"https://paiwise.tagroot.io/Schema/PaymentInstructions.xsd\" />";
				this.ContractId = Path.GetFileName(Path.ChangeExtension(Dialog.FileName, string.Empty));

				await this.ValidateParameters();
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private static string[] GetDescriptions(List<string> Descriptions)
		{
			if (Descriptions is null || Descriptions.Count == 0 ||
				(Descriptions.Count == 1 && string.IsNullOrEmpty(Descriptions[0])))
			{
				return new string[] { defaultParameterDescription };
			}
			else
				return Descriptions.ToArray();
		}

		/// <summary>
		/// Command for adding languages
		/// </summary>
		public ICommand AddLanguage => this.addLanguage;

		private Task ExecuteAddLanguage()
		{
			AddLanguageDialog Dialog = new();
			AddLanguageModel Model = new(Dialog);

			bool? Result = Dialog.ShowDialog();
			if (!Result.HasValue || !Result.Value)
				return Task.CompletedTask;

			string Language = Model.SelectedLanguage;
			if (string.IsNullOrEmpty(Language))
				return Task.CompletedTask;

			this.Languages = this.Languages.ToCodes().Append(Language).ToIso639_1();
			this.Language = Language.ToLower();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Command for removing languages
		/// </summary>
		public ICommand RemoveLanguage => this.removeLanguage;

		private bool CanExecuteRemoveLanguage()
		{
			return !string.IsNullOrEmpty(this.Language) && this.Languages.Length >= 2;
		}

		private Task ExecuteRemoveLanguage()
		{
			string Language = this.Language;

			if (MessageBox.Show("Are you sure you want to remove language " + Language + " from the contract?", "Confirm",
				MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			{
				return Task.CompletedTask;
			}

			foreach (ParameterInfo PI in this.AllParameterInfos)
				PI.Parameter.Descriptions = PI.Parameter.Descriptions.Remove(Language);

			foreach (RoleInfo RI in this.Roles)
				RI.Role.Descriptions = RI.Role.Descriptions.Remove(Language);

			this.contract.ForHumans = this.contract.ForHumans.Remove(Language);

			this.Languages = this.Languages.ToCodes().Remove(Language).ToIso639_1();
			this.Language = this.contract.DefaultLanguage;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Key to use when calling the OpenAI service.
		/// </summary>
		public string OpenAiKey
		{
			get => this.openAiKey.Value;
			set => this.openAiKey.Value = value;
		}

		/// <summary>
		/// Propose command
		/// </summary>
		public ICommand Propose => this.propose;

		private bool CanExecuteProposeContract()
		{
			return this.Connected &&
				(this.ParametersOk || this.PartsMode == ContractParts.TemplateOnly) &&
				!string.IsNullOrEmpty(this.ContractId) &&
				this.contract.ForMachines is not null;
		}

		/// <inheritdoc/>
		protected override Task StateChanged(XmppState NewState)
		{
			this.propose.RaiseCanExecuteChanged();
			return base.StateChanged(NewState);
		}

		private async Task ExecuteProposeContract()
		{
			try
			{
				LegalModel LegalModel = this.Network.Legal;

				if (MessageBox.Show("Are you sure you want to propose the designed template to " + LegalModel.Contracts.ComponentAddress + "?", "Confirm",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
				{
					return;
				}

				MainWindow.MouseHourglass();

				Contract Contract = await LegalModel.Contracts.CreateContractAsync(this.contract.ForMachines, this.contract.ForHumans, this.contract.Roles,
					this.contract.Parts, this.contract.Parameters, this.contract.Visibility, this.contract.PartsMode, this.contract.Duration,
					this.contract.ArchiveRequired, this.contract.ArchiveOptional, this.contract.SignAfter, this.contract.SignBefore,
					this.contract.CanActAsTemplate);

				await RuntimeSettings.SetAsync("Contract.Template." + this.ContractId, Contract.ContractId);
				LegalModel.ContractTemplateAdded(this.ContractId, Contract);

				MainWindow.SuccessBox("Template successfully proposed. The template ID, which has been copied to the clipboard, is: " + Contract.ContractId);
				Clipboard.SetText(Contract.ContractId);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}
	}
}
