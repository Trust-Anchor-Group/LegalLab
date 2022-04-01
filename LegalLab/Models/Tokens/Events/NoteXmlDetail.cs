using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains a XML note for a token.
	/// </summary>
	public class NoteXmlDetail : NoteDetail
	{
		/// <summary>
		/// Contains a XML note for a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public NoteXmlDetail(NoteXml Event)
			: base(Event)
		{
		}
	}
}
