using LegalLabMaui.Views;
using LegalLabMaui.Views.Dialogs;

namespace LegalLabMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(PromptPage), typeof(PromptPage));
        Routing.RegisterRoute(nameof(AddLanguagePage), typeof(AddLanguagePage));
        Routing.RegisterRoute(nameof(AddXmlNotePage), typeof(AddXmlNotePage));
        Routing.RegisterRoute(nameof(BuyEDalerPage), typeof(BuyEDalerPage));
        Routing.RegisterRoute(nameof(SellEDalerPage), typeof(SellEDalerPage));
        Routing.RegisterRoute(nameof(TransferEDalerPage), typeof(TransferEDalerPage));
        Routing.RegisterRoute(nameof(ParametersPage), typeof(ParametersPage));
    }
}
