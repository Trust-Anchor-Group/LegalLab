﻿using LegalLab.Extensions;
using System;
using System.Windows.Input;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Contains information about a role
	/// </summary>
	public class RoleInfo : Model
	{
		private readonly ContractModel contractModel;
		private readonly Contract contract;
		private readonly Property<string> name;
		private readonly Property<object> description;
		private readonly Property<int> minCount;
		private readonly Property<int> maxCount;
		private readonly Property<bool> canRevoke;

		private readonly Command signAsRole;
		private readonly Command proposeForRole;

		/// <summary>
		/// Contains information about a role
		/// </summary>
		/// <param name="ContractModel">Contract Model hosting the parameter</param>
		/// <param name="Role">Role</param>
		public RoleInfo(ContractModel ContractModel, Role Role)
			: this(ContractModel.Contract, Role)
		{
			this.contractModel = ContractModel;
		}

		/// <summary>
		/// Contains information about a role
		/// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
		/// <param name="Role">Role</param>
		public RoleInfo(Contract Contract, Role Role)
		{
			this.contract = Contract;

			this.name = new Property<string>(nameof(this.Name), Role.Name, this);
			this.description = new Property<object>(nameof(this.Description), Role.ToSimpleXAML(this.contract?.DefaultLanguage, this.contract), this);
			this.minCount = new Property<int>(nameof(this.MaxCount), Role.MinCount, this);
			this.maxCount = new Property<int>(nameof(this.MinCount), Role.MaxCount, this);
			this.canRevoke = new Property<bool>(nameof(this.CanRevoke), Role.CanRevoke, this);

			this.signAsRole = new Command(this.CanExecuteSignAsRole, this.ExecuteSignAsRole);
			this.proposeForRole = new Command(this.CanExecuteProposeForRole, this.ExecuteProposeForRole);
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get => this.name.Value;
			set => this.name.Value = value;
		}

		/// <summary>
		/// Description (formatted) of the role.
		/// </summary>
		public object Description
		{
			get => this.description.Value;
			set => this.description.Value = value;
		}

		/// <summary>
		/// Minimum number of signatures for this role required.
		/// </summary>
		public int MinCount
		{
			get => this.minCount.Value;
			set => this.minCount.Value = value;
		}

		/// <summary>
		/// Maximum number of signatures for this role.
		/// </summary>
		public int MaxCount
		{
			get => this.maxCount.Value;
			set => this.maxCount.Value = value;
		}

		/// <summary>
		/// If the role can revoke the contract.
		/// </summary>
		public bool CanRevoke
		{
			get => this.canRevoke.Value;
			set => this.canRevoke.Value = value;
		}

		public void CanBeSignedChanged()
		{
			this.signAsRole.RaiseCanExecuteChanged();
			this.proposeForRole.RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Sign as role command
		/// </summary>
		public ICommand SignAsRole => this.signAsRole;

		/// <summary>
		/// If the sign as role command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteSignAsRole()
		{
			return this.contractModel?.CanBeSigned ?? false;
		}

		/// <summary>
		/// Proposes the contract.
		/// </summary>
		public async void ExecuteSignAsRole()
		{
			await this.contractModel?.SignAsRole(this.Name);
		}


		/// <summary>
		/// Propose for role command
		/// </summary>
		public ICommand ProposeForRole => this.proposeForRole;

		/// <summary>
		/// If the propose for role command can be exeucted.
		/// </summary>
		/// <returns></returns>
		public bool CanExecuteProposeForRole()
		{
			return this.contractModel?.CanBeSigned ?? false;
		}

		/// <summary>
		/// Proposes the contract.
		/// </summary>
		public void ExecuteProposeForRole()
		{
			this.contractModel?.ProposeForRole(this.Name);
		}
	}
}
