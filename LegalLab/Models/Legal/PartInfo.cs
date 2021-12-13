using LegalLab.Models.Design;
using System;
using System.Windows.Input;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contains information about a part in the contract
	/// </summary>
	public class PartInfo : Model
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
		/// <param name="Roles">Currently available roles</param>
		public PartInfo(string LegalId, string Role, DesignModel DesignModel)
		{
			this.legalId = new Property<string>(nameof(this.LegalId), LegalId, this);
			this.role = new Property<string>(nameof(this.Role), Role, this);

			this.designModel = DesignModel;

			this.removePart = new Command(this.CanExecuteRemovePart, this.ExecuteRemovePart);
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
			this.designModel?.RemovePart(this);
		}
	}
}
