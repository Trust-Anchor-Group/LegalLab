using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a boolean parameter
	/// </summary>
	public class BooleanParameterInfo : ParameterInfo
	{
		private readonly BooleanParameter booleanParameter;

		/// <summary>
		/// Contains information about a boolean parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		public BooleanParameterInfo(Contract Contract, BooleanParameter Parameter, Control Control, DesignModel DesignModel) 
			: base(Contract, Parameter, Control, DesignModel)
		{
			this.booleanParameter = Parameter;
		}

		/// <inheritdoc/>
		public override void Revalidate()
		{
			this.booleanParameter.SetValue(this.booleanParameter.Value);
		}
	}
}
