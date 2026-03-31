using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Legal.Items;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items.Parameters;

/// <summary>
/// Abstract base class for ranged parameters (date, numerical, etc.)
/// </summary>
public abstract class RangedParameterInfo : ParameterInfo
{
    private readonly Property<bool> minIncluded;
    private readonly Property<bool> maxIncluded;

    protected RangedParameterInfo(Contract Contract, Parameter Parameter, DesignModel? DesignModel, IProperty Parameters)
        : base(Contract, Parameter, DesignModel, Parameters)
    {
        this.minIncluded = new Property<bool>(nameof(this.MinIncluded), false, this);
        this.maxIncluded = new Property<bool>(nameof(this.MaxIncluded), false, this);
    }

    /// <summary>
    /// Whether the minimum value is included in the valid range.
    /// </summary>
    public virtual bool MinIncluded
    {
        get => this.minIncluded.Value;
        set { this.minIncluded.Value = value; this.Revalidate(); }
    }

    /// <summary>
    /// Whether the maximum value is included in the valid range.
    /// </summary>
    public virtual bool MaxIncluded
    {
        get => this.maxIncluded.Value;
        set { this.maxIncluded.Value = value; this.Revalidate(); }
    }

    /// <summary>
    /// Sets the minimum value from its string representation.
    /// </summary>
    public abstract void SetMin(string Value);

    /// <summary>
    /// Sets the maximum value from its string representation.
    /// </summary>
    public abstract void SetMax(string Value);
}
