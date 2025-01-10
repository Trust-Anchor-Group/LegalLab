using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about the owner of a token.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public abstract class OwnershipDetail(TokenOwnershipEvent Event) 
		: ValueDetail(Event)
	{
		/// <summary>
		/// Owner of the token
		/// </summary>
		public string Owner { get; } = Event.Owner;

		/// <summary>
		/// Contract establishing ownership of thetoken
		/// </summary>
		public string OwnershipContract { get; } = Event.OwnershipContract;
	}
}
