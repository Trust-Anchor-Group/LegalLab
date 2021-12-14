using System;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Interface for delayed actions.
	/// </summary>
	public interface IDelayedAction
	{
		/// <summary>
		/// When action is scheduled.
		/// </summary>
		DateTime ScheduledFor
		{
			get;
			set;
		}

		/// <summary>
		/// Method called when it is time to execute action.
		/// </summary>
		Task Action();
	}
}
