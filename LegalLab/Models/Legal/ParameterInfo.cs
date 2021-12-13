using LegalLab.Extensions;
using System;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contains information about a parameter
	/// </summary>
	public class ParameterInfo : Model
	{
		private readonly Property<string> name;
		private readonly Property<object> description;
		private readonly Property<object> value;

		/// <summary>
		/// Contains information about a parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		public ParameterInfo(Contract Contract, Parameter Parameter, Control Control)
		{
			this.name = new Property<string>(nameof(this.Name), Parameter.Name, this);
			this.description = new Property<object>(nameof(this.Description), Parameter.ToSimpleXAML(Contract.DefaultLanguage, Contract), this);
			this.value = new Property<object>(nameof(this.Value), Parameter.ObjectValue, this);

			this.Parameter = Parameter;
			this.Control = Control;
		}

		/// <summary>
		/// Original parameter object in contract
		/// </summary>
		public Parameter Parameter { get; internal set; }

		/// <summary>
		/// Generated control.
		/// </summary>
		public Control Control { get; internal set; }

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get => this.name.Value;
			set => this.name.Value = value;
		}

		/// <summary>
		/// Description (formatted) of the parameter.
		/// </summary>
		public object Description
		{
			get => this.description.Value;
			set => this.description.Value = value;
		}

		/// <summary>
		/// Parameter value
		/// </summary>
		public object Value
		{
			get => this.value.Value;
			set
			{
				this.Parameter.SetValue(value);
				this.value.Value = value;
			}
		}
	}
}
