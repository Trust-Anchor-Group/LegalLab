using LegalLab.Models.Design;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a string parameter
	/// </summary>
	public abstract class RangedParameterInfo : ParameterInfo
	{
		private readonly Property<bool> minIncluded;
		private readonly Property<bool> maxIncluded;

		private readonly Control minControl;
		private readonly Control minIncludedControl;
		private readonly Control maxControl;
		private readonly Control maxIncludedControl;

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
		/// <param name="Parameters">Collection of parameters.</param>
		public RangedParameterInfo(Contract Contract, Parameter Parameter, Control Control, Control MinControl, Control MinIncludedControl, 
			Control MaxControl, Control MaxIncludedControl, DesignModel DesignModel, Property<ParameterInfo[]> Parameters) 
			: base(Contract, Parameter, Control, DesignModel, Parameters)
		{
			this.minIncluded = new Property<bool>(nameof(this.MinIncluded), false, this);
			this.maxIncluded = new Property<bool>(nameof(this.MaxIncluded), false, this);

			this.minControl = MinControl;
			this.minIncludedControl = MinIncludedControl;
			this.maxControl = MaxControl;
			this.maxIncludedControl = MaxIncludedControl;
		}

		/// <summary>
		/// If minimum value of parameter is included in range
		/// </summary>
		public virtual bool MinIncluded
		{
			get => this.minIncluded.Value;
			set
			{
				this.minIncluded.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// If minimum value of parameter is included in range
		/// </summary>
		public virtual bool MaxIncluded
		{
			get => this.maxIncluded.Value;
			set
			{
				this.maxIncluded.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Control for editing Minimum value
		/// </summary>
		public override object MinControl => this.minControl;

		/// <summary>
		/// Control for defining if Minimum value is included or not
		/// </summary>
		public override object MinIncludedControl => this.minIncludedControl;

		/// <summary>
		/// Control for editing Maximum value
		/// </summary>
		public override object MaxControl => this.maxControl;

		/// <summary>
		/// Control for defining if Maximum value is included or not
		/// </summary>
		public override object MaxIncludedControl => this.maxIncludedControl;

		/// <summary>
		/// Sets the minimum value.
		/// </summary>
		/// <param name="Value">Text representation of value.</param>
		public abstract void SetMin(string Value);

		/// <summary>
		/// Sets the maximum value.
		/// </summary>
		/// <param name="Value">Text representation of value.</param>
		public abstract void SetMax(string Value);
	}
}
