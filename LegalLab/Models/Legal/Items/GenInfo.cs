namespace LegalLab.Models.Legal.Items
{
	/// <summary>
	/// Contains some general information
	/// </summary>
	/// <param name="Name">Name</param>
	/// <param name="Value">Value</param>
	public class GenInfo(string Name, string Value)
	{
		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; internal set; } = Name;

		/// <summary>
		/// Value
		/// </summary>
		public string Value { get; internal set; } = Value;
	}
}
