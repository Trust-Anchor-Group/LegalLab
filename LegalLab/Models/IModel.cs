using System.ComponentModel;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Interface for view models
	/// </summary>
	public interface IModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="PropertyName">Name of property that has changed.</param>
		void RaisePropertyChanged(string PropertyName);

		/// <summary>
		/// Starts the model.
		/// </summary>
		Task Start();

		/// <summary>
		/// Stops the model.
		/// </summary>
		Task Stop();

		/// <summary>
		/// If the model has been started.
		/// </summary>
		bool Started { get; }
	}
}
