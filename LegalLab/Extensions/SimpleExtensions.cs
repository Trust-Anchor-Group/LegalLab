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

		/// <summary>
		/// Converts a <see cref="DateTime"/> value to a string, optionally adding a Z if UTC time coordinates are used.
		/// </summary>
		/// <param name="Value">DateTime value</param>
		/// <returns>String</returns>
		public static string ToStringTZ(this DateTime Value)
		{
			if (Value == DateTime.MinValue || Value == DateTime.MaxValue)
				return string.Empty;
			else
			{
				string s = Value.ToShortDateString() + ", " + Value.ToLongTimeString();

				if (Value.Kind == DateTimeKind.Utc)
					s += "Z";

				return s;
			}
		}
	}
}
