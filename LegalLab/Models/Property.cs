using System;
using System.ComponentModel;
using Waher.Events;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for properties
	/// </summary>
	public class Property<T> : IProperty
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
		/// Untyped value of property
		/// </summary>
		public object UntypedValue
		{
			get => this.Value;
			set
			{
				if (value is T Typed)
					this.Value = Typed;
				else
					throw new ArgumentException(nameof(this.UntypedValue), "Expected value of type " + typeof(T).FullName);
			}
		}

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

				PropertyChangedEventHandler h = this.PropertyChanged;
				if (h is not null)
				{
					try
					{
						h.Invoke(this, new PropertyChangedEventArgs(this.name));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

				this.model.RaisePropertyChanged(this.name);
			}
		}

		/// <summary>
		/// Occurs when the property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
