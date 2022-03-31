using System;

namespace LegalLab.Models.Items
{
	/// <summary>
	/// Abstract base class for selectable items.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public abstract class SelectableItem : Model
	{
		private bool selected = false;

		/// <summary>
		/// Abstract base class for selectable items.
		/// From the IoTGateway project, with permission.
		/// </summary>
		public SelectableItem()
		{
		}

		protected void Raise(EventHandler h)
		{
			if (h != null)
			{
				try
				{
					h(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
			}
		}

		/// <summary>
		/// If the node is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return this.selected; }
			set
			{
				if (this.selected != value)
				{
					this.selected = value;
					this.RaisePropertyChanged(nameof(IsSelected));

					if (this.selected)
						this.OnSelected();
					else
						this.OnDeselected();
				}
			}
		}

		/// <summary>
		/// Event raised when the node has been selected.
		/// </summary>
		public event EventHandler Selected = null;

		/// <summary>
		/// Event raised when the node has been deselected.
		/// </summary>
		public event EventHandler Deselected = null;

		/// <summary>
		/// Raises the <see cref="Selected"/> event.
		/// </summary>
		protected virtual void OnSelected()
		{
			this.Raise(this.Selected);
		}

		/// <summary>
		/// Raises the <see cref="Deselected"/> event.
		/// </summary>
		protected virtual void OnDeselected()
		{
			this.Raise(this.Deselected);
		}
	}
}
