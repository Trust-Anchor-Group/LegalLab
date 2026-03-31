using LegalLabMaui.Models.Network;

namespace LegalLabMaui.Views;

public partial class NetworkPage : ContentPage
{
    public NetworkPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.NetworkModel;
    }

    private void XmppPassword_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.Password = e.NewTextValue;
    }

    private void XmppPassword2_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.Password2 = e.NewTextValue;
    }

    private void ApiKeySecret_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.ApiKeySecret = e.NewTextValue;
    }
}
