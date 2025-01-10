using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token create event.
	/// </summary>
	/// <param name="Event">Token Event object.</param>
	public class CreatedDetail(Created Event) 
		: OwnershipDetail(Event)
	{
		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Created";
	}
}
