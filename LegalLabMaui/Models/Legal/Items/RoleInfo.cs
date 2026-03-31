using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Items;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLabMaui.Models.Legal.Items;

public class RoleInfo : OrderedItem, INamedItem, ITranslatable
{
    private readonly ContractModel? contractModel;
    private readonly DesignModel? designModel;
    private readonly Contract contract;
    private readonly Property<string> name;
    private readonly Property<string> description;
    private readonly Property<string> descriptionAsMarkdown;
    private readonly Property<int> minCount;
    private readonly Property<int> maxCount;
    private readonly Property<bool> canRevoke;
    private readonly Command signAsRole;
    private readonly Command proposeForRole;
    private readonly Command removeRole;
    private readonly Role role;

    public RoleInfo(ContractModel ContractModel, Role Role, Property<RoleInfo[]> Roles)
        : this(ContractModel, null, ContractModel.Contract, Role, Roles) { }

    public RoleInfo(DesignModel DesignModel, Role Role, Property<RoleInfo[]> Roles)
        : this(null, DesignModel, DesignModel.Contract, Role, Roles) { }

    private RoleInfo(ContractModel? ContractModel, DesignModel? DesignModel, Contract Contract, Role Role, Property<RoleInfo[]> Roles)
        : base(Roles)
    {
        this.contractModel = ContractModel;
        this.designModel = DesignModel;
        this.contract = Contract;
        this.role = Role;

        string language = this.designModel?.Language ?? this.contractModel?.Language ?? Contract.DefaultLanguage;

        this.name = new Property<string>(nameof(this.Name), Role.Name, this);
        this.description = new Property<string>(nameof(this.Description), Role.ToMarkdown(language, Contract, MarkdownType.ForRendering).Result.Trim(), this);
        this.descriptionAsMarkdown = new Property<string>(nameof(this.DescriptionAsMarkdown), Role.ToMarkdown(language, Contract, MarkdownType.ForEditing).Result.Trim(), this);
        this.minCount = new Property<int>(nameof(this.MinCount), Role.MinCount, this);
        this.maxCount = new Property<int>(nameof(this.MaxCount), Role.MaxCount, this);
        this.canRevoke = new Property<bool>(nameof(this.CanRevoke), Role.CanRevoke, this);

        this.signAsRole = new Command(this.CanExecuteSignAsRole, this.ExecuteSignAsRole);
        this.proposeForRole = new Command(this.CanExecuteProposeForRole, this.ExecuteProposeForRole);
        this.removeRole = new Command(this.CanExecuteRemoveRole, this.ExecuteRemoveRole);
    }

    public Role Role => this.role;

    public string Name
    {
        get => this.name.Value;
        set { this.role.Name = value; this.name.Value = value; }
    }

    public string Description => this.description.Value;

    public string DescriptionAsText => this.Description;

    public string DescriptionAsMarkdown
    {
        get => this.descriptionAsMarkdown.Value;
        set
        {
            string language = this.designModel?.Language ?? this.contractModel?.Language ?? this.contract.DefaultLanguage;
            HumanReadableText? text = value.ToHumanReadableText(language).Result;
            this.role.Descriptions = text is null ? this.role.Descriptions.Remove(language) : this.role.Descriptions.Append(text);
            this.descriptionAsMarkdown.Value = value;
            this.description.Value = text is null
                ? string.Empty
                : (text.GenerateMarkdown(this.contract, MarkdownType.ForRendering).Result ?? string.Empty).Trim();
        }
    }

    public int MinCount
    {
        get => this.minCount.Value;
        set { this.role.MinCount = value; this.minCount.Value = value; }
    }

    public int MaxCount
    {
        get => this.maxCount.Value;
        set { this.role.MaxCount = value; this.maxCount.Value = value; }
    }

    public bool CanRevoke
    {
        get => this.canRevoke.Value;
        set { this.role.CanRevoke = value; this.canRevoke.Value = value; }
    }

    public override void RaisePropertyChanged(string PropertyName)
    {
        base.RaisePropertyChanged(PropertyName);
        this.designModel?.RaisePropertyChanged(nameof(this.designModel.Roles));
    }

    internal void CanBeSignedChanged()
    {
        this.signAsRole.RaiseCanExecuteChanged();
        this.proposeForRole.RaiseCanExecuteChanged();
    }

    public ICommand SignAsRole => this.signAsRole;
    public bool CanBeSigned => this.contractModel?.CanBeSigned ?? false;
    public bool CanExecuteSignAsRole() => this.contractModel?.CanBeSigned ?? false;
    public async Task ExecuteSignAsRole()
    {
        try { if (this.contractModel is not null) await this.contractModel.SignAsRole(this.Name); }
        catch (Exception ex) { AppService.ErrorBox(ex.Message); }
    }

    public ICommand ProposeForRole => this.proposeForRole;
    public bool CanExecuteProposeForRole() => this.contractModel?.CanBeSigned ?? false;
    public async Task ExecuteProposeForRole()
    {
        if (this.contractModel is not null) await this.contractModel.ProposeForRole(this.Name);
    }

    public ICommand RemoveRole => this.removeRole;
    public bool CanExecuteRemoveRole() => this.designModel is not null;
    public Task ExecuteRemoveRole() { this.designModel?.RemoveRole(this); return Task.CompletedTask; }

    public async Task<string[]?> GetTranslatableTexts(string Language)
    {
        HumanReadableText? text = this.role.Descriptions.Find(Language, null);
        if (text is null) return null;
        return [await text.GenerateMarkdown(this.contract, MarkdownType.ForEditing)];
    }

    public void SetTranslatableTexts(string[] Texts, string Language)
    {
        if (Texts.Length > 0) this.DescriptionAsMarkdown = Texts[0].Trim();
    }
}
