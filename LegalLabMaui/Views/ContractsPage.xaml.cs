using LegalLabMaui.Models.Legal;
using System.ComponentModel;

namespace LegalLabMaui.Views;

public partial class ContractsPage : ContentPage
{
    public ContractsPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (AppService.LegalModel is not null)
            AppService.LegalModel.PropertyChanged += this.LegalModel_PropertyChanged;

        this.BindingContext = AppService.LegalModel?.CurrentContract;
    }

    protected override void OnDisappearing()
    {
        if (AppService.LegalModel is not null)
            AppService.LegalModel.PropertyChanged -= this.LegalModel_PropertyChanged;

        base.OnDisappearing();
    }

    private void LegalModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LegalModel.CurrentContract))
            this.BindingContext = AppService.LegalModel?.CurrentContract;
    }
}
