using System.Collections.ObjectModel;
using Waher.Events;

namespace LegalLabMaui.Models.Events;

/// <summary>
/// Event sink that populates an ObservableCollection for MAUI binding.
/// Replaces the WPF ListViewEventSink.
/// </summary>
public class ObservableEventSink(int MaxCount) : EventSink(string.Empty)
{
    private readonly int maxCount = MaxCount;
    public ObservableCollection<LogItem> Items { get; } = [];
    public ObservableCollection<LogItem> Events => this.Items;

    public override Task Queue(Event Event)
    {
        AppService.UpdateGui(this.AddItem, Event);
        return Task.CompletedTask;
    }

    private Task AddItem(object p)
    {
        if (this.Items.Count >= this.maxCount)
            this.Items.RemoveAt(0);

        this.Items.Add(new LogItem((Event)p));
        return Task.CompletedTask;
    }
}
