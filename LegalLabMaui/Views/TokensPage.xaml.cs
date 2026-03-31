using LegalLabMaui.Models.Tokens;

namespace LegalLabMaui.Views;

public partial class TokensPage : ContentPage
{
    public TokensPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.NetworkModel?.Tokens;
    }

    private void OnTokenSelected(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is TokensModel model && e.CurrentSelection.Count > 0)
            model.SelectedItem = e.CurrentSelection[0] as TokenModel;
    }
}
