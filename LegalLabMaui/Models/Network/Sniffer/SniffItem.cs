using LegalLabMaui.Models.Items;
using Microsoft.Maui.Graphics;

namespace LegalLabMaui.Models.Network.Sniffer;

public enum SniffItemType
{
    DataReceived, DataTransmitted, TextReceived, TextTransmitted,
    Information, Warning, Error, Exception
}

public class SniffItem(DateTime Timestamp, SniffItemType Type, string Message,
    byte[]? Data, Color ForegroundColor, Color BackgroundColor, ObservableSniffer Sniffer)
    : ColorableItem(ForegroundColor, BackgroundColor)
{
    private readonly SniffItemType type = Type;
    private readonly DateTime timestamp = Timestamp;
    private readonly string message = Message;
    private readonly byte[]? data = Data;
    private readonly ObservableSniffer sniffer = Sniffer;

    public DateTime Timestamp => this.timestamp;
    public SniffItemType Type => this.type;
    public string Time => this.timestamp.ToLongTimeString();
    public string Message => this.message;
    public byte[]? Data => this.data;
    public ObservableSniffer Sniffer => this.sniffer;

    protected override void OnDeselected()
    {
        base.OnDeselected();
        this.sniffer?.RaiseSelectionChanged();
    }

    protected override void OnSelected()
    {
        base.OnSelected();
        this.sniffer?.RaiseSelectionChanged();
    }
}
