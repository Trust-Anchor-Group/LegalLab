using System;
using System.Threading.Tasks;
using Waher.Runtime.Settings;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for persistant properties
	/// </summary>
	public class PersistedProperty<T> : IPersistedProperty
	{
		private T value = default;
		private DateTime scheduledSave = DateTime.MinValue;
		private bool changed = false;
		private readonly bool liveUpdates = false;
		private readonly string category;
		private readonly string name;

		/// <summary>
		/// Generic class for persistant properties
		/// </summary>
		/// <param name="Category">Property category</param>
		/// <param name="Name">Property name</param>
		/// <param name="LiveUpdates">If updates to the parameter should be persisted live.</param>
		/// <param name="DefaultValue">Default value of property</param>
		public PersistedProperty(string Category, string Name, bool LiveUpdates, T DefaultValue)
		{
			this.category = Category;
			this.name = Name;
			this.liveUpdates = LiveUpdates;
			this.value = DefaultValue;
		}

		/// <summary>
		/// Property Category
		/// </summary>
		public string Category => this.category;

		/// <summary>
		/// Property Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// If updates to the parameter should be persisted live.
		/// </summary>
		public bool LiveUpdates => this.liveUpdates;

		/// <summary>
		/// Current value of the property
		/// </summary>
		public T Value
		{
			get => this.value;
			set
			{
				if (this.value is null && value is null)
					return;

				if (this.value?.Equals(value) ?? false)
					return;

				this.value = value;
				this.changed = true;

				if (this.liveUpdates)
					PersistedModel.DelayedSave(this, ref scheduledSave);
			}
		}

		/// <summary>
		/// If the property value has changed and needs to be persisted.
		/// </summary>
		public bool Changed => this.changed;

		/// <summary>
		/// Loads the property from persisted storage.
		/// </summary>
		public async Task Load()
		{
			this.value = (T)await RuntimeSettings.GetAsync(this.category + "." + this.name, this.value);
		}

		/// <summary>
		/// Saves the property to persisted storage.
		/// </summary>
		public async Task Save()
		{
			if (this.changed)
			{
				await RuntimeSettings.SetAsync(this.category + "." + this.name, this.value);
				
				this.changed = false;
				PersistedModel.RemoveDelayedSave(ref this.scheduledSave);
			}
		}
	}
}
