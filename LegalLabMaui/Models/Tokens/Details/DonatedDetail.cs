using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains information about a token donation event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class DonatedDetail(Donated Event)
		: OwnershipDetail(Event)
	{
		/// <summary>
		/// Donor of token
		/// </summary>
		public string Donor { get; } = Event.Donor;

		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Donated";
	}
}
