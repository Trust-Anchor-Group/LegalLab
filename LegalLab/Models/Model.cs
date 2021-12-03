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
		public abstract Task Start();

		/// <summary>
		/// Stops the model.
		/// </summary>
		public abstract Task Stop();
	}
}
