using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains an external text note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class ExternalNoteTextDetail(ExternalNoteText Event)
		: ExternalNoteDetail(Event)
	{
	}
}
