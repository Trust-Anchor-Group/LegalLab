using System.Windows.Input;

namespace LegalLabMaui.Models.Items;

public abstract class OrderedItem : Model
{
    private readonly IProperty items;
    private readonly Command moveUp;
    private readonly Command moveDown;

    public OrderedItem(IProperty Items) : base()
    {
        this.items = Items;
        this.moveUp = new Command(this.CanExecuteMoveUp, this.ExecuteMoveUp);
        this.moveDown = new Command(this.CanExecuteMoveDown, this.ExecuteMoveDown);
    }

    public ICommand MoveUp => this.moveUp;
    public ICommand MoveDown => this.moveDown;

    public bool CanExecuteMoveUp()
    {
        if (this.items.UntypedValue is Array A)
            return Array.IndexOf(A, this) > 0;
        return false;
    }

    public Task ExecuteMoveUp()
    {
        if (this.items.UntypedValue is not Array Items)
            return Task.CompletedTask;

        Items = (Array)Items.Clone();
        int i = Array.IndexOf(Items, this);
        if (i <= 0)
            return Task.CompletedTask;

        object? Item1 = Items.GetValue(i - 1);
        object? Item2 = Items.GetValue(i);
        Items.SetValue(Item2, i - 1);
        Items.SetValue(Item1, i);
        this.items.UntypedValue = Items;
        return Task.CompletedTask;
    }

    public bool CanExecuteMoveDown()
    {
        if (this.items.UntypedValue is Array A)
            return Array.IndexOf(A, this) < A.Length - 1;
        return false;
    }

    public Task ExecuteMoveDown()
    {
        if (this.items.UntypedValue is not Array Items)
            return Task.CompletedTask;

        Items = (Array)Items.Clone();
        int i = Array.IndexOf(Items, this);
        if (i < 0 || i >= Items.Length - 1)
            return Task.CompletedTask;

        object? Item1 = Items.GetValue(i + 1);
        object? Item2 = Items.GetValue(i);
        Items.SetValue(Item2, i + 1);
        Items.SetValue(Item1, i);
        this.items.UntypedValue = Items;
        return Task.CompletedTask;
    }
}
