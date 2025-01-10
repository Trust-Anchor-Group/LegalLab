using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains a text note for a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class NoteTextDetail(NoteText Event) 
		: NoteDetail(Event)
	{
	}
}
