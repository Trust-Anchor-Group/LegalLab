using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Content;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a boolean parameter
	/// </summary>
	/// <param name="Contract">Contract hosting the parameter</param>
	/// <param name="Parameter">Parameter</param>
	/// <param name="Control">Edit control</param>
	/// <param name="DesignModel">Design model</param>
	/// <param name="Parameters">Collection of parameters.</param>
	public class BooleanParameterInfo(Contract Contract, BooleanParameter Parameter, Control Control, DesignModel DesignModel, Property<ParameterInfo[]> Parameters) 
		: ParameterInfo(Contract, Parameter, Control, DesignModel, Parameters)
	{
		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			this.Value = string.IsNullOrEmpty(Value) ? (bool?)null :
				CommonTypes.TryParse(Value, out bool b) ? b : throw new ArgumentException("Invalid boolean value.", nameof(Value));
		}
	}
}
