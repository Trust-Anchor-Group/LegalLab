using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains information about a token kill event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class KilledDetail(Killed Event)
		: OwnershipDetail(Event)
	{
		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Killed";
	}
}
