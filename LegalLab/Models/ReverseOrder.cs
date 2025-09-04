using System;
using System.Collections.Generic;

namespace LegalLab.Models
{
	/// <summary>
	/// Reverse sorting order
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReverseOrder<T> : IComparer<T>
		where T : IComparable
	{
		/// <summary>
		/// Compares two elements.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(T x, T y)
		{
			return y.CompareTo(x);
		}
	}
}
