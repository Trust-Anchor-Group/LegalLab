namespace LegalLabMaui.Views.Dialogs;

[QueryProperty(nameof(DefaultCurrencyParam), "currency")]
public partial class TransferEDalerPage : ContentPage
{
    private TransferEDalerModel model;

    public string DefaultCurrencyParam { get; set; }

    public TransferEDalerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (model is null)
        {
            model = new TransferEDalerModel(DefaultCurrencyParam ?? string.Empty);
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
