using LegalLabMaui.Models.Script;

namespace LegalLabMaui.Views;

public partial class ScriptPage : ContentPage
{
    public ScriptPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.ScriptModel;
    }
}
