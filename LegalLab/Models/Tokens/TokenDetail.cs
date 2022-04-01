﻿namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Contains a detail of information about a token.
	/// </summary>
	public class TokenDetail
	{
		/// <summary>
		/// Contains a detail of information about a token.
		/// </summary>
		/// <param name="Name">Detail name</param>
		/// <param name="Value">Detail value.</param>
		public TokenDetail(string Name, object Value)
		{
			this.Name = Name;
			this.Value = Value;
		}

		public string Name { get; }
		public object Value { get; }
	}
}
