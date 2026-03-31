namespace LegalLabMaui.Views.Dialogs;

public partial class AddXmlNotePage : ContentPage
{
    private readonly AddXmlNoteModel model;

    public AddXmlNotePage()
    {
        InitializeComponent();
        model = new AddXmlNoteModel();
        BindingContext = model;
        _ = WaitAndGoBack();
    }

    private async Task WaitAndGoBack()
    {
        await model.WaitForResult();
        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));
    }
}
