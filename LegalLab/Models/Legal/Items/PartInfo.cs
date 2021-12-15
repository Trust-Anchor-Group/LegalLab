using LegalLab.Models.Design;
using LegalLab.Models.Items;
using System;
using System.Windows.Input;

namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains information about a part in the contract
	/// </summary>
	public class PartInfo : OrderedItem<PartInfo>
	{
		private readonly Property<string> legalId;
		private readonly Property<string> role;
		private readonly DesignModel designModel;

		private readonly Command removePart;

		/// <summary>
		/// Contains information about a part in the contract
		/// </summary>
		/// <param name="LegalId">Legal ID</param>
		/// <param name="Role">Role</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parts">Collection of parts.</param>
		public PartInfo(string LegalId, string Role, DesignModel DesignModel, Property<PartInfo[]> Parts)
			: base(Parts)
		{
			this.legalId = new Property<string>(nameof(this.LegalId), LegalId, this);
			this.role = new Property<string>(nameof(this.Role), Role, this);

			this.designModel = DesignModel;

			this.removePart = new Command(this.CanExecuteRemovePart, this.ExecuteRemovePart);

			if (!(this.designModel is null))
				this.designModel.PropertyChanged += DesignModel_PropertyChanged;
		}

		/// <summary>
		/// Legal ID
		/// </summary>
		public string LegalId
		{
			get => this.legalId.Value;
			set => this.legalId.Value = value;
		}

		/// <summary>
		/// Role
		/// </summary>
		public string Role
		{
			get => this.role.Value;
			set => this.role.Value = value;
		}

		/// <summary>
		/// Roles
		/// </summary>
		public string[] Roles
		{
			get
			{
				RoleInfo[] Roles = this.designModel?.Roles;

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
			return !(this.designModel is null);
		}

		/// <summary>
		/// Removes the part.
		/// </summary>
		public void ExecuteRemovePart()
		{
			if (!(this.designModel is null))
			{
				this.designModel.PropertyChanged -= DesignModel_PropertyChanged;
				this.designModel.RemovePart(this);
			}
		}
	}
}
