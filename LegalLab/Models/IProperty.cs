using System.ComponentModel;

namespace LegalLab.Models
{
	/// <summary>
	/// Interface for properties.
	/// </summary>
	public interface IProperty
	{
		/// <summary>
		/// Property Name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Model hosting the property.
		/// </summary>
		IModel Model { get; }

		/// <summary>
		/// Untyped value of property
		/// </summary>
		object UntypedValue { get; set; }

		/// <summary>
		/// Occurs when the property value changes.
		/// </summary>
		event PropertyChangedEventHandler PropertyChanged;
	}
}
