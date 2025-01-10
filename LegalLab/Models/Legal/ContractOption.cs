using System.Collections.Generic;
using Waher.Persistence;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// The data model for contract visibility.
	/// </summary>
	/// <param name="Name">The contract name.</param>
	/// <param name="Option">Parameter dictionary of option.</param>
	public class ContractOption(string Name, IDictionary<CaseInsensitiveString, object> Option)
	{
		/// <summary>
		/// The contract visibility.
		/// </summary>
		public IDictionary<CaseInsensitiveString, object> Option { get; } = Option;

		/// <summary>
		/// The contract name.
		/// </summary>
		public string Name { get; } = Name;

		/// <summary>
		/// Returns the string representation, i.e. name, of this contract option.
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString()
		{
			return this.Name;
		}
	}
}
