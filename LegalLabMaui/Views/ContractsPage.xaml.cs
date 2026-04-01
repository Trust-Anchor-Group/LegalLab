using LegalLabMaui.Models.Legal;
using System.ComponentModel;
using System;

namespace LegalLabMaui.Views;

public partial class ContractsPage : ContentPage
{
    public ContractsPage() => InitializeComponent();

    public LegalModel? LegalModel => AppService.LegalModel;

    public TemplateReferenceModel[] Templates => this.LegalModel?.Templates ?? [];

    public TemplateReferenceModel? SelectedTemplate
    {
        get
        {
            LegalModel? legalModel = this.LegalModel;
            if (legalModel is null)
                return null;

            return Array.Find(legalModel.Templates, reference => reference.TemplateName == legalModel.ContractTemplateName);
        }
        set
        {
            if (this.LegalModel is not null)
                this.LegalModel.ContractTemplateName = value?.TemplateName ?? string.Empty;
        }
    }

    public ContractReferenceModel[] ExistingContracts => this.LegalModel?.ExistingContracts ?? [];

    public ContractReferenceModel? SelectedExistingContract
    {
        get
        {
            LegalModel? legalModel = this.LegalModel;
            if (legalModel is null)
                return null;

            return Array.Find(legalModel.ExistingContracts, reference => reference.ContractId == legalModel.ExistingContractId);
        }
        set
        {
            if (this.LegalModel is not null)
                this.LegalModel.ExistingContractId = value?.ContractId ?? string.Empty;
        }
    }

    public string TemplateSourceFeedback
    {
        get
        {
            LegalModel? legalModel = this.LegalModel;

            if (legalModel is null)
                return "Legal services are not available yet.";

            if (legalModel.IsTemplate && legalModel.CurrentContract is not null && this.SelectedTemplate is null)
                return "A remote template is currently loaded for one-time use. It is not saved in the local template list.";

            if (legalModel.Templates.Length > 0)
                return legalModel.Templates.Length == 1 ? "1 saved template is available." : $"{legalModel.Templates.Length} saved templates are available.";

            return "No saved templates are available yet. Templates are added when you propose one from Design or import one through supported wallet flows.";
        }
    }

    private async void OnFetchRemoteTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            string? contractId = await AppService.PromptUser(
                "Fetch Remote Template",
                "Enter a template contract ID to load it without saving it locally:",
                string.Empty,
                "Load",
                "Cancel");

            if (string.IsNullOrWhiteSpace(contractId) || this.LegalModel is null)
                return;

            await this.LegalModel.LoadRemoteTemplate(contractId);
            this.RefreshLegalBindings();
        }
        catch (Exception ex)
        {
            AppService.ErrorBox(ex.Message);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (AppService.LegalModel is not null)
            AppService.LegalModel.PropertyChanged += this.LegalModel_PropertyChanged;

        this.BindingContext = AppService.LegalModel?.CurrentContract;
        this.RefreshLegalBindings();
    }

    protected override void OnDisappearing()
    {
        if (AppService.LegalModel is not null)
            AppService.LegalModel.PropertyChanged -= this.LegalModel_PropertyChanged;

        base.OnDisappearing();
    }

    private void LegalModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(LegalModel.CurrentContract):
                this.BindingContext = AppService.LegalModel?.CurrentContract;
                this.RefreshLegalBindings();
                break;

            case nameof(LegalModel.Templates):
            case nameof(LegalModel.ContractTemplateName):
            case nameof(LegalModel.ExistingContracts):
            case nameof(LegalModel.ExistingContractId):
                this.RefreshLegalBindings();
                break;
        }
    }

    private void RefreshLegalBindings()
    {
        this.OnPropertyChanged(nameof(this.LegalModel));
        this.OnPropertyChanged(nameof(this.Templates));
        this.OnPropertyChanged(nameof(this.SelectedTemplate));
        this.OnPropertyChanged(nameof(this.ExistingContracts));
        this.OnPropertyChanged(nameof(this.SelectedExistingContract));
        this.OnPropertyChanged(nameof(this.TemplateSourceFeedback));
    }
}
