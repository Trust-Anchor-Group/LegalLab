using NeuroFeatures.Events;
using System;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public abstract class TokenEventDetail(TokenEvent Event)
	{
		/// <summary>
		/// If event is personal or not
		/// </summary>
		public bool Personal { get; } = Event.Personal;

		/// <summary>
		/// Timestamp of event
		/// </summary>
		public DateTime Timestamp { get; } = Event.Timestamp;

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
			else if (Event is ExternalNoteText ExternalNoteText)
				return new ExternalNoteTextDetail(ExternalNoteText);
			else if (Event is ExternalNoteXml ExternalNoteXml)
				return new ExternalNoteXmlDetail(ExternalNoteXml);
			else if (Event is Transferred Transferred)
				return new TransferredDetail(Transferred);
			else if (Event is Donated Donated)
				return new DonatedDetail(Donated);
			else if (Event is Killed Killed)
				return new KilledDetail(Killed);
			else
				return null;
		}
	}
}
