using LegalLab.Converters;
using LegalLab.Models.Design;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a calculation parameter
	/// </summary>
	public class CalcParameterInfo : ParameterInfo
	{
		private CalcParameter calcParameter;
		private readonly TextBox textBox;

		/// <summary>
		/// Contains information about a calculation parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public CalcParameterInfo(Contract Contract, CalcParameter Parameter, TextBox Control, DesignModel DesignModel, 
			Property<ParameterInfo[]> Parameters)
			: base(Contract, Parameter, Control, DesignModel, Parameters)
		{
			this.calcParameter = Parameter;
			this.textBox = Control;
		}

		/// <summary>
		/// Method called when the parameter needs to be validated.
		/// </summary>
		public override async Task<bool> ValidateParameter(Variables Variables)
		{
			if (await this.Parameter.IsParameterValid(Variables))
			{
				object Value = this.Parameter.ObjectValue;

				if (Value is decimal d)
					this.textBox.Text = MoneyToString.ToString(d);
				else if (Value is double d2)
					this.textBox.Text = MoneyToString.ToString((decimal)d2);
				else
					this.textBox.Text = Value?.ToString() ?? string.Empty;

				return true;
			}
			else
				return false;
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			throw new InvalidOperationException("Read-only parameter.");
		}

		private static decimal? Parse(string Value)
		{
			throw new InvalidOperationException("Read-only parameter.");
		}

		/// <inheritdoc/>
		public override void ContractUpdated(Contract Contract)
		{
			base.ContractUpdated(Contract);
			this.calcParameter = this.Parameter as CalcParameter;
		}
	}
}
