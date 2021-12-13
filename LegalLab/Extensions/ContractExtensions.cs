using System;
using System.Text;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

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

		/// <summary>
		/// Converts Markdown to Human-readable text for use in smart contracts.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <returns>Human-readable text.</returns>
		public static HumanReadableText ToHumanReadableText(this string Markdown)
		{
			MarkdownDocument ParsedMarkdown = new MarkdownDocument(Markdown);
			StringBuilder sb = new StringBuilder();
			using XmlWriter w = XmlWriter.Create(sb);

			w.WriteStartElement("Root", ContractsClient.NamespaceSmartContracts);
			ParsedMarkdown.GenerateSmartContractXml(w);
			w.WriteEndElement();
			w.Flush();

			string Xml = sb.ToString();
			XmlDocument ParsedXml = new XmlDocument();
			ParsedXml.LoadXml(Xml);

			HumanReadableText Result = HumanReadableText.Parse(ParsedXml.DocumentElement);

			return Result;
		}

		public static string GetLabel(this Parameter P)
		{
			if (string.IsNullOrEmpty(P.Guide))
				return P.Name + ":";
			else
				return P.Name + " (" + P.Guide + "):";
		}

	}
}
