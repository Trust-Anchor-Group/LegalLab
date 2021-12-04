using System;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for properties
	/// </summary>
	public class Property<T>
	{
		protected T value = default;
		private readonly string name;
		private readonly IModel model;

		/// <summary>
		/// Generic class for persistant properties
		/// </summary>
		/// <param name="Category">Property category</param>
		/// <param name="Name">Property name</param>
		/// <param name="LiveUpdates">If updates to the parameter should be persisted live.</param>
		/// <param name="DefaultValue">Default value of property</param>
		/// <param name="Model">Model hosting the property</param>
		public Property(string Name, T DefaultValue, IModel Model)
		{
			this.name = Name;
			this.value = DefaultValue;
			this.model = Model;
		}

		/// <summary>
		/// Property Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Model hosting the property.
		/// </summary>
		public IModel Model => this.model;

		/// <summary>
		/// Current value of the property
		/// </summary>
		public virtual T Value
		{
			get => this.value;
			set
			{
				if (this.value is null && value is null)
					return;

				if (this.value?.Equals(value) ?? false)
					return;

				this.value = value;

				this.model.RaisePropertyChanged(this.name);
			}
		}
	}
}
