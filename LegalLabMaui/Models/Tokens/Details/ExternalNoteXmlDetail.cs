using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains an external XML note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class ExternalNoteXmlDetail(ExternalNoteXml Event)
		: ExternalNoteDetail(Event)
	{
	}
}
