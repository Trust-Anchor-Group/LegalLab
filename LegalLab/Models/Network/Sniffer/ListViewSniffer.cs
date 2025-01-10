using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;

namespace LegalLab.Models.Network.Sniffer
{
	/// <summary>
	/// Sniffer, that outputs items to a ListView control.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class ListViewSniffer(ListView View, int MaxCount) 
		: SnifferBase()
	{
		private readonly ListView view = View;
		private readonly int maxCount = MaxCount;

		public void Add(SniffItem Item)
		{
			MainWindow.UpdateGui(this.AddItem, Item);
		}

		private Task AddItem(object P)
		{
			if (this.view.Items.Count >= this.maxCount)
				this.view.Items.RemoveAt(0);

			this.view.Items.Add((SniffItem)P);

			return Task.CompletedTask;
		}

		public override Task Process(SnifferRxBinary Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataReceived,
				HexToString(Event.Data, Event.Offset, Event.Count),
				CloneSection(Event.Data, Event.Offset, Event.Count),
				Colors.White, Colors.Navy, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferTxBinary Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataTransmitted, 
				HexToString(Event.Data, Event.Offset, Event.Count), 
				CloneSection(Event.Data, Event.Offset, Event.Count),
				Colors.Black, Colors.White, this));
			
			return Task.CompletedTask;
		}

		internal static string HexToString(byte[] Data, int Offset, int Count)
		{
			StringBuilder Output = new();
			int i = 0;

			while (Count-- > 0)
			{
				byte b = Data[Offset++];

				if (i > 0)
					Output.Append(' ');

				Output.Append(b.ToString("X2"));

				i = (i + 1) & 31;
				if (i == 0)
					Output.AppendLine();
			}

			return Output.ToString().TrimEnd();
		}

		public override Task Process(SnifferRxText Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextReceived, 
				Event.Text, null, Colors.White, Colors.Navy, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferTxText Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextTransmitted, 
				Event.Text, null, Colors.Black, Colors.White, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferInformation Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.Information, 
				Event.Text, null, Colors.Yellow, Colors.DarkGreen, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferWarning Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.Warning, 
				Event.Text, null, Colors.Black, Colors.Yellow, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferError Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.Error, 
				Event.Text, null, Colors.White, Colors.Red, this));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferException Event)
		{
			this.Add(new SniffItem(Event.Timestamp, SniffItemType.Exception, 
				Event.Text, null, Colors.White, Colors.DarkRed, this));

			return Task.CompletedTask;
		}

		public void RaiseSelectionChanged()
		{
			this.SelectionChanged.Raise(this, EventArgs.Empty);
		}

		/// <summary>
		/// Event raised when selection changed
		/// </summary>
		public event EventHandler SelectionChanged;
	}
}
