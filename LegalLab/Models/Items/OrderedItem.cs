using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LegalLab.Models.Items
{
	/// <summary>
	/// Abstract base class for ordered items
	/// </summary>
	public abstract class OrderedItem : Model
	{
		private readonly IProperty items;

		private readonly Command moveUp;
		private readonly Command moveDown;

		public OrderedItem(IProperty Items)
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
		/// <returns>If the item can move up.</returns>
		public bool CanExecuteMoveUp()
		{
			if (this.items.UntypedValue is Array A)
				return Array.IndexOf(A, this) > 0;
			else
				return false;
		}

		/// <summary>
		/// Moves the item up.
		/// </summary>
		public Task ExecuteMoveUp()
		{
			if (this.items.UntypedValue is not Array Items)
				return Task.CompletedTask;

			Items = (Array)Items.Clone();

			int i = Array.IndexOf(Items, this);
			if (i <= 0)
				return Task.CompletedTask;

			object Item1 = Items.GetValue(i - 1);
			object Item2 = Items.GetValue(i);

			Items.SetValue(Item2, i - 1);
			Items.SetValue(Item1, i);

			this.items.UntypedValue = Items;

			return Task.CompletedTask;
		}

		/// <summary>
		/// If the item can be moved up.
		/// </summary>
		/// <returns>If the item can move down.</returns>
		public bool CanExecuteMoveDown()
		{
			if (this.items.UntypedValue is Array A)
				return Array.IndexOf(A, this) < A.Length - 1;
			else
				return false;
		}

		/// <summary>
		/// Moves the item up.
		/// </summary>
		public Task ExecuteMoveDown()
		{
			if (this.items.UntypedValue is not Array Items)
				return Task.CompletedTask;

			Items = (Array)Items.Clone();

			int i = Array.IndexOf(Items, this);
			if (i < 0 || i >= Items.Length - 1)
				return Task.CompletedTask;

			object Item1 = Items.GetValue(i + 1);
			object Item2 = Items.GetValue(i);

			Items.SetValue(Item2, i + 1);
			Items.SetValue(Item1, i);

			this.items.UntypedValue = Items;

			return Task.CompletedTask;
		}
	}
}
