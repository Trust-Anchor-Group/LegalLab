﻿using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Geo;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a geo-spatial parameter
	/// </summary>
	public class GeoParameterInfo : RangedParameterInfo
	{
		private readonly Property<GeoPosition> min;
		private readonly Property<GeoPosition> max;

		private GeoParameter geoParameter;

		/// <summary>
		/// Contains information about a geo-spatial parameter
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
		public GeoParameterInfo(Contract Contract, GeoParameter Parameter, Control Control, Control MinControl, Control MinIncludedControl,
			Control MaxControl, Control MaxIncludedControl, DesignModel DesignModel, Property<ParameterInfo[]> Parameters)
			: base(Contract, Parameter, Control, MinControl, MinIncludedControl, MaxControl, MaxIncludedControl, DesignModel, Parameters)
		{
			this.geoParameter = Parameter;

			this.min = new Property<GeoPosition>(nameof(this.Min), Parameter.Min, this);
			this.max = new Property<GeoPosition>(nameof(this.Max), Parameter.Max, this);

			this.MinIncluded = Parameter.MinIncluded;
			this.MaxIncluded = Parameter.MaxIncluded;
		}

		/// <summary>
		/// Minimum value of parameter
		/// </summary>
		public GeoPosition Min
		{
			get => this.min.Value;
			set
			{
				this.geoParameter.Min = value;
				this.min.Value = value;
				this.Revalidate();
			}
		}

		/// <summary>
		/// Maximum value of parameter
		/// </summary>
		public GeoPosition Max
		{
			get => this.max.Value;
			set
			{
				this.geoParameter.Max = value;
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
				this.geoParameter.MinIncluded = value;
				base.MinIncluded = value;
			}
		}

		/// <inheritdoc/>
		public override bool MaxIncluded
		{
			get => base.MaxIncluded;
			set
			{
				this.geoParameter.MaxIncluded = value;
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

		private static GeoPosition Parse(string Value)
		{
			if (string.IsNullOrEmpty(Value))
				return null;
			else if (GeoPosition.TryParse(Value, out GeoPosition GP))
				return GP;
			else
				throw new ArgumentException("Invalid geographical position value.", nameof(Value));
		}

		/// <inheritdoc/>
		public override void ContractUpdated(Contract Contract)
		{
			base.ContractUpdated(Contract);
			this.geoParameter = this.Parameter as GeoParameter;
		}
	}
}
