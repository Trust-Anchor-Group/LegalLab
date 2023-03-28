using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Events;

namespace LegalLab.Models.Network.Events
{
	/// <summary>
	/// Event Sink, that outputs items to a ListView control.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class ListViewEventSink : EventSink
	{
		private readonly ListView view;
		private readonly int maxCount;

		public ListViewEventSink(ListView View, int MaxCount)
			: base(string.Empty)
		{
			this.view = View;
			this.maxCount = MaxCount;
		}

		public override Task Queue(Event Event)
		{
			MainWindow.UpdateGui(this.AddItem, Event);
			return Task.CompletedTask;
		}

		private Task AddItem(object P)
		{
			if (this.view.Items.Count >= this.maxCount)
				this.view.Items.RemoveAt(0);

			this.view.Items.Add(new LogItem((Event)P));

			return Task.CompletedTask;
		}
	}
}
