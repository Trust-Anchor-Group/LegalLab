using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains a XML note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class NoteXmlDetail(NoteXml Event) 
		: NoteDetail(Event)
	{
	}
}
