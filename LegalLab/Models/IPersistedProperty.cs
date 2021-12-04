using System;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for persistant properties
	/// </summary>
	public interface IPersistedProperty
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
