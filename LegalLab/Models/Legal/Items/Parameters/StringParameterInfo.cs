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
		private readonly Property<int?> maxLength;
		private readonly Property<int?> minLength;
		private readonly Property<string> regEx;

		private readonly StringParameter stringParameter;

		private readonly Control minLengthControl;
		private readonly Control maxLengthControl;
		private readonly Control regExControl;

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
		/// <param name="MinLengthControl">Control for editing the minimum length of the parameter.</param>
		/// <param name="MaxLengthControl">Control for editing the maximum length of the parameter.</param>
		/// <param name="RegExControl">Control for editing a regular expression for validating the parameter.</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public StringParameterInfo(Contract Contract, StringParameter Parameter, Control Control, Control MinControl, Control MinIncludedControl, 
			Control MaxControl, Control MaxIncludedControl, Control MinLengthControl, Control MaxLengthControl, Control RegExControl, 
			DesignModel DesignModel, Property<ParameterInfo[]> Parameters) 
			: base(Contract, Parameter, Control, MinControl, MinIncludedControl, MaxControl, MaxIncludedControl, DesignModel, Parameters)
		{
			this.stringParameter = Parameter;
		
			this.min = new Property<string>(nameof(this.Min), Parameter.Min, this);
			this.max = new Property<string>(nameof(this.Max), Parameter.Max, this);
			this.minLength = new Property<int?>(nameof(this.MinLength), Parameter.MinLength, this);
			this.maxLength = new Property<int?>(nameof(this.MaxLength), Parameter.MaxLength, this);
			this.regEx = new Property<string>(nameof(this.RegEx), Parameter.RegEx, this);

			this.minLengthControl = MinLengthControl;
			this.maxLengthControl = MaxLengthControl;
			this.regExControl = RegExControl;

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
				base.MaxIncluded = value;
			}
		}

		/// <summary>
		/// Minimum length of parameter
		/// </summary>
		public int? MinLength
		{
			get => this.minLength.Value;
			set
			{
				this.stringParameter.MinLength = value;
				this.minLength.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Maximum length of parameter
		/// </summary>
		public int? MaxLength
		{
			get => this.maxLength.Value;
			set
			{
				this.stringParameter.MaxLength = value;
				this.maxLength.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Regular expression validating parameter input
		/// </summary>
		public string RegEx
		{
			get => this.regEx.Value;
			set
			{
				this.stringParameter.RegEx = value;
				this.regEx.Value = value;
				this.Revalidate();
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

		/// <summary>
		/// Control for editing the Minimum length of the parameter
		/// </summary>
		public override Control MinLengthControl => this.minLengthControl;

		/// <summary>
		/// Control for editing the Maximum length of the parameter
		/// </summary>
		public override Control MaxLengthControl => this.maxLengthControl;

		/// <summary>
		/// Control for editing a regular expression for validating parameter values.
		/// </summary>
		public override Control RegExControl => this.regExControl;

	}
}
