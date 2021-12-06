using System;
using System.Collections.Generic;

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

		/// <summary>
		/// Returns an array of values.
		/// </summary>
		/// <typeparam name="Key">Key type</typeparam>
		/// <typeparam name="Value">Value type</typeparam>
		/// <param name="Dictionary">Dictionary</param>
		/// <returns>Value array</returns>
		public static Value[] ToValueArray<Key, Value>(this IDictionary<Key, Value> Dictionary)
		{
			Value[] Result = new Value[Dictionary.Count];
			Dictionary.Values.CopyTo(Result, 0);
			return Result;
		}

		/// <summary>
		/// Returns an array of keys.
		/// </summary>
		/// <typeparam name="Key">Key type</typeparam>
		/// <typeparam name="Value">Value type</typeparam>
		/// <param name="Dictionary">Dictionary</param>
		/// <returns>Key array</returns>
		public static Key[] ToKeyArray<Key, Value>(this IDictionary<Key, Value> Dictionary)
		{
			Key[] Result = new Key[Dictionary.Count];
			Dictionary.Keys.CopyTo(Result, 0);
			return Result;
		}

	}
}
