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
        if (BindingContext is not WalletModel model)
            return;

        if (model.SelectedItem is not null)
            model.SelectedItem.IsSelected = false;

        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is AccountEventWrapper item)
            item.IsSelected = true;
    }
}
