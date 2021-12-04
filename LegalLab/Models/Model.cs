using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Waher.Events;

namespace LegalLab.Models
{
	/// <summary>
	/// Abstract base class for view models
	/// </summary>
	public abstract class Model : IModel
	{
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="PropertyName">Name of property that has changed.</param>
		public void RaisePropertyChanged(string PropertyName)
		{
			try
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public virtual Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the model.
		/// </summary>
		public virtual Task Stop()
		{
			return Task.CompletedTask;
		}
	}
}
