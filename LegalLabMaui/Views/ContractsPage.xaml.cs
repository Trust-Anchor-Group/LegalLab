using LegalLabMaui.Models.Legal;

namespace LegalLabMaui.Views;

public partial class ContractsPage : ContentPage
{
    public ContractsPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.LegalModel?.CurrentContract;
    }
}
