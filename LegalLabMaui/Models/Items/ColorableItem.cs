using Microsoft.Maui.Graphics;

namespace LegalLabMaui.Models.Items;

/// <summary>
/// Abstract base class for selectable colorable item.
/// </summary>
public abstract class ColorableItem(Color ForegroundColor, Color BackgroundColor) : SelectableItem
{
    private Color foregroundColor = ForegroundColor;
    private Color backgroundColor = BackgroundColor;

    public Color ForegroundColor
    {
        get => this.foregroundColor;
        set => this.foregroundColor = value;
    }

    public Color BackgroundColor
    {
        get => this.backgroundColor;
        set => this.backgroundColor = value;
    }

    /// <summary>
    /// Foreground color as hex string for MAUI binding.
    /// </summary>
    public string ForegroundColorHex => this.foregroundColor.ToHex();

    /// <summary>
    /// Background color as hex string for MAUI binding.
    /// </summary>
    public string BackgroundColorHex => this.backgroundColor.ToHex();
}
