using NeuroFeatures.Events;
using System;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token event.
	/// </summary>
	public abstract class TokenEventDetail
	{
		/// <summary>
		/// Contains information about a token event.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public TokenEventDetail(TokenEvent Event)
		{
			this.Personal = Event.Personal;
			this.Timestamp = Event.Timestamp;
		}

		/// <summary>
		/// If event is personal or not
		/// </summary>
		public bool Personal { get; }

		/// <summary>
		/// Timestamp of event
		/// </summary>
		public DateTime Timestamp { get; }

		/// <summary>
		/// Event message
		/// </summary>
		public abstract string Message { get; }

		public static TokenEventDetail Create(TokenEvent Event)
		{
			if (Event is Created Created)
				return new CreatedDetail(Created);
			else if (Event is Destroyed Destroyed)
				return new DestroyedDetail(Destroyed);
			else if (Event is NoteText NoteText)
				return new NoteTextDetail(NoteText);
			else if (Event is NoteXml NoteXml)
				return new NoteXmlDetail(NoteXml);
			else if (Event is Transferred Transferred)
				return new TransferredDetail(Transferred);
			else
				return null;
		}
	}
}
