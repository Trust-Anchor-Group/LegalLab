using ContractServiceProvider = Waher.Networking.XMPP.Contracts.IServiceProvider;

namespace LegalLabMaui.Views.Dialogs;

[QueryProperty(nameof(ProvidersParam), "providers")]
[QueryProperty(nameof(DefaultCurrencyParam), "currency")]
public partial class BuyEDalerPage : ContentPage
{
    private BuyEDalerModel model;

    public ContractServiceProvider[] ProvidersParam { get; set; }
    public string DefaultCurrencyParam { get; set; }

    public BuyEDalerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (model is null && ProvidersParam is not null)
        {
            model = new BuyEDalerModel(ProvidersParam, DefaultCurrencyParam ?? string.Empty);
            BindingContext = model;
            _ = WaitAndGoBack();
        }
    }

    private async Task WaitAndGoBack()
    {
        await model.WaitForResult();
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
    }
}
