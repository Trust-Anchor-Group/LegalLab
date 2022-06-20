using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains an external note for a token.
	/// </summary>
	public abstract class ExternalNoteDetail : NoteDetail
	{
		/// <summary>
		/// Contains an external note for a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public ExternalNoteDetail(TokenExternalNoteEvent Event)
			: base(Event)
		{
			this.Source = Event.Source;
		}

		/// <summary>
		/// Source of note
		/// </summary>
		public string Source { get; }
	}
}
