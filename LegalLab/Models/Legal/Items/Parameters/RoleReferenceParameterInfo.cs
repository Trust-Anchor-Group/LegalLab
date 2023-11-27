using LegalLab.Models.Design;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items.Parameters
{
	/// <summary>
	/// Contains information about a role reference parameter
	/// </summary>
	public class RoleReferenceParameterInfo : ParameterInfo
	{
		private readonly Property<string> role;
		private readonly Property<string> property;
		private readonly Property<int> index;
		private readonly Property<bool> required;

		/// <summary>
		/// Contains information about a role reference parameter
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Control">Edit control</param>
		/// <param name="DesignModel">Design model</param>
		/// <param name="Parameters">Collection of parameters.</param>
		public RoleReferenceParameterInfo(Contract Contract, RoleParameter Parameter, Control Control, DesignModel DesignModel, Property<RoleReferenceParameterInfo[]> Parameters)
			: base(Contract, Parameter, Control, DesignModel, Parameters)
		{
			this.role = new Property<string>(nameof(this.Role), Parameter.Role, this);
			this.property = new Property<string>(nameof(this.Property), Parameter.Property, this);
			this.index = new Property<int>(nameof(this.Index), Parameter.Index, this);
			this.required = new Property<bool>(nameof(this.Required), Parameter.Required, this);

			if (this.designModel is not null)
				this.designModel.PropertyChanged += this.DesignModel_PropertyChanged;
		}

		/// <summary>
		/// Name of the referenced role.
		/// </summary>
		public string Role
		{
			get => this.role.Value;
			set => this.role.Value = value;
		}

		/// <summary>
		/// Role index.
		/// </summary>
		public int Index
		{
			get => this.index.Value;
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(this.Index), "Role index must be positive.");

				this.index.Value = value;
			}
		}

		/// <summary>
		/// Property name.
		/// </summary>
		public string Property
		{
			get => this.property.Value;
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentOutOfRangeException(nameof(this.Property), "The property reference cannot be empty.");

				this.property.Value = value;
			}
		}

		/// <summary>
		/// If property is required.
		/// </summary>
		public bool Required
		{
			get => this.required.Value;
			set => this.required.Value = value;
		}

		/// <summary>
		/// Available role names.
		/// </summary>
		public string[] Roles => this.designModel.RoleNames;

		private void DesignModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.Roles))
				this.RaisePropertyChanged(nameof(this.Roles));
		}

		/// <inheritdoc/>
		public override void SetValue(string Value)
		{
			throw new Exception("Role reference parameter cannot be set.");
		}

		/// <summary>
		/// Removes the parameter.
		/// </summary>
		public override Task ExecuteRemoveParameter()
		{
			this.designModel?.RemoveRoleReferenceParameter(this);
			return Task.CompletedTask;
		}
	}
}
