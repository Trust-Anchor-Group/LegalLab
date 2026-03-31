using Waher.Runtime.Inventory;

namespace LegalLabMaui.Models.Window;

/// <summary>
/// Persists the selected tab index so the app restores the last active tab.
/// Window sizing is handled by the OS on MAUI.
/// </summary>
[Singleton]
public class WindowSizeModel : PersistedModel
{
    private readonly PersistedProperty<int> selectedTab;

    public WindowSizeModel()
    {
        this.Add(this.selectedTab = new PersistedProperty<int>("Window", nameof(this.SelectedTab), true, 0, this));
    }

    public int SelectedTab
    {
        get => this.selectedTab.Value;
        set => this.selectedTab.Value = value;
    }
}
