using LegalLabMaui.Models.Events;

namespace LegalLabMaui.Views;

public partial class EventsPage : ContentPage
{
    public EventsPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.EventSink;
    }
}
