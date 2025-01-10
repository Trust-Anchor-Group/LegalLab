using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
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
