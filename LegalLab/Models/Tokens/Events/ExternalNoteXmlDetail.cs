using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains an external XML note for a token.
	/// </summary>
	public class ExternalNoteXmlDetail : ExternalNoteDetail
	{
		/// <summary>
		/// Contains an external XML note for a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public ExternalNoteXmlDetail(ExternalNoteXml Event)
			: base(Event)
		{
		}
	}
}
