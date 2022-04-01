using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about the value of a token.
	/// </summary>
	public abstract class ValueDetail : TokenEventDetail
	{
		/// <summary>
		/// Contains information about the value of a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public ValueDetail(TokenValueEvent Event)
			: base(Event)
		{
			this.Value = Event.Value;
			this.Currency = Event.Currency;
		}

		/// <summary>
		/// Value of the token
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Currency of the token
		/// </summary>
		public string Currency { get; }
	}
}
