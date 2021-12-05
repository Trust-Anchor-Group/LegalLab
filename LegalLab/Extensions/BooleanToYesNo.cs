using System;

namespace LegalLab.Extensions
{
	/// <summary>
	/// Static class with simple type extensions.
	/// </summary>
	public static class SimpleExtensions
	{
		/// <summary>
		/// Converts a boolean value to either "Yes" or "No".
		/// </summary>
		/// <param name="Value">Boolean Value</param>
		/// <returns>"Yes" if true, "No" if false.</returns>
		public static string ToYesNo(this bool Value)
		{
			return Value ? "Yes" : "No";
		}
	}
}
