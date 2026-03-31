using LegalLabMaui.Models.XmlEditor;

namespace LegalLabMaui.Views;

public partial class XmlEditorPage : ContentPage
{
    public XmlEditorPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.XmlEditorModel;
    }
}
