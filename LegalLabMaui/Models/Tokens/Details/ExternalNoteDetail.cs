using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains an external note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public abstract class ExternalNoteDetail(TokenExternalNoteEvent Event)
		: NoteDetail(Event)
	{
		/// <summary>
		/// Source of note
		/// </summary>
		public string Source { get; } = Event.Source;
	}
}
