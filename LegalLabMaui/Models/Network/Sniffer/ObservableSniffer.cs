using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;

namespace LegalLabMaui.Models.Network.Sniffer;

/// <summary>
/// Sniffer that populates an ObservableCollection for MAUI binding.
/// Replaces the WPF ListViewSniffer.
/// </summary>
public class ObservableSniffer(int MaxCount) : SnifferBase("Observable sniffer")
{
    private readonly int maxCount = MaxCount;
    public ObservableCollection<SniffItem> Items { get; } = [];

    public override BinaryPresentationMethod BinaryPresentationMethod => BinaryPresentationMethod.Hexadecimal;

    public void Add(SniffItem Item)
    {
        AppService.UpdateGui(this.AddItem, Item);
    }

    private Task AddItem(object P)
    {
        if (this.Items.Count >= this.maxCount)
            this.Items.RemoveAt(0);
        this.Items.Add((SniffItem)P);
        return Task.CompletedTask;
    }

    public void RaiseSelectionChanged()
    {
        this.SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SelectionChanged;

    public void Clear() => AppService.UpdateGui(() => { this.Items.Clear(); return Task.CompletedTask; });

    public override Task Process(SnifferRxBinary Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataReceived,
            HexToString(Event.Data, Event.Offset, Event.Count),
            CloneSection(Event.Data, Event.Offset, Event.Count),
            Colors.White, Color.FromArgb("#FF000080"), this));  // Navy
        return Task.CompletedTask;
    }

    public override Task Process(SnifferTxBinary Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataTransmitted,
            HexToString(Event.Data, Event.Offset, Event.Count),
            CloneSection(Event.Data, Event.Offset, Event.Count),
            Colors.Black, Colors.White, this));
        return Task.CompletedTask;
    }

    public override Task Process(SnifferRxText Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextReceived,
            Event.Text, null, Colors.White, Color.FromArgb("#FF000080"), this));
        return Task.CompletedTask;
    }

    public override Task Process(SnifferTxText Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextTransmitted,
            Event.Text, null, Colors.Black, Colors.White, this));
        return Task.CompletedTask;
    }

    public override Task Process(SnifferInformation Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.Information,
            Event.Text, null, Colors.Yellow, Color.FromArgb("#FF006400"), this)); // DarkGreen
        return Task.CompletedTask;
    }

    public override Task Process(SnifferWarning Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.Warning,
            Event.Text, null, Colors.Black, Colors.Yellow, this));
        return Task.CompletedTask;
    }

    public override Task Process(SnifferError Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.Error,
            Event.Text, null, Colors.White, Colors.Red, this));
        return Task.CompletedTask;
    }

    public override Task Process(SnifferException Event, CancellationToken Cancel)
    {
        this.Add(new SniffItem(Event.Timestamp, SniffItemType.Exception,
            Event.Text, null, Colors.White, Color.FromArgb("#FF8B0000"), this)); // DarkRed
        return Task.CompletedTask;
    }

    internal static string HexToString(byte[] Data, int Offset, int Count)
    {
        StringBuilder Output = new();
        int i = 0;
        while (Count-- > 0)
        {
            byte b = Data[Offset++];
            if (i > 0) Output.Append(' ');
            Output.Append(b.ToString("X2"));
            i = (i + 1) & 31;
            if (i == 0) Output.AppendLine();
        }
        return Output.ToString().TrimEnd();
    }

    private static byte[] CloneSection(byte[] Data, int Offset, int Count)
    {
        byte[] Result = new byte[Count];
        Array.Copy(Data, Offset, Result, 0, Count);
        return Result;
    }
}
