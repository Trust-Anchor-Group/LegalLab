using LegalLabMaui.Models.Wallet;

namespace LegalLabMaui.Views;

public partial class WalletPage : ContentPage
{
    public WalletPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.WalletModel;
    }

    private void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is WalletModel model && e.CurrentSelection.Count > 0)
            model.SelectedItem = e.CurrentSelection[0] as AccountEventWrapper;
    }
}
