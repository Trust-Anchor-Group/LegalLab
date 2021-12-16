﻿using LegalLab.Models.Legal.Items;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Xml;
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
			Contract.Serialize(Result, true, true, false, false, false, false, false);
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
		/// <param name="Language">Language code</param>
		/// <returns>Human-readable text.</returns>
		public static HumanReadableText ToHumanReadableText(this string Markdown, string Language)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			MarkdownDocument ParsedMarkdown = new MarkdownDocument(Markdown);
			StringBuilder sb = new StringBuilder();
			using XmlWriter w = XmlWriter.Create(sb);

			w.WriteStartElement("Root", ContractsClient.NamespaceSmartContracts);
			ParsedMarkdown.GenerateSmartContractXml(w);
			w.WriteEndElement();
			w.Flush();

			string Xml = sb.ToString();
			XmlDocument ParsedXml = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			ParsedXml.LoadXml(Xml);

			HumanReadableText Result = HumanReadableText.Parse(ParsedXml.DocumentElement);
			Result.Language = Language;

			return Result;
		}

		/// <summary>
		/// Gets the label text for a parameter.
		/// </summary>
		/// <param name="P">Parameter</param>
		/// <returns>Label</returns>
		public static string GetLabel(this Parameter P)
		{
			if (string.IsNullOrEmpty(P.Guide))
				return P.Name + ":";
			else
				return P.Name + " (" + P.Guide + "):";
		}

		/// <summary>
		/// Creates an array of roles, from an array of Role information models.
		/// </summary>
		/// <param name="Roles">Role information models</param>
		/// <returns>Roles</returns>
		public static Role[] ToRoles(this RoleInfo[] Roles)
		{
			int i, c = Roles.Length;
			Role[] Result = new Role[c];

			for (i = 0; i < c; i++)
				Result[i] = Roles[i].Role;

			return Result;
		}

		/// <summary>
		/// Creates an array of Parts, from an array of Part information models.
		/// </summary>
		/// <param name="Parts">Part information models</param>
		/// <returns>Parts</returns>
		public static Part[] ToParts(this PartInfo[] Parts)
		{
			int i, c = Parts.Length;
			Part[] Result = new Part[c];
			PartInfo Info;

			for (i = 0; i < c; i++)
			{
				Info = Parts[i];

				Result[i] = new Part()
				{
					LegalId = Info.LegalId,
					Role = Info.Role
				};
			}

			return Result;
		}

		/// <summary>
		/// Creates an array of parameters, from an array of Parameter information models.
		/// </summary>
		/// <param name="Parameters">Parameter information models</param>
		/// <returns>Parameters</returns>
		public static Parameter[] ToParameters(this ParameterInfo[] Parameters)
		{
			int i, c = Parameters.Length;
			Parameter[] Result = new Parameter[c];

			for (i = 0; i < c; i++)
				Result[i] = Parameters[i].Parameter;

			return Result;
		}

		/// <summary>
		/// Makes XML pretty and readable.
		/// </summary>
		/// <param name="Xml">XML</param>
		/// <returns>String representation of pretty XML, as well as parsed version.</returns>
		public static (string, XmlElement) ToPrettyXml(this string Xml)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);

			return Doc.DocumentElement.ToPrettyXml();
		}

		/// <summary>
		/// Makes XML pretty and readable.
		/// </summary>
		/// <param name="Xml">XML</param>
		/// <returns>String representation of pretty XML, as well as parsed version.</returns>
		public static (string, XmlElement) ToPrettyXml(this XmlElement Xml)
		{
			if (Xml is null)
				return (string.Empty, null);

			StringBuilder sb = new StringBuilder();
			Contract.NormalizeXml(Xml, sb, ContractsClient.NamespaceSmartContracts);

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(sb.ToString());
			sb.Clear();

			XmlWriterSettings Settings = XML.WriterSettings(true, true);
			using XmlWriter w = XmlWriter.Create(sb, Settings);

			Xml.WriteTo(w);
			w.Flush();

			string s = sb.ToString().Replace("&#xD;\n", "\n");
			Xml = Doc.DocumentElement;

			return (s, Xml);
		}

		/// <summary>
		/// Normalizes XML
		/// </summary>
		/// <param name="Xml">XML being edited</param>
		/// <returns>Normalized XML</returns>
		public static XmlElement NormalizeXml(this string Xml)
		{
			if (string.IsNullOrEmpty(Xml))
				return null;

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);

			if (Doc.DocumentElement is null)
				return null;

			StringBuilder sb = new StringBuilder();
			Contract.NormalizeXml(Doc.DocumentElement, sb, ContractsClient.NamespaceSmartContracts);

			Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(sb.ToString());

			return Doc.DocumentElement;
		}

		/// <summary>
		/// Appends a localization to a set of localized texts.
		/// </summary>
		/// <param name="Texts">Set of localized texts</param>
		/// <param name="Text">New addition</param>
		/// <returns>Sorted array of updated texts.</returns>
		public static HumanReadableText[] Append(this HumanReadableText[] Texts, HumanReadableText Text)
		{
			SortedDictionary<string, HumanReadableText> Sorted = new SortedDictionary<string, HumanReadableText>();

			if (!(Texts is null))
			{
				foreach (HumanReadableText Old in Texts)
					Sorted[Old.Language] = Old;
			}

			Sorted[Text.Language] = Text;

			int c = Sorted.Count;
			HumanReadableText[] Result = new HumanReadableText[c];

			Sorted.Values.CopyTo(Result, 0);

			return Result;
		}

		/// <summary>
		/// Removes a language from an array of localized texts.
		/// </summary>
		/// <param name="Texts">Set of localized texts.</param>
		/// <param name="Language">Language to remove</param>
		/// <returns>Updated array</returns>
		public static HumanReadableText[] Remove(this HumanReadableText[] Texts, string Language)
		{
			List<HumanReadableText> Result = new List<HumanReadableText>();

			if (!(Texts is null))
			{
				foreach (HumanReadableText Old in Texts)
				{
					if (Old.Language != Language)
						Result.Add(Old);
				}
			}

			return Result.ToArray();
		}
	}
}
