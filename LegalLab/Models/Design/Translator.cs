using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Static class performing language translation using OpenAI
	/// </summary>
	public static class Translator
	{
		/// <summary>
		/// Translates a string from one language to another, using OpenAI.
		/// </summary>
		/// <param name="Text">Text to translate.</param>
		/// <param name="From">Language to translate from.</param>
		/// <param name="To">Language to translate to.</param>
		/// <param name="Key">OpenAI key.</param>
		/// <returns>Translated string.</returns>
		public static async Task<string> Translate(string Text, string From, string To, string Key)
		{
			return (await Translate(new string[] { Text }, From, To, Key))[0];
		}

		/// <summary>
		/// Translates an array of strings from one language to another, using OpenAI.
		/// </summary>
		/// <param name="Texts">Strings to translate.</param>
		/// <param name="From">Language to translate from.</param>
		/// <param name="To">Language to translate to.</param>
		/// <param name="Key">OpenAI key.</param>
		/// <returns>Translated strings.</returns>
		public static async Task<string[]> Translate(string[] Texts, string From, string To, string Key)
		{
			int i, c = Texts.Length;
			string[] Response = new string[c];
			Task[] Requests = new Task[c];

			for (i = 0; i < c; i++)
				Requests[i] = Translate(Texts[i], From, To, Key, Response, i);

			await Task.WhenAll(Requests);

			return Response;
		}

		/// <summary>
		/// Translates a text string from one language to another, using OpenAI.
		/// </summary>
		/// <param name="Text">String to translate.</param>
		/// <param name="From">Language to translate from.</param>
		/// <param name="To">Language to translate to.</param>
		/// <param name="Key">OpenAI key.</param>
		/// <param name="Translations">Array of translations.</param>
		/// <param name="Index">Index into the <paramref name="Translations"/> array, where the translation is stored.</param>
		public static async Task Translate(string Text, string From, string To, string Key, string[] Translations, int Index)
		{
			try
			{
				object ResponseObj = await InternetContent.PostAsync(openAiCompletions,
					new Dictionary<string, object>()
					{
					{ "model", "gpt-3.5-turbo" },
					{ "messages", new Dictionary<string,object>[]
						{
							new Dictionary<string, object>()
							{
								{ "role", "system" },
								{ "content", "You help to translate Markdown text from language code " +
									From + " to language code " + To + ". Input is in raw Markdown. " +
									"Output must be in raw Markdown, keeping the Markdown formatting of the input. "+
									"No descriptive text or additional formatting must be included. No examples added. "+
									"Result must only include the translation. If the message is a question, "+
									"don't answer the question, only translate the question." }
							},
							new Dictionary<string, object>()
							{
								{ "role", "user" },
								{ "content", Text }
							}
						}
					}
					},
					new KeyValuePair<string, string>[]
					{
					new KeyValuePair<string, string>("Accept", "application/json"),
					new KeyValuePair<string, string>("Authorization", "Bearer " + Key),
					});

				if (ResponseObj is Dictionary<string, object> Response &&
					Response.TryGetValue("choices", out object Obj) &&
					Obj is Array Choices &&
					Choices.Length > 0 &&
					Choices.GetValue(0) is Dictionary<string, object> Choice &&
					Choice.TryGetValue("message", out Obj) &&
					Obj is Dictionary<string, object> Message &&
					Message.TryGetValue("content", out Obj) &&
					Obj is string Content)
				{
					Translations[Index] = Content;
				}
				else
					Translations[Index] = Text;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				Translations[Index] = Text;
			}
		}

		private static Uri openAiCompletions = new Uri("https://api.openai.com/v1/chat/completions");

	}
}
