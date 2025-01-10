using LegalLab.Converters;
using LegalLab.Extensions;
using LegalLab.Models.Design;
using LegalLab.Models.Legal.Items;
using LegalLab.Models.Legal.Items.Parameters;
using LegalLab.Models.Standards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Script;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contract parameters model
	/// </summary>
	public class ContractParametersModel : Model
	{
		private static readonly Contract emptyContract = new();

		private readonly Property<bool> parametersOk;
		private readonly Property<ParameterInfo[]> parameters;
		private readonly Property<string> language;
		private readonly Property<Iso__639_1.Record[]> languages;

		protected readonly Dictionary<CaseInsensitiveString, ParameterInfo> parametersByName = [];
		private readonly DesignModel designModel;
		private StackPanel languageOptions = null;
		private StackPanel parameterOptions = null;
		private StackPanel additionalCommands = null;

		private Contract contract;
		private Parameter[] contractParameters;

		/// <summary>
		/// Contract parameters model
		/// </summary>
		/// <param name="Parameters">Contract parameters.</param>
		/// <param name="Contract">Optional contract.</param>
		/// <param name="Language">Language</param>
		/// <param name="LegalModel">Reference to the legal model.</param>
		public ContractParametersModel(Parameter[] Parameters, Contract Contract, string Language, DesignModel DesignModel)
		{
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
			this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), [], this);
			this.language = new Property<string>(nameof(this.Language), Language, this);
			this.languages = new Property<Iso__639_1.Record[]>(nameof(this.Languages), [], this);
			this.contract = Contract ?? emptyContract;
			this.designModel = DesignModel;

			this.SetParameters(Parameters);
		}

		/// <summary>
		/// Sets the current contract.
		/// </summary>
		/// <param name="Contract">Contract object.</param>
		public virtual Task SetContract(Contract Contract)
		{
			if (this.contract is not null)
				this.contract.FormatParameterDisplay -= this.Contract_FormatParameterDisplay;

			this.contract = Contract;
			this.contract.FormatParameterDisplay += this.Contract_FormatParameterDisplay;

			this.SetParameters(Contract.Parameters);

			return Task.CompletedTask;
		}

		private Task Contract_FormatParameterDisplay(object Sender, ParameterValueFormattingEventArgs e)
		{
			if (e.Value is Waher.Content.Duration D)
				e.Value = DurationToString.ToString(D);

			return Task.CompletedTask;
		}

		protected void SetParameters(Parameter[] ContractParameters)
		{
			this.contractParameters = ContractParameters;

			string PrevLanguage = this.Language;
			bool Found = false;

			this.Languages = this.contract.GetLanguages().ToIso639_1();

			if (!string.IsNullOrWhiteSpace(PrevLanguage))
			{
				foreach (Iso__639_1.Record Rec in this.Languages)
				{
					if (Rec.Code == PrevLanguage)
					{
						Found = true;
						break;
					}
				}
			}

			this.Language = Found ? PrevLanguage : this.contract.DefaultLanguage;

			if (string.IsNullOrEmpty(this.Language) && (this.Languages?.Length ?? 0) == 0)
			{
				this.Languages = DesignModel.English;
				this.Language = "en";
			}

			List<ParameterInfo> Parameters = [];

			if (ContractParameters is not null)
			{
				foreach (Parameter Parameter in ContractParameters)
				{
					if (this.parametersByName.TryGetValue(Parameter.Name, out ParameterInfo ParameterInfo))
					{
						Parameters.Add(ParameterInfo);
						ParameterInfo.ContractUpdated(this.Contract);
					}
				}
			}

			this.Parameters = [.. Parameters];
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
			set => this.SetLanguage(value).Wait();
		}

		/// <summary>
		/// Available languages.
		/// </summary>
		public Iso__639_1.Record[] Languages
		{
			get => this.languages.Value;
			set => this.languages.Value = value;
		}

		/// <summary>
		/// Sets the current language.
		/// </summary>
		/// <param name="Language">Language</param>
		protected virtual async Task<bool> SetLanguage(string Language)
		{
			try
			{
				this.language.Value = Language;

				if (!string.IsNullOrEmpty(Language))
				{
					foreach (ParameterInfo PI in this.Parameters)
					{
						HumanReadableText Text = PI.Parameter.Descriptions.Find(Language);

						if (Text is null)
							PI.DescriptionAsMarkdown = string.Empty;
						else
							PI.DescriptionAsMarkdown = (await Text.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();
					}

					if (this.languageOptions is not null)
						await this.PopulateParameters(this.languageOptions, this.parameterOptions, this.additionalCommands, null);
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

		/// <summary>
		/// If all parameters are OK or not
		/// </summary>
		public bool ParametersOk
		{
			get => this.parametersOk.Value;
			set
			{
				this.parametersOk.Value = value;
				this.ParametersOkChanged();
			}
		}

		protected virtual void ParametersOkChanged()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Populates a <see cref="StackPanel"/> with parameter controls.
		/// </summary>
		/// <param name="Languages">StackPanel to language options</param>
		/// <param name="Parameters">StackPanel to populate with parameters</param>
		/// <param name="AdditionalCommands">StackPanel to populate with additional commends.</param>
		/// <param name="PresetValues">Optional preset values. Can be null.</param>
		/// <returns>Reference to first editable control.</returns>
		public virtual async Task<Control> PopulateParameters(StackPanel Languages, StackPanel Parameters, StackPanel AdditionalCommands,
			Dictionary<CaseInsensitiveString, object> PresetValues)
		{
			List<ParameterInfo> ParameterList = [];
			ParameterInfo ParameterInfo;
			Control First = null;

			this.languageOptions = Languages;
			if (this.languageOptions is not null)
				this.languageOptions.DataContext = this;

			this.parameterOptions = Parameters;
			this.parameterOptions.Children.Clear();
			this.parameterOptions.DataContext = this;
			this.parametersByName.Clear();

			foreach (Parameter Parameter in this.contractParameters)
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

					First ??= CheckBox;
				}
				else
				{
					System.Windows.Controls.Label Label = new()
					{
						Content = Parameter.GetLabel(),
						Margin = new Thickness(0, 10, 0, 0)
					};

					TextBox TextBox = new()
					{
						Tag = Parameter.Name,
						Text = MoneyToString.ToString(Parameter.ObjectValue),
						ToolTip = await Parameter.ToSimpleXAML(this.Language, this.contract)
					};

					if (PresetValues?.TryGetValue(Parameter.Name, out object PresetValue) ?? false)
						TextBox.Text = MoneyToString.ToString(PresetValue) ?? string.Empty;
					else
						PresetValue = null;

					TextBox.TextChanged += this.Parameter_TextChanged;

					if (Parameter is NumericalParameter NP)
					{
						if (PresetValue is decimal d)
							NP.Value = d;

						this.parametersByName[Parameter.Name] = ParameterInfo = new NumericalParameterInfo(this.contract, NP, TextBox,
							null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is StringParameter SP)
					{
						if (PresetValue is string s)
							SP.Value = s;

						this.parametersByName[Parameter.Name] = ParameterInfo = new StringParameterInfo(this.contract, SP, TextBox,
							null, null, null, null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is DateParameter DP)
					{
						if (PresetValue is DateTime TP)
							DP.Value = TP;

						this.parametersByName[Parameter.Name] = ParameterInfo = new DateParameterInfo(this.contract, DP, TextBox,
							null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is DateTimeParameter DTP)
					{
						if (PresetValue is DateTime TP)
							DTP.Value = TP;

						this.parametersByName[Parameter.Name] = ParameterInfo = new DateTimeParameterInfo(this.contract, DTP, TextBox,
							null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is TimeParameter TP)
					{
						if (PresetValue is TimeSpan TS)
							TP.Value = TS;

						this.parametersByName[Parameter.Name] = ParameterInfo = new TimeParameterInfo(this.contract, TP, TextBox,
							null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is DurationParameter DrP)
					{
						if (PresetValue is Waher.Content.Duration D)
							DrP.Value = D;

						this.parametersByName[Parameter.Name] = ParameterInfo = new DurationParameterInfo(this.contract, DrP, TextBox,
							null, null, null, null, this.designModel, this.parameters);
					}
					else if (Parameter is CalcParameter CP)
					{
						TextBox.IsReadOnly = true;

						this.parametersByName[Parameter.Name] = ParameterInfo = new CalcParameterInfo(this.contract, CP, TextBox,
							this.designModel, this.parameters);
					}
					else if (Parameter is ContractReferenceParameter CRP)
					{
						if (PresetValue is string s)
							CRP.Value = s;

						this.parametersByName[Parameter.Name] = ParameterInfo = new ContractReferenceParameterInfo(this.contract, CRP, TextBox,
							this.designModel, this.parameters);
					}
					else
						continue;

					Parameters.Children.Add(Label);
					Parameters.Children.Add(TextBox);

					First ??= TextBox;
				}

				ParameterList.Add(ParameterInfo);
			}

			this.Parameters = [.. ParameterList];

			await this.ValidateParameters();

			this.additionalCommands = AdditionalCommands;
			if (this.additionalCommands is not null)
			{
				this.additionalCommands.DataContext = this;
				this.additionalCommands.Visibility = Visibility.Visible;
				this.additionalCommands.InvalidateVisual();
			}

			return First;
		}

		private async void Parameter_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is not CheckBox CheckBox || !this.parametersByName.TryGetValue(CheckBox.Tag.ToString(), out ParameterInfo ParameterInfo))
					return;

				ParameterInfo.Value = CheckBox.IsChecked;

				await this.ValidateParameters();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			await this.RaiseParametersChanged();
		}

		protected async Task RaiseParametersChanged()
		{
			try
			{
				await this.ParametersChanged();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		protected virtual Task ParametersChanged()
		{
			return Task.CompletedTask;  // Do nothing by default.
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

					TextBox.Background = ParameterInfo.Protection.DefaultBrush();

					Log.Informational("Parameter " + ParameterInfo.Parameter.Name + " set to " + TextBox.Text.ToString());

					await this.ValidateParameters();
				}
				catch (Exception ex)
				{
					TextBox.Background = Brushes.Salmon;
					this.ParametersOk = false;

					Log.Error("Unable to set parameter " + ParameterInfo.Parameter.Name + " to " + TextBox.Text.ToString() + ": " + ex.Message);
				}

				await this.RaiseParametersChanged();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Validates available parameters. If OK, a collection of variables is returned, otherwise null is returned.
		/// </summary>
		/// <returns>Collection of parameter values, if ok, null otherwise.</returns>
		public virtual async Task<Variables> ValidateParameters()
		{
			Variables Variables = [];
			bool Ok = true;

			Variables["Duration"] = this.contract.Duration;

			foreach (ParameterInfo P in this.parametersByName.Values)
				P.Parameter.Populate(Variables);

			foreach (ParameterInfo P in this.parametersByName.Values)
			{
				if (await P.ValidateParameter(Variables))
				{
					P.Control.Background = P.Protection.DefaultBrush();
					Log.Informational("Parameter " + P.Name + " is OK.");
				}
				else
				{
					StringBuilder Msg = new();

					Msg.Append("Parameter ");
					Msg.Append(P.Name);
					Msg.Append(" contains errors.");

					if (P.ErrorReason.HasValue)
					{
						Msg.Append(" Reason: ");
						Msg.Append(P.ErrorReason.Value.ToString());
					}

					if (!string.IsNullOrEmpty(P.ErrorText))
					{
						Msg.Append(", Text: ");
						Msg.Append(P.ErrorText);
					}

					P.Control.Background = Brushes.Salmon;
					Log.Error(Msg.ToString());
					Ok = false;
				}
			}

			this.ParametersOk = Ok;

			return Ok ? Variables : null;
		}

		/// <summary>
		/// Parameters defined the contract.
		/// </summary>
		public ParameterInfo[] Parameters
		{
			get => this.parameters.Value;
			set => this.parameters.Value = value;
		}
	}
}
