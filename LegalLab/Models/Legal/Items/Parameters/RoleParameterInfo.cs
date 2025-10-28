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
	/// Contains information about a role parameter
	/// </summary>
	/// <param name="Contract">Contract hosting the parameter</param>
	/// <param name="Parameter">Parameter</param>
	/// <param name="Control">Edit control</param>
	/// <param name="DesignModel">Design model</param>
	/// <param name="Parameters">Collection of parameters.</param>
	public class RoleParameterInfo(Contract Contract, RoleParameter Parameter, TextBox Control, 
		DesignModel DesignModel, Property<ParameterInfo[]> Parameters) 
		: ParameterInfo(Contract, Parameter, Control, DesignModel, Parameters)
	{
		private RoleParameter roleParameter = Parameter;
		private readonly TextBox textBox = Control;

		/// <summary>
		/// Method called when the parameter needs to be validated.
		/// </summary>
		public override async Task<bool> ValidateParameter(Variables Variables)
		{
			bool Result = await this.roleParameter.IsParameterValid(Variables, this.designModel?.Network.Legal.Contracts);

			this.ErrorReason = this.Parameter.ErrorReason;
			this.ErrorText = this.Parameter.ErrorText;

			if (Result)
			{
				object Value = this.Parameter.ObjectValue;

				this.@value.Value = Value;
				this.textBox.Text = MoneyToString.ToString(Value);
			}

			return Result;
		}

		/// <summary>
		/// Parameter value
		/// </summary>
		public override object Value
		{
			get => this.@value.Value;
			set => this.@value.Value = value;
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			throw new InvalidOperationException("Read-only parameter.");
		}

		/// <inheritdoc/>
		public override void ContractUpdated(Contract Contract)
		{
			base.ContractUpdated(Contract);
			this.roleParameter = this.Parameter as RoleParameter;
		}
	}
}
