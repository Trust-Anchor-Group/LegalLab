using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Timing;

namespace LegalLab.Models
{
	/// <summary>
	/// Abstract base class for persistant view models
	/// </summary>
	public abstract class PersistedModel : Model
	{
		private static readonly Scheduler scheduler = new Scheduler();

		/// <summary>
		/// Queues a property for delayed persistence.
		/// </summary>
		/// <param name="Property">Property to be persisted</param>
		/// <param name="ScheduledFor">Schedule for when the property is to be persisted.</param>
		public static void DelayedSave(IPersistedProperty Property, ref DateTime ScheduledFor)
		{
			if (ScheduledFor != DateTime.MinValue)
				scheduler.Remove(ScheduledFor);

			ScheduledFor = scheduler.Add(DateTime.Now.AddSeconds(1), SaveProperty, Property);
		}

		/// <summary>
		/// Removes a delayed save
		/// </summary>
		/// <param name="ScheduledFor">Schedule for when the property is to be persisted.</param>
		public static void RemoveDelayedSave(ref DateTime ScheduledFor)
		{
			if (ScheduledFor != DateTime.MinValue)
			{
				scheduler.Remove(ScheduledFor);
				ScheduledFor = DateTime.MinValue;
			}
		}

		private static async Task SaveProperty(object P)
		{
			IPersistedProperty Property = (IPersistedProperty)P;
			await Property.Save();
		}

		private readonly LinkedList<IPersistedProperty> properties = new LinkedList<IPersistedProperty>();

		/// <summary>
		/// Adds a persistant property
		/// </summary>
		/// <param name="Property">Property</param>
		protected void Add(IPersistedProperty Property)
		{
			this.properties.AddLast(Property);
		}

		/// <summary>
		/// Loads properties from persisted storage.
		/// </summary>
		public async Task Load()
		{
			foreach (IPersistedProperty Property in this.properties)
				await Property.Load();
		}

		/// <summary>
		/// Saves properties to persisted storage.
		/// </summary>
		public async Task Save()
		{
			foreach (IPersistedProperty Property in this.properties)
				await Property.Save();
		}
	}
}
