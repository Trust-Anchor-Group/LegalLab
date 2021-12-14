using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a string parameter
	/// </summary>
	public class StringParameterInfo : RangedParameterInfo
	{
		private readonly Property<string> min;
		private readonly Property<string> max;

		private readonly StringParameter stringParameter;

		/// <summary>
		/// Contains information about a string parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="MinControl">Control for editing minimum value.</param>
		/// <param name="MinIncludedControl">Control for defining if minimum value is included or not.</param>
		/// <param name="MaxControl">Control for editing maximum value.</param>
		/// <param name="MaxIncludedControl">Control for defining if maximum value is included or not.</param>
		/// <param name="DesignModel">Design model</param>
		public StringParameterInfo(Contract Contract, StringParameter Parameter, Control Control, Control MinControl, Control MinIncludedControl, 
			Control MaxControl, Control MaxIncludedControl, DesignModel DesignModel) 
			: base(Contract, Parameter, Control, MinControl, MinIncludedControl, MaxControl, MaxIncludedControl, DesignModel)
		{
			this.stringParameter = Parameter;
		
			this.min = new Property<string>(nameof(this.Min), Parameter.Min, this);
			this.max = new Property<string>(nameof(this.Max), Parameter.Max, this);

			this.MinIncluded = Parameter.MinIncluded;
			this.MaxIncluded = Parameter.MaxIncluded;
		}

		/// <summary>
		/// Minimum value of parameter
		/// </summary>
		public string Min
		{
			get => this.min.Value;
			set
			{
				this.stringParameter.Min = value;
				this.min.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Maximum value of parameter
		/// </summary>
		public string Max
		{
			get => this.max.Value;
			set
			{
				this.stringParameter.Max = value;
				this.max.Value = value;
				this.Revalidate();
			}
		}

		/// <inheritdoc/>
		public override bool MinIncluded
		{
			get => base.MinIncluded;
			set
			{
				this.stringParameter.MinIncluded = value;
				base.MinIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override bool MaxIncluded
		{
			get => base.MaxIncluded;
			set
			{
				this.stringParameter.MaxIncluded = value;
				base.MinIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override void SetMax(string Value)
		{
			this.Max = Value;
		}

		/// <inheritdoc/>
		public override void SetMin(string Value)
		{
			this.Min = Value;
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			this.Value = Value;
		}
	}
}
