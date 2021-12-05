using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contract model
	/// </summary>
	public class ContractModel : Model
	{
		private readonly Property<bool> parametersOk;

		private readonly Command propose;

		private readonly Dictionary<string, ParameterInfo> parametersByName = new Dictionary<string, ParameterInfo>();
		private readonly Contract contract;

		/// <summary>
		/// Contract model
		/// </summary>
		/// <param name="Contract">Contract</param>
		public ContractModel(Contract Contract)
		{
			this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);

			this.propose = new Command(this.CanExecutePropose, this.ExecutePropose);

			this.contract = Contract;
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
		/// Propose contract command
		/// </summary>
		public ICommand Propose => this.propose;

		/// <summary>
		/// If the propose command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecutePropose()
		{
			return this.ParametersOk;
		}

		/// <summary>
		/// Proposes the contract.
		/// </summary>
		public void ExecutePropose()
		{
			// TODO
		}

		/// <summary>
		/// Populates a <see cref="StackPanel"/> with parameter controls.
		/// </summary>
		/// <param name="Parameters">StackPanel to populate</param>
		public void PopulateParameters(StackPanel Parameters)
		{
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

					this.parametersByName[Parameter.Name] = new ParameterInfo(Parameter, CheckBox);

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

					this.parametersByName[Parameter.Name] = new ParameterInfo(Parameter, TextBox);

					Parameters.Children.Add(Label);
					Parameters.Children.Add(TextBox);
				}
			}

			this.ValidateParameters();
			PopulateHumanReadableText();
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

			if (ParameterInfo.Parameter is BooleanParameter BP)
				BP.Value = CheckBox.IsChecked;

			this.ValidateParameters();
			PopulateHumanReadableText();
		}

		private void Parameter_TextChanged(object sender, RoutedEventArgs e)
		{
			if (!(sender is TextBox TextBox) || !this.parametersByName.TryGetValue(TextBox.Tag.ToString(), out ParameterInfo ParameterInfo))
				return;

			if (ParameterInfo.Parameter is StringParameter SP)
				SP.Value = TextBox.Text;
			else if (ParameterInfo.Parameter is NumericalParameter NP)
			{
				if (double.TryParse(TextBox.Text, out double d))
				{
					NP.Value = d;
					TextBox.Background = null;
				}
				else
				{
					TextBox.Background = Brushes.Salmon;
					return;
				}
			}
			else if (ParameterInfo.Parameter is BooleanParameter BP)
			{
				if (bool.TryParse(TextBox.Text, out bool b))
				{
					BP.Value = b;
					TextBox.Background = null;
				}
				else
				{
					TextBox.Background = Brushes.Salmon;
					return;
				}
			}

			this.ValidateParameters();
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
			// TODO
		}
	}
}
