using LegalLabMaui.Models.Tokens;

namespace LegalLabMaui.Views;

public partial class TokensPage : ContentPage
{
    public TokensPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = AppService.NetworkModel?.Tokens;
    }

    private void OnTokenSelected(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not TokensModel model)
            return;

        if (model.SelectedItem is not null)
            model.SelectedItem.IsSelected = false;

        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is TokenModel item)
            item.IsSelected = true;
    }
}
