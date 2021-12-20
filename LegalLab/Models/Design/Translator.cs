using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

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
			Dictionary<int, string> ImmutableByIndex = new Dictionary<int, string>();
			Dictionary<string, int> ImmutableByString = new Dictionary<string, int>();
			string s, s2, Ref;
			int i, j, Index;

			foreach (string Text in Texts)
			{
				s = Text;

				i = s.IndexOf("[%");

				while (i >= 0)
				{
					j = s.IndexOf(']', i + 2);
					if (j < 0)
						break;

					Ref = s.Substring(i, j - i + 1);

					if (!ImmutableByString.TryGetValue(Ref, out Index))
					{
						Index = ImmutableByString.Count;
						ImmutableByString[Ref] = Index;
						ImmutableByIndex[Index] = Ref;
					}

					s2 = "{" + Index.ToString() + "}";
					s = s.Remove(i, j - i + 1).Insert(i, s2);

					i = s.IndexOf("[%", i + s2.Length);
				}

				Req.Add(new Dictionary<string, object>() { { "Text", s } });
			}

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

				i = TranslatedText.IndexOf('{');
				while (i >= 0)
				{
					j = TranslatedText.IndexOf('}', i + 1);
					if (j < 0)
						break;

					if (int.TryParse(TranslatedText.Substring(i + 1, j - i - 1), out Index) &&
						ImmutableByIndex.TryGetValue(Index, out Ref))
					{
						TranslatedText = TranslatedText.Remove(i, j - i + 1).Insert(i, Ref);
						i = TranslatedText.IndexOf('{', i + Ref.Length);
					}
					else
						i = TranslatedText.IndexOf('{', j + 1);
				}

				Response.Add(TranslatedText);
			}

			return Response.ToArray();
		}

	}
}
