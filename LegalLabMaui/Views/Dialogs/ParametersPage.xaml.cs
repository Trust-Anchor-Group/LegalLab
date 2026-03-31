using LegalLabMaui.Models.Legal;

namespace LegalLabMaui.Views.Dialogs;

[QueryProperty(nameof(ModelParam), "model")]
[QueryProperty(nameof(TitleParam), "title")]
public partial class ParametersPage : ContentPage
{
    private ContractParametersModel parametersModel;
    private TaskCompletionSource<bool> tcs = new();

    public ContractParametersModel ModelParam
    {
        set
        {
            parametersModel = value;
            BindingContext = value;
        }
    }

    public string TitleParam
    {
        set => Title = value;
    }

    public ParametersPage()
    {
        InitializeComponent();
    }

    private async void OkButton_Clicked(object sender, EventArgs e)
    {
        if (parametersModel?.ParametersOk == true)
        {
            tcs.TrySetResult(true);
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        tcs.TrySetResult(false);
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Waits for the user to confirm or cancel.
    /// </summary>
    public Task<bool> WaitForResult() => tcs.Task;
}
