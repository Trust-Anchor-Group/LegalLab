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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
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
		private readonly DelayedActionProperty<string> machineReadable;
		private readonly Property<string> forMachinesLocalName;
		private readonly Property<string> forMachinesNamespace;
		private readonly Property<XmlElement> forMachines;
		private readonly DelayedActionProperty<string> humanReadableMarkdown;
		private readonly Property<object> humanReadable;
		private readonly PersistedProperty<string> microsoftTranslatorKey;

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
		private readonly Command @new;
		private readonly Command load;
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
			this.languages = new Property<Iso__639_1.Record[]>(nameof(this.Languages), new Iso__639_1.Record[0], this);
			this.signBefore = new Property<DateTime?>(nameof(this.SignBefore), null, this);
			this.signAfter = new Property<DateTime?>(nameof(this.SignAfter), null, this);
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.roles = new Property<RoleInfo[]>(nameof(this.Roles), new RoleInfo[0], this);
			this.parts = new Property<PartInfo[]>(nameof(this.Parts), new PartInfo[0], this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), new ParameterInfo[0], this);
			this.machineReadable = new DelayedActionProperty<string>(nameof(this.MachineReadable), TimeSpan.FromSeconds(1), string.Empty, this);
			this.forMachines = new Property<XmlElement>(nameof(this.ForMachines), null, this);
			this.forMachinesLocalName = new Property<string>(nameof(this.ForMachinesLocalName), string.Empty, this);
			this.forMachinesNamespace = new Property<string>(nameof(this.ForMachinesNamespace), string.Empty, this);
			this.contractId = new Property<string>(nameof(this.ContractId), string.Empty, this);
			this.humanReadableMarkdown = new DelayedActionProperty<string>(nameof(this.HumanReadableMarkdown), TimeSpan.FromSeconds(1), true, string.Empty, this);
			this.humanReadable = new Property<object>(nameof(this.HumanReadable), null, this);

			this.roles.PropertyChanged += Roles_PropertyChanged;
			this.parameters.PropertyChanged += Parameters_PropertyChanged;
			this.parts.PropertyChanged += Parts_PropertyChanged;

			this.Add(this.microsoftTranslatorKey = new PersistedProperty<string>("Design", nameof(MicrosoftTranslatorKey), true, string.Empty, this));

			this.machineReadable.OnAction += NormalizeMachineReadableXml;
			this.humanReadableMarkdown.OnAction += RenderHumanReadableMarkdown;

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
			this.@new = new Command(this.ExecuteNewContract);
			this.load = new Command(this.ExecuteLoadContract);
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

			List<PartInfo> Parts = new List<PartInfo>();

			if (!(Contract.Parts is null))
			{
				foreach (Part Part in Contract.Parts)
					Parts.Add(new PartInfo(Part, this, this.parts));
			}

			this.Parts = Parts.ToArray();

			List<RoleInfo> Roles = new List<RoleInfo>();

			if (!(Contract.Roles is null))
			{
				foreach (Role Role in Contract.Roles)
					Roles.Add(new RoleInfo(this, Role, this.roles));
			}

			this.Roles = Roles.ToArray();

			List<ParameterInfo> ParameterList = new List<ParameterInfo>();
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
				else
					continue;

				ParameterList.Add(ParameterInfo);
			}

			this.Parameters = ParameterList.ToArray();

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
			MainWindow.currentInstance.DesignTab.MicrosoftTranslationKey.Password = this.MicrosoftTranslatorKey;
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
		public string[] Visibilities => Enum.GetNames(typeof(ContractVisibility));

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
		public string[] PartsModes => Enum.GetNames(typeof(ContractParts));

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
							foreach (ParameterInfo PI in this.Parameters)
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

						List<TranslationItem> Items = new List<TranslationItem>();

						foreach (ParameterInfo PI in this.Parameters)
							this.Add(Items, PI, FromLanguage, Language);

						foreach (RoleInfo RI in this.Roles)
							this.Add(Items, RI, FromLanguage, Language);

						this.Add(Items, this, FromLanguage, Language);

						if (Items.Count > 0)
						{
							List<string> AllTexts = new List<string>();

							foreach (TranslationItem Item in Items)
								AllTexts.AddRange(Item.Original);

							string[] AllTranslated = await Translator.Translate(AllTexts.ToArray(), FromLanguage, Language, this.MicrosoftTranslatorKey);

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

		private void Add(List<TranslationItem> Items, ITranslatable Translatable, string From, string To)
		{
			string[] Texts = Translatable.GetTranslatableTexts(To);
			if (!(Texts is null))
				Translatable.SetTranslatableTexts(Texts, To);
			else
			{
				Texts = Translatable.GetTranslatableTexts(From);
				if (!(Texts is null))
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
			Variables Variables = new Variables();
			bool Ok = true;

			Variables["Duration"] = this.Duration;

			foreach (ParameterInfo P in this.Parameters)
				P.Parameter.Populate(Variables);

			foreach (ParameterInfo P in this.Parameters)
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

		private void Parameters_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.contract.Parameters = this.parameters.Value.ToParameters();
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

			Array.Resize<RoleInfo>(ref Roles, c + 1);
			Roles[c] = new RoleInfo(this, new Role()
			{
				Name = this.FindNewName("Role", this.Roles),
				Descriptions = new HumanReadableText[] { await "Enter role description as **Markdown**".ToHumanReadableText("en") },
				MinCount = 1,
				MaxCount = 1,
				CanRevoke = false
			}, this.roles);

			this.Roles = Roles;
		}

		private string FindNewName(string ProposedName, INamedItem[] NamedItems)
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
		/// Command for adding a numeric parameter
		/// </summary>
		public ICommand AddNumericParameter => this.addNumericParameter;

		/// <summary>
		/// Adds a numeric parameter to the design.
		/// </summary>
		public async Task ExecuteAddNumericParameter()
		{
			NumericalParameter NP = new NumericalParameter()
			{
				Name = this.FindNewName("Numeric", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = null
			};

			this.AddParameter(this.GetParameterInfo(NP));
		}

		private ParameterInfo GetParameterInfo(NumericalParameter NP)
		{
			TextBox ValueControl = new TextBox();
			Binding Binding = new Binding("Value")
			{
				Converter = new MoneyToString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.SetBinding(TextBox.TextProperty, "Value");
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			Binding = new Binding("Value")
			{
				Converter = new MoneyToString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.SetBinding(TextBox.TextProperty, "Min");
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			Binding = new Binding("Value")
			{
				Converter = new MoneyToString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.SetBinding(TextBox.TextProperty, "Max");
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

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
		public async Task ExecuteAddStringParameter()
		{
			StringParameter SP = new StringParameter()
			{
				Name = this.FindNewName("String", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Max = null,
				MaxIncluded = false,
				Min = null,
				MinIncluded = false,
				Value = null,
				MinLength = null,
				MaxLength = null,
				RegEx = null
			};

			this.AddParameter(this.GetParameterInfo(SP));
		}

		private ParameterInfo GetParameterInfo(StringParameter SP)
		{
			TextBox ValueControl = new TextBox();
			ValueControl.SetBinding(TextBox.TextProperty, "Value");
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			MinControl.SetBinding(TextBox.TextProperty, "Min");
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			MaxControl.SetBinding(TextBox.TextProperty, "Max");
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

			TextBox MinLengthControl = new TextBox();
			MinLengthControl.SetBinding(TextBox.TextProperty, "MinLength");
			MinLengthControl.TextChanged += Parameter_MinLengthTextChanged;

			TextBox MaxLengthControl = new TextBox();
			MaxLengthControl.SetBinding(TextBox.TextProperty, "MaxLength");
			MaxLengthControl.TextChanged += Parameter_MaxLengthTextChanged;

			TextBox RegExControl = new TextBox();
			RegExControl.SetBinding(TextBox.TextProperty, "RegEx");
			RegExControl.TextChanged += Parameter_RegExTextChanged;

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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is ParameterInfo ParameterInfo))
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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is RangedParameterInfo ParameterInfo))
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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is RangedParameterInfo ParameterInfo))
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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is StringParameterInfo ParameterInfo))
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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is StringParameterInfo ParameterInfo))
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
				if (!(sender is TextBox TextBox) || !(TextBox.Tag is StringParameterInfo ParameterInfo))
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
		public async Task ExecuteAddBooleanParameter()
		{
			BooleanParameter BP = new BooleanParameter()
			{
				Name = this.FindNewName("Boolean", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty,
				Value = null
			};

			this.AddParameter(this.GetParameterInfo(BP));
		}

		private ParameterInfo GetParameterInfo(BooleanParameter BP)
		{
			CheckBox CheckBox = new CheckBox()
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center
			};

			CheckBox.SetBinding(CheckBox.IsCheckedProperty, "Value");
			CheckBox.Checked += Parameter_CheckedChanged;
			CheckBox.Unchecked += Parameter_CheckedChanged;

			ParameterInfo ParameterInfo = new BooleanParameterInfo(this.contract, BP, CheckBox, this, this.parameters);
			CheckBox.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private async void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(sender is CheckBox CheckBox) || !(CheckBox.Tag is ParameterInfo ParameterInfo))
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
				if (!(sender is CheckBox CheckBox) || !(CheckBox.Tag is RangedParameterInfo ParameterInfo))
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
				if (!(sender is CheckBox CheckBox) || !(CheckBox.Tag is RangedParameterInfo ParameterInfo))
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
		public async Task ExecuteAddDateParameter()
		{
			DateParameter DP = new DateParameter()
			{
				Name = this.FindNewName("Date", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
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

		private ParameterInfo GetParameterInfo(DateParameter DP)
		{
			TextBox ValueControl = new TextBox();
			Binding Binding = new Binding("Value")
			{
				Converter = new DateToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			Binding = new Binding("Min")
			{
				Converter = new DateToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			Binding = new Binding("Max")
			{
				Converter = new DateToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

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
			DateTimeParameter DP = new DateTimeParameter()
			{
				Name = this.FindNewName("DateTime", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
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
			TextBox ValueControl = new TextBox();
			Binding Binding = new Binding("Value")
			{
				Converter = new DateTimeToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			Binding = new Binding("Min")
			{
				Converter = new DateTimeToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			Binding = new Binding("Max")
			{
				Converter = new DateTimeToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

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
		public async Task ExecuteAddTimeParameter()
		{
			TimeParameter DP = new TimeParameter()
			{
				Name = this.FindNewName("Time", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
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

		private ParameterInfo GetParameterInfo(TimeParameter DP)
		{
			TextBox ValueControl = new TextBox();
			Binding Binding = new Binding("Value")
			{
				Converter = new TimeToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			Binding = new Binding("Min")
			{
				Converter = new TimeToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			Binding = new Binding("Min")
			{
				Converter = new TimeToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

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
			DurationParameter DP = new DurationParameter()
			{
				Name = this.FindNewName("Duration", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
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
			TextBox ValueControl = new TextBox();
			Binding Binding = new Binding("Value")
			{
				Converter = new DurationToXmlString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);
			ValueControl.TextChanged += Parameter_TextChanged;

			TextBox MinControl = new TextBox();
			Binding = new Binding("Min")
			{
				Converter = new DurationToXmlString()
			};
			MinControl.SetBinding(TextBox.TextProperty, Binding);
			MinControl.TextChanged += Parameter_MinTextChanged;

			TextBox MaxControl = new TextBox();
			Binding = new Binding("Max")
			{
				Converter = new DurationToXmlString()
			};
			MaxControl.SetBinding(TextBox.TextProperty, Binding);
			MaxControl.TextChanged += Parameter_MaxTextChanged;

			CheckBox MinIncludedControl = new CheckBox();
			MinIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MinIncluded");
			MinIncludedControl.Checked += Parameter_MinIncludedCheckedChanged;
			MinIncludedControl.Unchecked += Parameter_MinIncludedCheckedChanged;

			CheckBox MaxIncludedControl = new CheckBox();
			MaxIncludedControl.SetBinding(CheckBox.IsCheckedProperty, "MaxIncluded");
			MaxIncludedControl.Checked += Parameter_MaxIncludedCheckedChanged;
			MaxIncludedControl.Unchecked += Parameter_MaxIncludedCheckedChanged;

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
			CalcParameter CP = new CalcParameter()
			{
				Name = this.FindNewName("Calc", this.Parameters),
				Descriptions = new HumanReadableText[] { await "Enter parameter description as **Markdown**".ToHumanReadableText("en") },
				Expression = string.Empty,
				Guide = string.Empty
			};

			this.AddParameter(this.GetParameterInfo(CP));
		}

		private ParameterInfo GetParameterInfo(CalcParameter CP)
		{
			TextBox ValueControl = new TextBox()
			{
				IsReadOnly = true
			};
			Binding Binding = new Binding("Value")
			{
				Converter = new MoneyToString()
			};
			ValueControl.SetBinding(TextBox.TextProperty, Binding);

			ParameterInfo ParameterInfo = new CalcParameterInfo(this.contract, CP, ValueControl, this, this.parameters);
			ValueControl.Tag = ParameterInfo;

			return ParameterInfo;
		}

		private void AddParameter(ParameterInfo Parameter)
		{
			ParameterInfo[] Parameters = this.Parameters;
			int c = Parameters.Length;

			Array.Resize<ParameterInfo>(ref Parameters, c + 1);
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
			int i = Array.IndexOf<ParameterInfo>(Parameters, Parameter);
			if (i < 0)
				return;

			int c = Parameters.Length;

			if (i < c - 1)
				Array.Copy(Parameters, i + 1, Parameters, i, c - i - 1);

			Array.Resize<ParameterInfo>(ref Parameters, c - 1);

			this.Parameters = Parameters;
		}

		/// <summary>
		/// New command
		/// </summary>
		public ICommand New => this.@new;

		private async Task ExecuteNewContract()
		{
			try
			{
				if (MessageBox.Show("Are you sure you want clear the form and lose all data that has not been saved?", "Confirm",
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
				this.Languages = new Iso__639_1.Record[0];
				this.SignBefore = null;
				this.SignAfter = null;
				this.Roles = new RoleInfo[0];
				this.Parts = new PartInfo[0];
				this.Parameters = new ParameterInfo[0];
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
				OpenFileDialog Dialog = new OpenFileDialog()
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

				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};

				Doc.Load(Dialog.FileName);

				ParsedContract Parsed = await Waher.Networking.XMPP.Contracts.Contract.Parse(Doc);
				Contract Contract = Parsed.Contract;
				if (Contract is null)
					throw new InvalidOperationException("Not a valid Smart Contract file.");

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
				SaveFileDialog Dialog = new SaveFileDialog()
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
		/// Command for adding languages
		/// </summary>
		public ICommand AddLanguage => this.addLanguage;

		private Task ExecuteAddLanguage()
		{
			AddLanguageDialog Dialog = new AddLanguageDialog();
			AddLanguageModel Model = new AddLanguageModel(Dialog);

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

			foreach (ParameterInfo PI in this.Parameters)
				PI.Parameter.Descriptions = PI.Parameter.Descriptions.Remove(Language);

			foreach (RoleInfo RI in this.Roles)
				RI.Role.Descriptions = RI.Role.Descriptions.Remove(Language);

			this.contract.ForHumans = this.contract.ForHumans.Remove(Language);

			this.Languages = this.Languages.ToCodes().Remove(Language).ToIso639_1();
			this.Language = this.contract.DefaultLanguage;
	
			return Task.CompletedTask;
		}

		/// <summary>
		/// Key to use when calling the Microsoft Translator service.
		/// </summary>
		public string MicrosoftTranslatorKey
		{
			get => this.microsoftTranslatorKey.Value;
			set => this.microsoftTranslatorKey.Value = value;
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
				!(this.contract.ForMachines is null);
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
