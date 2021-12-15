using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Abstract base class for persistant view models
	/// </summary>
	public abstract class PersistedModel : Model
	{
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
