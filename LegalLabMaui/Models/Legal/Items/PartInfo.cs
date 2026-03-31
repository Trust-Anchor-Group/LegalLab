using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Items;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Models.Legal.Items;

public class PartInfo : OrderedItem
{
    private readonly Property<string> legalId;
    private readonly Property<string> role;
    private readonly IPartsModel partsModel;
    private readonly Command removePart;
    private readonly Part part;

    public PartInfo(Part Part, IPartsModel PartsModel, Property<PartInfo[]> Parts) : base(Parts)
    {
        this.legalId = new Property<string>(nameof(this.LegalId), Part.LegalId, this);
        this.role = new Property<string>(nameof(this.Role), Part.Role, this);
        this.partsModel = PartsModel;
        this.part = Part;
        this.removePart = new Command(this.CanExecuteRemovePart, this.ExecuteRemovePart);

        if (this.partsModel is not null)
            this.partsModel.PropertyChanged += this.PartsModel_PropertyChanged;
    }

    public Part Part => this.part;

    public string LegalId
    {
        get => this.legalId.Value;
        set { this.part.LegalId = value; this.legalId.Value = value; }
    }

    public string Role
    {
        get => this.role.Value;
        set { this.part.Role = value; this.role.Value = value; }
    }

    public string[] Roles => this.partsModel?.RoleNames ?? [];

    private void PartsModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.Roles))
            this.RaisePropertyChanged(nameof(this.Roles));
    }

    public ICommand RemovePart => this.removePart;

    public bool CanExecuteRemovePart() => this.partsModel is not null;

    public Task ExecuteRemovePart()
    {
        if (this.partsModel is not null)
        {
            this.partsModel.PropertyChanged -= this.PartsModel_PropertyChanged;
            this.partsModel.RemovePart(this);
        }
        return Task.CompletedTask;
    }
}
