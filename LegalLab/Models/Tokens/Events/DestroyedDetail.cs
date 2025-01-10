using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token destroy event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class DestroyedDetail(Destroyed Event) 
		: OwnershipDetail(Event)
	{
		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Destroyed";
	}
}
