using LegalLabMaui.Models.Network;
using System.Collections.Specialized;

namespace LegalLabMaui.Views;

public partial class NetworkPage : ContentPage
{
    private NetworkModel? attachedModel;

    public NetworkPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = AppService.NetworkModel;

        if (BindingContext is NetworkModel model && !ReferenceEquals(this.attachedModel, model))
        {
            if (this.attachedModel is not null)
                this.attachedModel.SnifferItems.CollectionChanged -= SnifferItems_CollectionChanged;

            this.attachedModel = model;
            this.attachedModel.SnifferItems.CollectionChanged += SnifferItems_CollectionChanged;
        }
    }

    protected override void OnDisappearing()
    {
        if (this.attachedModel is not null)
        {
            this.attachedModel.SnifferItems.CollectionChanged -= SnifferItems_CollectionChanged;
            this.attachedModel = null;
        }

        base.OnDisappearing();
    }

    private async void SnifferItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (this.attachedModel?.SnifferItems.LastOrDefault() is Models.Network.Sniffer.SniffItem last)
                SnifferCollectionView.ScrollTo(last, position: ScrollToPosition.End, animate: false);
        });
    }

    private void XmppPassword_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.Password = e.NewTextValue;
    }

    private void XmppPassword2_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.Password2 = e.NewTextValue;
    }

    private void ApiKeySecret_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is NetworkModel model)
            model.ApiKeySecret = e.NewTextValue;
    }
}
