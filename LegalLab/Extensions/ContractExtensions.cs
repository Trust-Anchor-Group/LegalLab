﻿using LegalLab.Models.Legal.Items;
using NeuroFeatures.NoteCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Contracts;
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
			StringBuilder Result = new();
			Contract.Serialize(Result, true, true, false, false, false, false, false);
			return Result.ToString();
		}

		/// <summary>
		/// Converts XML to a contract.
		/// </summary>
		/// <param name="Xml">XML string</param>
		/// <param name="ContractsClient">Contracts client</param>
		/// <returns>Contract</returns>
		public static async Task<Contract> ToContract(this string Xml, ContractsClient ContractsClient)
		{
			XmlDocument Doc = new()
			{
				PreserveWhitespace = true
			};

			Doc.LoadXml(Xml);

			ParsedContract Parsed = await Contract.Parse(Doc, ContractsClient);
			return Parsed.Contract;
		}

		/// <summary>
		/// Converts Markdown to Human-readable text for use in smart contracts.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Language">Language code</param>
		/// <returns>Human-readable text.</returns>
		public static async Task<HumanReadableText[]> ToHumanReadableText(this string[] Markdown, string Language)
		{
			List<HumanReadableText> Result = [];

			foreach (string s in Markdown)
			{
				if (!string.IsNullOrEmpty(s))
					Result.Add(await s.ToHumanReadableText(Language));
			}

			return [.. Result];
		}

		/// <summary>
		/// Converts Markdown to Human-readable text for use in smart contracts.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Language">Language code</param>
		/// <returns>Human-readable text.</returns>
		public static async Task<HumanReadableText> ToHumanReadableText(this string Markdown, string Language)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			MarkdownDocument ParsedMarkdown = await MarkdownDocument.CreateAsync(Markdown);
			using ContractsRenderer Renderer = new(XML.WriterSettings(false, true), "Root", ContractsClient.NamespaceSmartContractsCurrent);
			
			await ParsedMarkdown.RenderDocument(Renderer);

			string Xml = Renderer.ToString();
			XmlDocument ParsedXml = new()
			{
				PreserveWhitespace = true
			};
			ParsedXml.LoadXml(Xml);

			HumanReadableText Result = HumanReadableText.Parse(ParsedXml.DocumentElement);
			Result.Language = Language;

			return Result;
		}

		/// <summary>
		/// Converts Markdown to Human-readable text for use in smart contracts.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Language">Language code</param>
		/// <returns>Human-readable text.</returns>
		public static async Task<Label[]> ToHumanReadableLabel(this string[] Markdown, string Language)
		{
			List<Label> Result = [];

			foreach (string s in Markdown)
			{
				if (!string.IsNullOrEmpty(s))
					Result.Add(await s.ToHumanReadableLabel(Language));
			}

			return [.. Result];
		}

		/// <summary>
		/// Converts Markdown to Human-readable text for use in smart contracts.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Language">Language code</param>
		/// <returns>Human-readable text.</returns>
		public static async Task<Label> ToHumanReadableLabel(this string Markdown, string Language)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			MarkdownDocument ParsedMarkdown = await MarkdownDocument.CreateAsync(Markdown);
			using ContractsRenderer Renderer = new(XML.WriterSettings(false, true), "Root", ContractsClient.NamespaceSmartContractsCurrent);
			
			await ParsedMarkdown.RenderDocument(Renderer);

			string Xml = Renderer.ToString();
			XmlDocument ParsedXml = new()
			{
				PreserveWhitespace = true
			};
			ParsedXml.LoadXml(Xml);

			Label Result = Label.Parse(ParsedXml.DocumentElement);
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
			string s = P.Name;

			if (P is BooleanParameter)
				s += ".";
			else
				s += ":";

			if (string.IsNullOrEmpty(P.Guide))
				return s;
			else
				return s + " (" + P.Guide + ")";
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

			for (i = 0; i < c; i++)
				Result[i] = Parts[i].Part;

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
			XmlDocument Doc = new()
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

			StringBuilder sb = new();
			Contract.NormalizeXml(Xml, sb, ContractsClient.NamespaceSmartContractsCurrent);

			XmlDocument Doc = new()
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

			XmlDocument Doc = new()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);

			if (Doc.DocumentElement is null)
				return null;

			StringBuilder sb = new();
			Contract.NormalizeXml(Doc.DocumentElement, sb, ContractsClient.NamespaceSmartContractsCurrent);

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
			SortedDictionary<string, HumanReadableText> Sorted = [];

			if (Texts is not null)
			{
				foreach (HumanReadableText Old in Texts)
				{
					if (Old.Language is not null)
						Sorted[Old.Language] = Old;
				}
			}

			Sorted[Text.Language ?? string.Empty] = Text;

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
			List<HumanReadableText> Result = [];

			if (Texts is not null)
			{
				foreach (HumanReadableText Old in Texts)
				{
					if (Old.Language != Language)
						Result.Add(Old);
				}
			}

			return [.. Result];
		}

		/// <summary>
		/// Appends a localization to a set of localized texts.
		/// </summary>
		/// <param name="Texts">Set of localized texts</param>
		/// <param name="Text">New addition</param>
		/// <returns>Sorted array of updated texts.</returns>
		public static Label[] Append(this Label[] Texts, Label Text)
		{
			SortedDictionary<string, Label> Sorted = [];

			if (Texts is not null)
			{
				foreach (Label Old in Texts)
				{
					if (Old.Language is not null)
						Sorted[Old.Language] = Old;
				}
			}

			Sorted[Text.Language ?? string.Empty] = Text;

			int c = Sorted.Count;
			Label[] Result = new Label[c];

			Sorted.Values.CopyTo(Result, 0);

			return Result;
		}

		/// <summary>
		/// Removes a language from an array of localized texts.
		/// </summary>
		/// <param name="Texts">Set of localized texts.</param>
		/// <param name="Language">Language to remove</param>
		/// <returns>Updated array</returns>
		public static Label[] Remove(this Label[] Texts, string Language)
		{
			List<Label> Result = [];

			if (Texts is not null)
			{
				foreach (Label Old in Texts)
				{
					if (Old.Language != Language)
						Result.Add(Old);
				}
			}

			return [.. Result];
		}

		/// <summary>
		/// Finds a localized version of a text.
		/// </summary>
		/// <param name="AvailableTexts">Available texts</param>
		/// <param name="Language">Language</param>
		/// <returns>Result, if found.</returns>
		public static HumanReadableText Find(this HumanReadableText[] AvailableTexts, string Language)
		{
			foreach (HumanReadableText Text in AvailableTexts)
			{
				if (Text.Language == Language)
					return Text;
			}

			if (Language != "en")
			{
				foreach (HumanReadableText Text in AvailableTexts)
				{
					if (Text.Language == "en")
						return Text;
				}
			}

			foreach (HumanReadableText Text in AvailableTexts)
				return Text;

			return null;
		}

		/// <summary>
		/// Finds a localized version of a text.
		/// </summary>
		/// <param name="AvailableTexts">Available texts</param>
		/// <param name="Language">Language</param>
		/// <returns>Result, if found.</returns>
		public static string Find(this LocalizedString[] AvailableTexts, string Language)
		{
			foreach (LocalizedString Text in AvailableTexts)
			{
				if (Text.Language == Language)
					return Text.Text;
			}

			if (Language != "en")
			{
				foreach (LocalizedString Text in AvailableTexts)
				{
					if (Text.Language == "en")
						return Text.Text;
				}
			}

			foreach (LocalizedString Text in AvailableTexts)
				return Text.Text;

			return null;
		}

		/// <summary>
		/// Appends a text to an array of texts.
		/// </summary>
		/// <param name="Texts">Set of texts</param>
		/// <param name="Text">New addition</param>
		/// <returns>Sorted array of updated texts.</returns>
		public static string[] Append(this string[] Texts, string Text)
		{
			SortedDictionary<string, string> Sorted = [];

			if (Texts is not null)
			{
				foreach (string Old in Texts)
					Sorted[Old] = Old;
			}

			Sorted[Text] = Text;

			int c = Sorted.Count;
			string[] Result = new string[c];

			Sorted.Values.CopyTo(Result, 0);

			return Result;
		}

		/// <summary>
		/// Removes a text from an array of texts.
		/// </summary>
		/// <param name="Texts">Set of texts.</param>
		/// <param name="Text">Text to remove</param>
		/// <returns>Updated array</returns>
		public static string[] Remove(this string[] Texts, string Text)
		{
			int i = Array.IndexOf(Texts, Text);

			if (i < 0)
				return Texts;

			int c = Texts.Length;
			if (c == 1)
				return [];

			string[] Result = new string[c - 1];

			if (i > 0)
				Array.Copy(Texts, 0, Result, 0, i);

			if (i < c - 1)
				Array.Copy(Texts, i + 1, Result, i, c - i - 1);

			return Result;
		}

		/// <summary>
		/// Gets editable Markdown from a localized set of texts.
		/// </summary>
		/// <param name="Contract">Contract hosting texts.</param>
		/// <param name="Texts">Localized set of texts.</param>
		/// <param name="Language">Language</param>
		/// <returns>Editable markdown</returns>
		public static async Task<string> ToMarkdown(this Contract Contract, HumanReadableText[] Texts, string Language)
		{
			foreach (HumanReadableText Text in Texts)
			{
				if (Text.Language == Language)
					return await Text.GenerateMarkdown(Contract, MarkdownType.ForEditing);
			}

			foreach (HumanReadableText Text in Texts)
			{
				if (Text.Language == Contract.DefaultLanguage)
					return await Text.GenerateMarkdown(Contract, MarkdownType.ForEditing);
			}

			return string.Empty;
		}

		/// <summary>
		/// Converts a <see cref="ProtectionLevel"/> enumeration to a Brush color.
		/// </summary>
		/// <param name="Protection">Protection level.</param>
		/// <returns>Default color brush.</returns>
		public static Brush DefaultBrush(this ProtectionLevel Protection)
		{
			return Protection switch
			{
				ProtectionLevel.Transient => Brushes.LightBlue,
				ProtectionLevel.Encrypted => Brushes.LightGreen,
				_ => null,
			};
		}
	}
}
