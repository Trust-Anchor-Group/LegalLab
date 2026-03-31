namespace LegalLabMaui.Views.Dialogs;

public partial class PromptPage : ContentPage
{
    private PromptModel model;

    public PromptPage()
    {
        InitializeComponent();
        model = new PromptModel();
        BindingContext = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = Task.Run(async () =>
        {
            // Give the page a moment to fully appear, then delegate to the shell prompt
            await Task.Delay(100);
            string? result = await AppService.PromptUser(model.Title, model.Label, model.Text, model.OkText, model.CancelText);
            if (result is not null)
                model.Text = result;
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
        });
    }
}
