namespace LegalLabMaui.Views;

public partial class LegalIdPage : ContentPage
{
    public LegalIdPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = AppService.NetworkModel?.Legal;
    }

    private void OnIdentitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not Models.Legal.LegalModel model)
            return;

        foreach (Models.Legal.IdentityWrapper identity in model.Identities)
            identity.IsSelected = false;

        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is Models.Legal.IdentityWrapper item)
            item.IsSelected = true;
    }
}
