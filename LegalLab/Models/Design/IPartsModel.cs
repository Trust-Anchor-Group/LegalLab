using LegalLab.Models.Legal.Items;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Interface for models allowing editing of parts
	/// </summary>
	public interface IPartsModel : IModel
	{
		/// <summary>
		/// Roles defined the contract.
		/// </summary>
		public RoleInfo[] Roles
		{
			get;
		}

		/// <summary>
		/// Parts defined the contract.
		/// </summary>
		public PartInfo[] Parts
		{
			get;
			set;
		}

		/// <summary>
		/// Removes a part from the design
		/// </summary>
		/// <param name="Part">Part to remove</param>
		void RemovePart(PartInfo Part);
	}
}
