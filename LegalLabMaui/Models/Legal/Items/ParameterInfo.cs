using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Items;
using System.Windows.Input;
using Waher.Script;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLabMaui.Models.Legal.Items;

public abstract class ParameterInfo : OrderedItem, INamedItem, ITranslatable
{
    private readonly Property<string> name;
    private readonly Property<string> description;
    private readonly Property<string> descriptionAsMarkdown;
    protected readonly Property<object?> @value;
    private readonly Property<string> expression;
    private readonly Property<string> guide;
    private readonly Property<ProtectionLevel> protection;
    private readonly Property<ParameterErrorReason?> errorReason;
    private readonly Property<string?> errorText;
    private readonly Command removeParameter;
    protected readonly DesignModel? designModel;
    private Parameter parameter;

    protected ParameterInfo(Contract Contract, Parameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Parameters)
    {
        string language = DesignModel?.Language ?? Contract.DefaultLanguage;

        this.parameter = Parameter;
        this.designModel = DesignModel;
        this.Contract = Contract;

        this.name = new Property<string>(nameof(this.Name), Parameter.Name, this);
        this.description = new Property<string>(nameof(this.Description), Parameter.ToMarkdown(language, Contract, MarkdownType.ForRendering).Result.Trim(), this);
        this.descriptionAsMarkdown = new Property<string>(nameof(this.DescriptionAsMarkdown), Parameter.ToMarkdown(language, Contract, MarkdownType.ForEditing).Result.Trim(), this);
        this.@value = new Property<object?>(nameof(this.Value), Parameter.ObjectValue, this);
        this.expression = new Property<string>(nameof(this.Expression), Parameter.Expression ?? string.Empty, this);
        this.guide = new Property<string>(nameof(this.Guide), Parameter.Guide ?? string.Empty, this);
        this.protection = new Property<ProtectionLevel>(nameof(this.Protection), Parameter.Protection, this);
        this.errorReason = new Property<ParameterErrorReason?>(nameof(this.ErrorReason), Parameter.ErrorReason, this);
        this.errorText = new Property<string?>(nameof(this.ErrorText), Parameter.ErrorText, this);

        this.removeParameter = new Command(this.CanExecuteRemoveParameter, this.ExecuteRemoveParameter);
    }

    public Contract Contract { get; private set; }

    public Parameter Parameter
    {
        get => this.parameter;
        private set
        {
            if (this.parameter != value)
            {
                this.parameter = value;
                this.ParameterObjectUpdated();
            }
        }
    }

    protected virtual void ParameterObjectUpdated()
    {
        this.Value = this.parameter.ObjectValue;
        this.ErrorReason = this.parameter.ErrorReason;
        this.ErrorText = this.parameter.ErrorText;
    }

    public string Name
    {
        get => this.name.Value;
        set { this.parameter.Name = value; this.name.Value = value; }
    }

    public string Description => this.description.Value;

    public string DescriptionAsText => this.Description;

    public string DescriptionAsMarkdown
    {
        get => this.descriptionAsMarkdown.Value;
        set
        {
            string language = this.designModel?.Language ?? this.Contract.DefaultLanguage;
            HumanReadableText? text = value.ToHumanReadableText(language).Result;
            this.parameter.Descriptions = text is null
                ? this.parameter.Descriptions.Remove(language)
                : this.parameter.Descriptions.Append(text);
            this.descriptionAsMarkdown.Value = value;
            this.description.Value = text is null
                ? string.Empty
                : (text.GenerateMarkdown(this.Contract, MarkdownType.ForRendering).Result ?? string.Empty).Trim();
        }
    }

    public object? Value
    {
        get => this.@value.Value;
        set
        {
            this.@value.Value = value;
            this.SetParameterValue(value);
            this.designModel?.ParameterValueChanged(this);
        }
    }

    protected abstract void SetParameterValue(object? value);

    internal void ContractUpdated(Contract contract)
    {
        this.Contract = contract;

        string language = this.designModel?.Language ?? contract.DefaultLanguage;
        this.description.Value = this.parameter.ToMarkdown(language, contract, MarkdownType.ForRendering).Result.Trim();
        this.descriptionAsMarkdown.Value = this.parameter.ToMarkdown(language, contract, MarkdownType.ForEditing).Result.Trim();
        this.ParameterObjectUpdated();
    }

    public string Expression
    {
        get => this.expression.Value;
        set
        {
            this.expression.Value = value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    _ = new Expression(value);
                }
                catch (Exception ex)
                {
                    // Allow incomplete expressions while the user is typing, such as a lone '-'.
                    this.ErrorText = ex.Message;
                    return;
                }
            }

            this.parameter.Expression = value;

            this.Revalidate();
        }
    }

    public void Revalidate()
    {
        this.designModel?.ValidateParameters();
    }

    public string Guide
    {
        get => this.guide.Value;
        set { this.parameter.Guide = value; this.guide.Value = value; }
    }

    public ProtectionLevel Protection
    {
        get => this.protection.Value;
        set { this.parameter.Protection = value; this.protection.Value = value; }
    }

    public string[] ProtectionLevels => Enum.GetNames<ProtectionLevel>();

    public ParameterErrorReason? ErrorReason
    {
        get => this.errorReason.Value;
        set => this.errorReason.Value = value;
    }

    public string? ErrorText
    {
        get => this.errorText.Value;
        set => this.errorText.Value = value;
    }

    public ICommand RemoveParameter => this.removeParameter;
    public bool CanExecuteRemoveParameter() => this.designModel is not null;
    public virtual Task ExecuteRemoveParameter() { this.designModel?.RemoveParameter(this); return Task.CompletedTask; }

    public virtual async Task<bool> ValidateParameter(Variables Variables)
    {
        bool result = await this.parameter.IsParameterValid(Variables, this.designModel?.Network?.Legal?.Contracts);

        this.ErrorReason = this.parameter.ErrorReason;
        this.ErrorText = this.parameter.ErrorText;

        return result;
    }

    public virtual void SetValue(string Value) { }

    public virtual async Task<string[]?> GetTranslatableTexts(string Language)
    {
        HumanReadableText? text = this.parameter.Descriptions.Find(Language, null);
        if (text is null) return null;
        return [await text.GenerateMarkdown(this.Contract, MarkdownType.ForEditing)];
    }

    public virtual void SetTranslatableTexts(string[] Texts, string Language)
    {
        if (Texts.Length > 0) this.DescriptionAsMarkdown = Texts[0].Trim();
    }
}
