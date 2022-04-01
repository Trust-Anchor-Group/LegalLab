using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about the owner of a token.
	/// </summary>
	public abstract class OwnershipDetail : ValueDetail
	{
		/// <summary>
		/// Contains information about the owner of a token.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public OwnershipDetail(TokenOwnershipEvent Event)
			: base(Event)
		{
			this.Owner = Event.Owner;
			this.OwnershipContract = Event.OwnershipContract;
		}

		/// <summary>
		/// Owner of the token
		/// </summary>
		public string Owner { get; }

		/// <summary>
		/// Contract establishing ownership of thetoken
		/// </summary>
		public string OwnershipContract { get; }
	}
}
