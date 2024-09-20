using System;
using System.Threading.Tasks;
using Waher.Events;

namespace LegalLab.Models
{
	/// <summary>
	/// Generic class for persistant properties
	/// </summary>
	public class DelayedActionProperty<T> : Property<T>, IDelayedAction
	{
		private readonly bool liveUpdates = false;
		private readonly TimeSpan delay;
		private DateTime scheduledFor = DateTime.MinValue;
		private bool changed = false;

		/// <summary>
		/// Generic class for persistant properties
		/// </summary>
		/// <param name="Name">Property name</param>
		/// <param name="DefaultValue">Default value of property</param>
		/// <param name="Model">Model hosting the property</param>
		public DelayedActionProperty(string Name, TimeSpan Delay, T DefaultValue, IModel Model)
			: this(Name, Delay, true, DefaultValue, Model)
		{
		}

		/// <summary>
		/// Generic class for persistant properties
		/// </summary>
		/// <param name="Name">Property name</param>
		/// <param name="LiveUpdates">If updates to the parameter should cause delayed actions live.</param>
		/// <param name="Delay">Delay after last change, before executing action.</param>
		/// <param name="DefaultValue">Default value of property</param>
		/// <param name="Model">Model hosting the property</param>
		public DelayedActionProperty(string Name, TimeSpan Delay, bool LiveUpdates, T DefaultValue, IModel Model)
			: base(Name, DefaultValue, Model)
		{
			this.liveUpdates = LiveUpdates;
			this.delay = Delay;
		}

		/// <summary>
		/// If updates to the parameter should be persisted live.
		/// </summary>
		public bool LiveUpdates => this.liveUpdates;

		/// <summary>
		/// Current value of the property
		/// </summary>
		public override T Value
		{
			get => this.@value;
			set
			{
				if (this.@value is null && value is null)
					return;

				if (this.@value?.Equals(value) ?? false)
					return;

				this.@value = value;
				this.changed = true;

				if (this.liveUpdates)
					DelayedActions.Add(this, DateTime.Now + this.delay);

				this.Model.RaisePropertyChanged(this.Name);
			}
		}

		/// <summary>
		/// If the property value has changed and needs to be persisted.
		/// </summary>
		public bool Changed
		{
			get => this.changed;
			protected set => this.changed = value;
		}

		/// <summary>
		/// When action is scheduled.
		/// </summary>
		public DateTime ScheduledFor
		{
			get => this.scheduledFor;
			set => this.scheduledFor = value;
		}

		/// <summary>
		/// Method called when it is time to execute action.
		/// </summary>
		public virtual Task Action()
		{
			try
			{
				this.OnAction?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				this.changed = false;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised when action is executed.
		/// </summary>
		public event EventHandler OnAction;

	}
}
