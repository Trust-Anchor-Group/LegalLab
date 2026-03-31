namespace LegalLabMaui.Views;

public partial class LegalIdPage : ContentPage
{
    public LegalIdPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.NetworkModel?.Legal;
    }
}
