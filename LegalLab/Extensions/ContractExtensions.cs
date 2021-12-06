using System;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Extensions
{
	/// <summary>
	/// Static class with contract extensions.
	/// </summary>
	public static class ContractExtensions
	{
		/// <summary>
		/// Converts a contract to XML
		/// </summary>
		/// <param name="Contract">Contract Reference</param>
		/// <returns>XML.</returns>
		public static string ToXml(this Contract Contract)
		{
			StringBuilder Result = new StringBuilder();
			Contract.Serialize(Result, true, true, true, true, true, true, true);
			return Result.ToString();
		}

		/// <summary>
		/// Converts XML to a contract.
		/// </summary>
		/// <param name="Xml">XML string</param>
		/// <returns>Contract</returns>
		public static Contract ToContract(this string Xml)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			Doc.LoadXml(Xml);

			return Contract.Parse(Doc, out _, out _);
		}
	}
}
