﻿using LegalLab.Extensions;
using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about a parameter
	/// </summary>
	public abstract class ParameterInfo : Model, INamedItem
	{
		private readonly Property<string> name;
		private readonly Property<object> description;
		private readonly Property<string> descriptionAsMarkdown;
		private readonly Property<object> value;
		private readonly Property<string> expression;
		private readonly Property<string> guide;

		private readonly Command removeParameter;
		private readonly DesignModel designModel;

		/// <summary>
		/// Contains information about a parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		public ParameterInfo(Contract Contract, Parameter Parameter, Control Control, DesignModel DesignModel)
		{
			this.name = new Property<string>(nameof(this.Name), Parameter.Name, this);
			this.description = new Property<object>(nameof(this.Description), Parameter.ToSimpleXAML(Contract.DefaultLanguage, Contract), this);
			this.descriptionAsMarkdown = new Property<string>(nameof(this.DescriptionAsMarkdown), Parameter.ToMarkdown(Contract.DefaultLanguage, Contract).Trim(), this);
			this.value = new Property<object>(nameof(this.Value), Parameter.ObjectValue, this);
			this.expression = new Property<string>(nameof(this.Expression), Parameter.Expression, this);
			this.guide = new Property<string>(nameof(this.Guide), Parameter.Guide, this);

			this.Parameter = Parameter;
			this.Control = Control;
			this.Contract = Contract;
			this.designModel = DesignModel;

			this.removeParameter = new Command(this.CanExecuteRemoveParameter, this.ExecuteRemoveParameter);
		}

		/// <summary>
		/// Original parameter object in contract
		/// </summary>
		public Contract Contract { get; }

		/// <summary>
		/// Original parameter object in contract
		/// </summary>
		public Parameter Parameter { get; }

		/// <summary>
		/// Generated control.
		/// </summary>
		public Control Control { get; }

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
		/// Description, as Markdown
		/// </summary>
		public string DescriptionAsMarkdown
		{
			get => this.descriptionAsMarkdown.Value;
			set
			{
				this.descriptionAsMarkdown.Value = value;
				this.description.Value = value.ToSimpleXAML();
			}
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

		/// <summary>
		/// Optional Validation expression
		/// </summary>
		public string Expression
		{
			get => this.expression.Value;
			set
			{
				if (!string.IsNullOrEmpty(value))
					new Expression(value);
				
				this.expression.Value = value;
			}
		}

		/// <summary>
		/// Guiding text.
		/// </summary>
		public string Guide
		{
			get => this.guide.Value;
			set => this.guide.Value = value;
		}

		/// <summary>
		/// Remove parameter command
		/// </summary>
		public ICommand RemoveParameter => this.removeParameter;

		/// <summary>
		/// If the remove parameter command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteRemoveParameter()
		{
			return !(this.designModel is null);
		}

		/// <summary>
		/// Removes the parameter.
		/// </summary>
		public void ExecuteRemoveParameter()
		{
			this.designModel?.RemoveParameter(this);
		}
	}
}
