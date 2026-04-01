using LegalLabMaui.Models.Design;

namespace LegalLabMaui.Views;

public partial class DesignPage : ContentPage
{
    public DesignPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = AppService.DesignModel;
    }
}
