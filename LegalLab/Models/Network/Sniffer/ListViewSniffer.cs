using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Waher.Networking.Sniffers;

namespace LegalLab.Models.Network.Sniffer
{
	/// <summary>
	/// Sniffer, that outputs items to a ListView control.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class ListViewSniffer : SnifferBase
	{
		private readonly ListView view;
		private readonly int maxCount;

		public ListViewSniffer(ListView View, int MaxCount)
			: base()
		{
			this.view = View;
			this.maxCount = MaxCount;
		}

		public void Add(SniffItem Item)
		{
			MainWindow.UpdateGui(this.AddItem, Item);
		}

		private void AddItem(object P)
		{
			if (this.view.Items.Count >= this.maxCount)
				this.view.Items.RemoveAt(0);

			this.view.Items.Add((SniffItem)P);
			this.view.ScrollIntoView(P);
		}

		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.DataTransmitted, HexToString(Data), Data, Colors.Black, Colors.White));
		}

		internal static string HexToString(byte[] Data)
		{
			StringBuilder Output = new StringBuilder();
			int i = 0;

			foreach (byte b in Data)
			{
				if (i > 0)
					Output.Append(' ');

				Output.Append(b.ToString("X2"));

				i = (i + 1) & 31;
				if (i == 0)
					Output.AppendLine();
			}

			return Output.ToString().TrimEnd();
		}

		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		public override void Information(DateTime Timestamp, string Comment)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		public override void Error(DateTime Timestamp, string Error)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.Add(new SniffItem(Timestamp, SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
