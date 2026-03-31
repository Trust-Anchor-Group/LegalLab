using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains information about a token transfer event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class TransferredDetail(Transferred Event)
		: OwnershipDetail(Event)
	{
		/// <summary>
		/// Seller of token
		/// </summary>
		public string Seller { get; } = Event.Seller;

		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Transferred";
	}
}
