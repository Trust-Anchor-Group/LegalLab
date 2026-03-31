using NeuroFeatures.Events;

namespace LegalLabMaui.Models.Tokens.Details
{
	/// <summary>
	/// Contains information about the value of a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public abstract class ValueDetail(TokenValueEvent Event)
		: TokenEventDetail(Event)
	{
		/// <summary>
		/// Value of the token
		/// </summary>
		public decimal Value { get; } = Event.Value;

		/// <summary>
		/// Currency of the token
		/// </summary>
		public string Currency { get; } = Event.Currency;
	}
}
