using System;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Interface for persistant properties
	/// </summary>
	public interface IPersistedProperty : IDelayedAction
	{
		/// <summary>
		/// Loads the property from persisted storage.
		/// </summary>
		Task Load();

		/// <summary>
		/// Saves the property to persisted storage.
		/// </summary>
		Task Save();
	}
}
