using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains a note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public abstract class NoteDetail(TokenNoteEvent Event) 
		: TokenEventDetail(Event)
	{
		/// <summary>
		/// Event message
		/// </summary>
		public override string Message { get; } = Event.Note;
	}
}
