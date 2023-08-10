using System.ComponentModel;

namespace LegalLab.Models
{
	/// <summary>
	/// Interface for properties.
	/// </summary>
	public interface IProperty: INotifyPropertyChanged
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
	}
}
