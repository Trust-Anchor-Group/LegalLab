using LegalLab.Models.Items;
using System;
using System.Windows.Media;
using Waher.Events;

namespace LegalLab.Models.Events
{
	/// <summary>
	/// Represents one item in an event log output.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class LogItem(Event Event) 
		: ColorableItem(CalcForegroundColor(Event), CalcBackgroundColor(Event))
	{
		private readonly Event e = Event;

		/// <summary>
		/// Event object
		/// </summary>
		public Event Event => this.e;

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp { get { return this.e.Timestamp; } }

		/// <summary>
		/// Event type.
		/// </summary>
		public EventType Type { get { return this.e.Type; } }

		/// <summary>
		/// Event level.
		/// </summary>
		public EventLevel Level { get { return this.e.Level; } }

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time { get { return this.e.Timestamp.ToLongTimeString(); } }

		/// <summary>
		/// Event ID
		/// </summary>
		public string EventId { get { return this.e.EventId; } }

		/// <summary>
		/// Object
		/// </summary>
		public string Object { get { return this.e.Object; } }

		/// <summary>
		/// Actor
		/// </summary>
		public string Actor { get { return this.e.Actor; } }

		/// <summary>
		/// Message
		/// </summary>
		public string Message { get { return this.e.Message; } }

		private static Color CalcForegroundColor(Event Event)
		{
			return Event.Type switch
			{
				EventType.Debug => Colors.White,
				EventType.Informational => Colors.Black,
				EventType.Notice => Colors.Black,
				EventType.Warning => Colors.Black,
				EventType.Error => Colors.Yellow,
				EventType.Critical => Colors.White,
				EventType.Alert => Colors.White,
				EventType.Emergency => Colors.White,
				_ => Colors.Black,
			};
		}

		private static Color CalcBackgroundColor(Event Event)
		{
			return Event.Type switch
			{
				EventType.Debug => Colors.DarkBlue,
				EventType.Informational => Colors.White,
				EventType.Notice => Colors.LightYellow,
				EventType.Warning => Colors.Yellow,
				EventType.Error => Colors.Red,
				EventType.Critical => Colors.DarkRed,
				EventType.Alert => Colors.Purple,
				EventType.Emergency => Colors.Black,
				_ => Colors.White,
			};
		}
	}
}
