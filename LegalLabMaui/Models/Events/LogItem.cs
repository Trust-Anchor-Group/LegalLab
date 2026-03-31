using LegalLabMaui.Models.Items;
using Microsoft.Maui.Graphics;
using Waher.Events;

namespace LegalLabMaui.Models.Events;

/// <summary>
/// Represents one item in an event log output.
/// </summary>
public class LogItem(Event Event)
    : ColorableItem(CalcForegroundColor(Event), CalcBackgroundColor(Event))
{
    private readonly Event e = Event;

    public Event Event => this.e;
    public DateTime Timestamp => this.e.Timestamp;
    public EventType Type => this.e.Type;
    public EventLevel Level => this.e.Level;
    public string Time => this.e.Timestamp.ToLongTimeString();
    public string EventId => this.e.EventId;
    public string Object => this.e.Object;
    public string Actor => this.e.Actor;
    public string Message => this.e.Message;

    private static Color CalcForegroundColor(Event Event) => Event.Type switch
    {
        EventType.Debug       => Colors.White,
        EventType.Informational => Colors.Black,
        EventType.Notice      => Colors.Black,
        EventType.Warning     => Colors.Black,
        EventType.Error       => Colors.Yellow,
        EventType.Critical    => Colors.White,
        EventType.Alert       => Colors.White,
        EventType.Emergency   => Colors.White,
        _                     => Colors.Black,
    };

    private static Color CalcBackgroundColor(Event Event) => Event.Type switch
    {
        EventType.Debug         => Color.FromArgb("#FF00008B"),  // DarkBlue
        EventType.Informational => Colors.White,
        EventType.Notice        => Color.FromArgb("#FFFFFFE0"),  // LightYellow
        EventType.Warning       => Colors.Yellow,
        EventType.Error         => Colors.Red,
        EventType.Critical      => Color.FromArgb("#FF8B0000"),  // DarkRed
        EventType.Alert         => Colors.Purple,
        EventType.Emergency     => Colors.Black,
        _                       => Colors.White,
    };
}
