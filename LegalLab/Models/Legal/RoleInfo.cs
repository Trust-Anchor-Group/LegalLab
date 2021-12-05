using System;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
    /// <summary>
    /// Contains information about a role
    /// </summary>
    public class RoleInfo : Model
    {
        private readonly Property<string> name;
        private readonly Property<string> description;
        private readonly Property<int> minCount;
        private readonly Property<int> maxCount;
        private readonly Property<bool> canRevoke;

        /// <summary>
        /// Contains information about a role
        /// </summary>
		/// <param name="Contract">Contract hosting the parameter</param>
        /// <param name="Role">Role</param>
        public RoleInfo(Contract Contract, Role Role)
		{
            this.name = new Property<string>(nameof(this.Name), Role.Name, this);
            this.description = new Property<string>(nameof(this.Description), Role.ToPlainText(Contract.DefaultLanguage, Contract).Trim(), this);
            this.minCount = new Property<int>(nameof(this.MaxCount), Role.MinCount, this);
            this.maxCount = new Property<int>(nameof(this.MinCount), Role.MaxCount, this);
            this.canRevoke = new Property<bool>(nameof(this.CanRevoke), Role.CanRevoke, this);
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
        /// Description of the role.
        /// </summary>
        public string Description
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
    }
}
