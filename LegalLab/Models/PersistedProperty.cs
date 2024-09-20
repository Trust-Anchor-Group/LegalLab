using System;
using System.Threading.Tasks;
using Waher.Runtime.Settings;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for persistant properties
	/// </summary>
	public class PersistedProperty<T> : DelayedActionProperty<T>, IPersistedProperty
	{
		private readonly string category;

		/// <summary>
		/// Generic class for persistant properties
		/// </summary>
		/// <param name="Category">Property category</param>
		/// <param name="Name">Property name</param>
		/// <param name="LiveUpdates">If updates to the parameter should be persisted live.</param>
		/// <param name="DefaultValue">Default value of property</param>
		/// <param name="Model">Model hosting the property</param>
		public PersistedProperty(string Category, string Name, bool LiveUpdates, T DefaultValue, IModel Model)
			: base(Name, TimeSpan.FromSeconds(1), LiveUpdates, DefaultValue, Model)
		{
			this.category = Category;
		}

		/// <summary>
		/// Property Category
		/// </summary>
		public string Category => this.category;

		/// <summary>
		/// Method called when it is time to execute action.
		/// </summary>
		public override async Task Action()
		{
			await this.Save();
			await base.Action();
		}

		/// <summary>
		/// Loads the property from persisted storage.
		/// </summary>
		public async Task Load()
		{
			this.@value = (T)await RuntimeSettings.GetAsync(this.category + "." + this.Name, this.@value);
		}

		/// <summary>
		/// Saves the property to persisted storage.
		/// </summary>
		public async Task Save()
		{
			if (this.Changed)
			{
				await RuntimeSettings.SetAsync(this.category + "." + this.Name, this.@value);

				this.Changed = false;
				DelayedActions.Remove(this);	// In case saved manually
			}
		}
	}
}
