using LegalLab.Models.Items;
using System;
using System.Windows.Media;

namespace LegalLab.Models.Network.Sniffer
{
	public enum SniffItemType
	{
		DataReceived,
		DataTransmitted,
		TextReceived,
		TextTransmitted,
		Information,
		Warning,
		Error,
		Exception
	}

	/// <summary>
	/// Represents one item in a sniffer output.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class SniffItem : ColorableItem
	{
		private readonly SniffItemType type;
		private readonly DateTime timestamp;
		private readonly string message;
		private readonly byte[] data;
		private readonly ListViewSniffer sniffer;

		/// <summary>
		/// Represents one item in a sniffer output.
		/// From the IoTGateway project, with permission.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Type">Type of sniff record.</param>
		/// <param name="Message">Message</param>
		/// <param name="Data">Optional binary data.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		/// <param name="Sniffer">Sniffer owning the item.</param>
		public SniffItem(DateTime Timestamp, SniffItemType Type, string Message, byte[] Data, Color ForegroundColor, Color BackgroundColor,
			ListViewSniffer Sniffer)
			: base(ForegroundColor, BackgroundColor)
		{
			this.type = Type;
			this.timestamp = Timestamp;
			this.message = Message;
			this.data = Data;
			this.sniffer = Sniffer;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Sniff item type.
		/// </summary>
		public SniffItemType Type => this.type;

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time => this.timestamp.ToLongTimeString();

		/// <summary>
		/// Message
		/// </summary>
		public string Message => this.message;

		/// <summary>
		/// Optional binary data.
		/// </summary>
		public byte[] Data => this.data;

		/// <summary>
		/// Sniffer owning the item.
		/// </summary>
		public ListViewSniffer Sniffer => this.sniffer;

		/// <inheritdoc/>
		protected override void OnDeselected()
		{
			base.OnDeselected();
			this.sniffer?.RaiseSelectionChanged();
		}

		/// <inheritdoc/>
		protected override void OnSelected()
		{
			base.OnSelected();
			this.sniffer?.RaiseSelectionChanged();
		}
	}
}
