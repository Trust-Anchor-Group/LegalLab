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
        bool confirmed = await model.WaitForResult();
        if (confirmed)
            AppService.DesignModel?.OnLanguageAdded(model.SelectedLanguage);

        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
    }
}
