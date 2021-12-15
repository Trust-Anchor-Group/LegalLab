using System;
using System.Windows.Input;

namespace LegalLab.Models.Items
{
	/// <summary>
	/// Abstract base class for ordered items
	/// </summary>
	public abstract class OrderedItem<T> : Model
	{
		private readonly Property<T[]> items;

		private readonly Command moveUp;
		private readonly Command moveDown;

		public OrderedItem(Property<T[]> Items)
			: base()
		{
			this.items = Items;

			this.moveUp = new Command(this.CanExecuteMoveUp, this.ExecuteMoveUp);
			this.moveDown = new Command(this.CanExecuteMoveDown, this.ExecuteMoveDown);
		}

		/// <summary>
		/// Moves the item up in the ordered list
		/// </summary>
		public ICommand MoveUp => this.moveUp;

		/// <summary>
		/// Moves the item down in the ordered list
		/// </summary>
		public ICommand MoveDown => this.moveDown;

		/// <summary>
		/// If the item can be moved up.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteMoveUp()
		{
			return Array.IndexOf(this.items.Value, this) > 0;
		}

		/// <summary>
		/// Moves the item up.
		/// </summary>
		public void ExecuteMoveUp()
		{
			T[] Items = (T[])this.items.Value.Clone();
			int i = Array.IndexOf(Items, this);
			if (i <= 0)
				return;

			T Temp = Items[i - 1];
			Items[i - 1] = Items[i];
			Items[i] = Temp;

			this.items.Value = Items;
		}

		/// <summary>
		/// If the item can be moved up.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteMoveDown()
		{
			T[] Items = this.items.Value;
			int i = Array.IndexOf(Items, this);
			return i >= 0 && i < Items.Length - 1;
		}

		/// <summary>
		/// Moves the item up.
		/// </summary>
		public void ExecuteMoveDown()
		{
			T[] Items = (T[])this.items.Value.Clone();
			int i = Array.IndexOf(Items, this);
			if (i < 0 || i >= Items.Length - 1)
				return;

			T Temp = Items[i + 1];
			Items[i + 1] = Items[i];
			Items[i] = Temp;

			this.items.Value = Items;
		}

	}
}
