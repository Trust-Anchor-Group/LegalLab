using LegalLab.Extensions;
using LegalLab.Items;
using LegalLab.Models.Design;
using LegalLab.Models.Items;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about a parameter
	/// </summary>
	public abstract class ParameterInfo : OrderedItem, INamedItem, ITranslatable
	{
		private readonly Property<string> name;
		private readonly Property<object> description;
		private readonly Property<string> descriptionAsMarkdown;
		protected readonly Property<object> value;
		private readonly Property<string> expression;
		private readonly Property<string> guide;

		private readonly Command removeParameter;
		protected readonly DesignModel designModel;

		/// <summary>
		/// Contains information about a parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public ParameterInfo(Contract Contract, Parameter Parameter, Control Control, DesignModel DesignModel, IProperty Parameters)
			: base(Parameters)
		{
			string Language = DesignModel?.Language ?? Contract.DefaultLanguage;

			this.name = new Property<string>(nameof(this.Name), Parameter.Name, this);
			this.description = new Property<object>(nameof(this.Description), Parameter.ToSimpleXAML(Language, Contract).Result, this);
			this.descriptionAsMarkdown = new Property<string>(nameof(this.DescriptionAsMarkdown), Parameter.ToMarkdown(Language, Contract, MarkdownType.ForEditing).Trim(), this);
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
		public Contract Contract
		{
			get;
			private set;
		}

		/// <summary>
		/// Original parameter object in contract
		/// </summary>
		public Parameter Parameter 
		{ 
			get;
			private set;
		}

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
			set
			{
				this.Parameter.Name = value;
				this.name.Value = value;
			}
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
				string Language = this.designModel?.Language ?? this.Contract.DefaultLanguage;
				HumanReadableText Text = value.ToHumanReadableText(Language).Result;

				if (Text is null)
					this.Parameter.Descriptions = this.Parameter.Descriptions.Remove(Language);
				else
					this.Parameter.Descriptions = this.Parameter.Descriptions.Append(Text);

				this.descriptionAsMarkdown.Value = value;
				this.description.Value = value.ToSimpleXAML(this.Contract, Language).Result;
			}
		}

		/// <summary>
		/// Parameter value
		/// </summary>
		public virtual object Value
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
				{
					Expression _ = new(value);
				}

				this.Parameter.Expression = value;
				this.expression.Value = value;

				this.Revalidate();
			}
		}

		/// <summary>
		/// Revalidates the parameter value.
		/// </summary>
		public void Revalidate()
		{
			this.designModel?.ValidateParameters();
		}

		/// <summary>
		/// Method called when the parameter needs to be validated.
		/// </summary>
		public virtual async Task<bool> ValidateParameter(Variables Variables)
		{
			return await this.Parameter.IsParameterValid(Variables);
		}

		/// <summary>
		/// Guiding text.
		/// </summary>
		public string Guide
		{
			get => this.guide.Value;
			set
			{
				this.Parameter.Guide = value;
				this.guide.Value = value;
			}
		}

		/// <summary>
		/// Control for editing Minimum value
		/// </summary>
		public virtual object MinControl => null;

		/// <summary>
		/// Control for defining if Minimum value is included or not
		/// </summary>
		public virtual object MinIncludedControl => null;

		/// <summary>
		/// Control for editing Maximum value
		/// </summary>
		public virtual object MaxControl => null;

		/// <summary>
		/// Control for defining if Maximum value is included or not
		/// </summary>
		public virtual object MaxIncludedControl => null;

		/// <summary>
		/// Control for editing the Minimum length of the parameter
		/// </summary>
		public virtual Control MinLengthControl => null;

		/// <summary>
		/// Control for editing the Maximum length of the parameter
		/// </summary>
		public virtual Control MaxLengthControl => null;

		/// <summary>
		/// Control for editing a regular expression for validating parameter values.
		/// </summary>
		public virtual Control RegExControl => null;

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
			return this.designModel is not null;
		}

		/// <summary>
		/// Removes the parameter.
		/// </summary>
		public virtual Task ExecuteRemoveParameter()
		{
			this.designModel?.RemoveParameter(this);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="Value">Text representation of value.</param>
		public abstract void SetValue(string Value);

		/// <summary>
		/// Gets associated texts to translate.
		/// </summary>
		/// <param name="Language">Language to translate from.</param>
		/// <returns>Array of translatable texts, or null if none.</returns>
		public string[] GetTranslatableTexts(string Language)
		{
			HumanReadableText Text = this.Parameter.Descriptions.Find(Language);
			if (Text is null)
				return null;
			else
				return new string[] { Text.GenerateMarkdown(this.Contract, MarkdownType.ForEditing) };
		}

		/// <summary>
		/// Sets translated texts.
		/// </summary>
		/// <param name="Texts">Available translated texts.</param>
		/// <param name="Language">Language translated to.</param>
		public void SetTranslatableTexts(string[] Texts, string Language)
		{
			if (Texts.Length > 0)
				this.DescriptionAsMarkdown = Texts[0].Trim();
		}

		/// <summary>
		/// Called when the contract has been updated.
		/// </summary>
		/// <param name="Contract">Updated contract.</param>
		public virtual void ContractUpdated(Contract Contract)
		{
			if (this.Contract != Contract)
			{
				this.Contract = Contract;

				foreach (Parameter P in Contract.Parameters)
				{
					if (P.Name == this.name.Value)
					{
						this.Parameter = P;
						break;
					}
				}
			}
		}
	}
}
