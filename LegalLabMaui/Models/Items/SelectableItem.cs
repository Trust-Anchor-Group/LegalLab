namespace LegalLabMaui.Models.Items;

public abstract class SelectableItem : Model
{
    private bool selected = false;

    protected void Raise(EventHandler? h)
    {
        if (h != null)
        {
            try { h(this, EventArgs.Empty); }
            catch (Exception ex) { AppService.ErrorBox(ex.Message); }
        }
    }

    public bool IsSelected
    {
        get => this.selected;
        set
        {
            if (this.selected != value)
            {
                this.selected = value;
                this.RaisePropertyChanged(nameof(this.IsSelected));

                if (this.selected)
                    this.OnSelected();
                else
                    this.OnDeselected();
            }
        }
    }

    public event EventHandler? Selected;
    public event EventHandler? Deselected;

    protected virtual void OnSelected() => this.Raise(this.Selected);
    protected virtual void OnDeselected() => this.Raise(this.Deselected);
}
