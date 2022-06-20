using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains an external text note for a token.
	/// </summary>
	public class ExternalNoteTextDetail : ExternalNoteDetail
	{
		/// <summary>
		/// Contains an external text note for a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public ExternalNoteTextDetail(ExternalNoteText Event)
			: base(Event)
		{
		}
	}
}
