using System.Collections;

namespace LegalLabMaui.Models;

/// <summary>
/// Comparer that reverses the default sort order.
/// </summary>
public class ReverseOrder<T>(IComparer<T> Inner) : IComparer<T>, IComparer
{
    private readonly IComparer<T> inner = Inner;

    public ReverseOrder()
        : this(Comparer<T>.Default)
    {
    }

    public int Compare(T? x, T? y) => this.inner.Compare(y, x);

    public int Compare(object? x, object? y)
    {
        if (x is T tx && y is T ty)
            return this.Compare(tx, ty);
        return 0;
    }
}
