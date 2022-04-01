using NeuroFeatures.Events;

namespace LegalLab.Models.Tokens.Events
{
	/// <summary>
	/// Contains information about a token create event.
	/// </summary>
	public class CreatedDetail : OwnershipDetail
	{
		/// <summary>
		/// Contains information about a token create event.
		/// </summary>
		/// <param name="Event">Token Event object.</param>
		public CreatedDetail(Created Event)
			: base(Event)
		{
		}

		/// <summary>
		/// Event message
		/// </summary>
		public override string Message => "Created";
	}
}
