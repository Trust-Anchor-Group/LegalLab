using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Static class performing language translation using Microsoft Translator
	/// </summary>
	public static class Translator
	{
		/// <summary>
		/// Translates a string from one language to another, using Microsoft Translator.
		/// </summary>
		/// <param name="Text">Text to translate.</param>
		/// <param name="From">Language to translate from.</param>
		/// <param name="To">Language to translate to.</param>
		/// <param name="Key">Microsoft Translator key.</param>
		/// <returns>Translated string.</returns>
		public static async Task<string> Translate(string Text, string From, string To, string Key)
		{
			return (await Translate(new string[] { Text }, From, To, Key))[0];
		}

		/// <summary>
		/// Translates an array of strings from one language to another, using Microsoft Translator.
		/// </summary>
		/// <param name="Texts">Strings to translate.</param>
		/// <param name="From">Language to translate from.</param>
		/// <param name="To">Language to translate to.</param>
		/// <param name="Key">Microsoft Translator key.</param>
		/// <returns>Translated strings.</returns>
		public static async Task<string[]> Translate(string[] Texts, string From, string To, string Key)
		{
			List<Dictionary<string, object>> Req = new List<Dictionary<string, object>>();

			foreach (string Text in Texts)
				Req.Add(new Dictionary<string, object>() { { "Text", Text } });

			StringBuilder Url = new StringBuilder();

			Url.Append("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=");
			Url.Append(From);
			Url.Append("&to=");
			Url.Append(To);

			object Resp = await InternetContent.PostAsync(new Uri(Url.ToString()), Req.ToArray(),
				new KeyValuePair<string, string>("Ocp-Apim-Subscription-Key", Key));

			if (!(Resp is Array A))
				throw new Exception("Unexpected reponse returned.");

			List<string> Response = new List<string>();

			foreach (object Item in A)
			{
				if (!(Item is Dictionary<string, object> TypedItem) ||
					!TypedItem.TryGetValue("translations", out object Obj) ||
					!(Obj is Array A2) ||
					A2.Length != 1 ||
					!(A2.GetValue(0) is Dictionary<string, object> Translation) ||
					!Translation.TryGetValue("text", out object Obj2) ||
					!(Obj2 is string TranslatedText))
				{
					throw new Exception("Unexpected reponse returned.");
				}

				Response.Add(TranslatedText);
			}

			return Response.ToArray();
		}

		public static async Task<string> GetMarkdown(HumanReadableText[] AvailableTexts, string Language, Contract Contract, string Key)
		{
			HumanReadableText Text = FindText(AvailableTexts, Language);
			if (!(Text is null))
				return Text.GenerateMarkdown(Contract, MarkdownType.ForEditing);

			string DefaultLanguage = Contract.DefaultLanguage;
			Text = FindText(AvailableTexts, DefaultLanguage);
			string Markdown = Text?.GenerateMarkdown(Contract, MarkdownType.ForEditing) ?? string.Empty;

			if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Language) || string.IsNullOrEmpty(DefaultLanguage))
				return Markdown;

			List<string> Immutable = new List<string>();
			int i = Markdown.IndexOf("[%");
			int j;

			while (i >= 0)
			{
				j = Markdown.IndexOf(']', i + 2);
				if (j < 0)
					break;

				Immutable.Add(Markdown.Substring(i, j - i + 1));
				string s = "{" + Immutable.Count.ToString() + "}";
				Markdown = Markdown.Remove(i, j - i + 1).Insert(i, s);

				i = Markdown.IndexOf("[%", i + s.Length);
			}

			Markdown = await Translate(Markdown, DefaultLanguage, Language, Key);

			j = 1;
			foreach (string s in Immutable)
			{
				string s2 = "{" + (j++).ToString() + "}";

				i = Markdown.IndexOf(s2);
				if (i < 0)
					Markdown += s;
				else
					Markdown = Markdown.Remove(i, s2.Length).Insert(i, s);
			}

			return Markdown;
		}

		public static HumanReadableText FindText(HumanReadableText[] AvailableTexts, string Language)
		{
			foreach (HumanReadableText Text in AvailableTexts)
			{
				if (Text.Language == Language)
					return Text;
			}

			return null;
		}
	}
}
