namespace LegalLabMaui.Views.Dialogs;

public partial class AddLanguagePage : ContentPage
{
    private readonly AddLanguageModel model;

    public AddLanguagePage()
    {
        InitializeComponent();
        model = new AddLanguageModel();
        BindingContext = model;
        _ = WaitAndGoBack();
    }

    private async Task WaitAndGoBack()
    {
        await model.WaitForResult();
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
    }
}
