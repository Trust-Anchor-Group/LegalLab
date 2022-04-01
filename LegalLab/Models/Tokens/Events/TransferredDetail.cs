using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token create event.
	/// </summary>
	public class TransferredDetail : OwnershipDetail
	{
		/// <summary>
		/// Contains information about a token create event.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public TransferredDetail(Transferred Event)
			: base(Event)
		{
			this.Seller = Event.Seller;
		}

		/// <summary>
		/// Seller of token
		/// </summary>
		public string Seller { get; }

		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Transferred";
	}
}
