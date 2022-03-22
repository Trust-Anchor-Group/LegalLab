using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a date parameter
	/// </summary>
	public class DateParameterInfo : RangedParameterInfo
	{
		private readonly Property<DateTime?> min;
		private readonly Property<DateTime?> max;

		private DateParameter dateParameter;

		/// <summary>
		/// Contains information about a date parameter
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
		public DateParameterInfo(Contract Contract, DateParameter Parameter, Control Control, Control MinControl, Control MinIncludedControl,
			Control MaxControl, Control MaxIncludedControl, DesignModel DesignModel, Property<ParameterInfo[]> Parameters)
			: base(Contract, Parameter, Control, MinControl, MinIncludedControl, MaxControl, MaxIncludedControl, DesignModel, Parameters)
		{
			this.dateParameter = Parameter;

			this.min = new Property<DateTime?>(nameof(this.Min), Parameter.Min, this);
			this.max = new Property<DateTime?>(nameof(this.Max), Parameter.Max, this);

			this.MinIncluded = Parameter.MinIncluded;
			this.MaxIncluded = Parameter.MaxIncluded;
		}

		/// <summary>
		/// Minimum value of parameter
		/// </summary>
		public DateTime? Min
		{
			get => this.min.Value;
			set
			{
				this.dateParameter.Min = value;
				this.min.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Maximum value of parameter
		/// </summary>
		public DateTime? Max
		{
			get => this.max.Value;
			set
			{
				this.dateParameter.Max = value;
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
				this.dateParameter.MinIncluded = value;
				base.MinIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override bool MaxIncluded
		{
			get => base.MaxIncluded;
			set
			{
				this.dateParameter.MaxIncluded = value;
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

		private static DateTime? Parse(string Value)
		{
			if (string.IsNullOrEmpty(Value))
				return null;
			else if (XML.TryParse(Value, out DateTime d) && d.TimeOfDay == TimeSpan.Zero)
				return d.Date;
			else
				throw new ArgumentException("Invalid date value.", nameof(Value));
		}

		/// <inheritdoc/>
		public override void ContractUpdated(Contract Contract)
		{
			base.ContractUpdated(Contract);
			this.dateParameter = this.Parameter as DateParameter;
		}
	}
}
