using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains a note for a token.
	/// </summary>
	public abstract class NoteDetail : TokenEventDetail
	{
		/// <summary>
		/// Contains a note for a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public NoteDetail(TokenNoteEvent Event)
			: base(Event)
		{
			this.Message = Event.Note;
		}

		/// <summary>
		/// Event message
		/// </summary>
		public override string Message { get; }
	}
}
