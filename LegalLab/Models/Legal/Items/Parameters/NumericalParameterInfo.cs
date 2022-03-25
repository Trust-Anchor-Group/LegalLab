using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Content;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a numerical parameter
	/// </summary>
	public class NumericalParameterInfo : RangedParameterInfo
	{
		private readonly Property<decimal?> min;
		private readonly Property<decimal?> max;

		private NumericalParameter numericalParameter;

		/// <summary>
		/// Contains information about a numerical parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="MinControl">Control for editing minimum value.</param>
		/// <param name="MinIncludedControl">Control for defining if minimum value is included or not.</param>
		/// <param name="MaxControl">Control for editing maximum value.</param>
		/// <param name="MaxIncludedControl">Control for defining if maximum value is included or not.</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public NumericalParameterInfo(Contract Contract, NumericalParameter Parameter, Control Control, Control MinControl, Control MinIncludedControl,
			Control MaxControl, Control MaxIncludedControl, DesignModel DesignModel, Property<ParameterInfo[]> Parameters)
			: base(Contract, Parameter, Control, MinControl, MinIncludedControl, MaxControl, MaxIncludedControl, DesignModel, Parameters)
		{
			this.numericalParameter = Parameter;
		
			this.min = new Property<decimal?>(nameof(this.Min), Parameter.Min, this);
			this.max = new Property<decimal?>(nameof(this.Max), Parameter.Max, this);

			this.MinIncluded = Parameter.MinIncluded;
			this.MaxIncluded = Parameter.MaxIncluded;
		}

		/// <summary>
		/// Minimum value of parameter
		/// </summary>
		public decimal? Min
		{
			get => this.min.Value;
			set
			{
				this.numericalParameter.Min = value;
				this.min.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Maximum value of parameter
		/// </summary>
		public decimal? Max
		{
			get => this.max.Value;
			set
			{
				this.numericalParameter.Max = value;
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
				this.numericalParameter.MinIncluded = value;
				base.MinIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override bool MaxIncluded
		{
			get => base.MaxIncluded;
			set
			{
				this.numericalParameter.MaxIncluded = value;
				base.MaxIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override void SetMax(string Value)
		{
			this.Max = Parse(Value);
		}

		/// <inheritdoc/>
		public override void SetMin(string Value)
		{
			this.Min = Parse(Value);
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			this.Value = Parse(Value);
		}

		private static decimal? Parse(string Value)
		{
			if (string.IsNullOrEmpty(Value))
				return null;
			else if (CommonTypes.TryParse(Value, out decimal d))
				return d;
			else
				throw new ArgumentException("Invalid numerical value.", nameof(Value));
		}

		/// <inheritdoc/>
		public override void ContractUpdated(Contract Contract)
		{
			base.ContractUpdated(Contract);
			this.numericalParameter = this.Parameter as NumericalParameter;
		}
	}
}
