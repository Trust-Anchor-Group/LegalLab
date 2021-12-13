using System;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Interface for named items
	/// </summary>
	public interface INamedItem
	{
		/// <summary>
		/// Name of item
		/// </summary>
		string Name { get; }
	}
}
