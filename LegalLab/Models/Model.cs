using System;
using System.Threading.Tasks;

namespace LegalLab.Models
{
	/// <summary>
	/// Abstract base class for view models
	/// </summary>
	public abstract class Model
	{
		/// <summary>
		/// Starts the model.
		/// </summary>
		/// <returns></returns>
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
