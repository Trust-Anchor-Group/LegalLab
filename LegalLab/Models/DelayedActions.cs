using System;
using System.Threading.Tasks;
using Waher.Runtime.Timing;

namespace LegalLab.Models
{
	/// <summary>
	/// Static class managing delayed actions.
	/// </summary>
	public static class DelayedActions
	{
		private static readonly Scheduler scheduler = new Scheduler();

		/// <summary>
		/// Queues a delayed action. If previously queued, previously queued item will be removed.
		/// </summary>
		/// <param name="Action">Action to execute.</param>
		/// <param name="When">When action is scheduled to execute.</param>
		public static void Add(IDelayedAction Action, DateTime When)
		{
			DateTime TP = Action.ScheduledFor;
			if (TP != DateTime.MinValue)
			{
				Action.ScheduledFor = DateTime.MinValue;
				scheduler.Remove(TP);
			}

			Action.ScheduledFor = scheduler.Add(When, ExecuteAction, Action);
		}

		/// <summary>
		/// Removes a delayed action
		/// </summary>
		/// <param name="Action">Action to remove.</param>
		public static void Remove(IDelayedAction Action)
		{
			if (Action.ScheduledFor != DateTime.MinValue)
			{
				Action.ScheduledFor = DateTime.MinValue;
				scheduler.Remove(Action.ScheduledFor);
			}
		}

		private static async Task ExecuteAction(object P)
		{
			IDelayedAction Action = (IDelayedAction)P;
			await Action.Action();
		}
	}
}
