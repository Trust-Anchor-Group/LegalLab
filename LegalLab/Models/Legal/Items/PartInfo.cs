using LegalLab.Models.Design;
using LegalLab.Models.Items;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about a part in the contract
	/// </summary>
	public class PartInfo : OrderedItem<PartInfo>
	{
		private readonly Property<string> legalId;
		private readonly Property<string> role;
		private readonly IPartsModel partsModel;

		private readonly Command removePart;

		private readonly Part part;

		/// <summary>
		/// Contains information about a part in the contract
		/// </summary>
		/// <param name="Part">Part</param>
		/// <param name="PartsModel">Parts model</param>
		/// <param name="Parts">Collection of parts.</param>
		public PartInfo(Part Part, IPartsModel PartsModel, Property<PartInfo[]> Parts)
			: base(Parts)
		{
			this.legalId = new Property<string>(nameof(this.LegalId), Part.LegalId, this);
			this.role = new Property<string>(nameof(this.Role), Part.Role, this);

			this.partsModel = PartsModel;
			this.part = Part;

			this.removePart = new Command(this.CanExecuteRemovePart, this.ExecuteRemovePart);

			if (!(this.partsModel is null))
				this.partsModel.PropertyChanged += DesignModel_PropertyChanged;
		}

		/// <summary>
		/// Part reference object.
		/// </summary>
		public Part Part => this.part;

		/// <summary>
		/// Legal ID
		/// </summary>
		public string LegalId
		{
			get => this.legalId.Value;
			set
			{
				this.part.LegalId = value;
				this.legalId.Value = value;
			}
		}

		/// <summary>
		/// Role
		/// </summary>
		public string Role
		{
			get => this.role.Value;
			set
			{
				this.part.Role = value;
				this.role.Value = value;
			}
		}

		/// <summary>
		/// Roles
		/// </summary>
		public string[] Roles
		{
			get
			{
				RoleInfo[] Roles = this.partsModel.Roles;

				if (Roles is null)
					return new string[0];

				int i, c = Roles.Length;
				string[] Result = new string[c];

				for (i = 0; i < c; i++)
					Result[i] = Roles[i].Name;

				return Result;
			}
		}

		private void DesignModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Roles))
				this.RaisePropertyChanged(nameof(Roles));
		}

		/// <summary>
		/// Remove part command
		/// </summary>
		public ICommand RemovePart => this.removePart;

		/// <summary>
		/// If the remove part command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteRemovePart()
		{
			return !(this.partsModel is null);
		}

		/// <summary>
		/// Removes the part.
		/// </summary>
		public Task ExecuteRemovePart()
		{
			if (!(this.partsModel is null))
			{
				this.partsModel.PropertyChanged -= DesignModel_PropertyChanged;
				this.partsModel.RemovePart(this);
			}
	
			return Task.CompletedTask;
		}
	}
}
