using System;
using System.Collections.Generic;

namespace LegalLab.Models.Standards
{
	/// <summary>
	/// Static class containing ISO 639-1 language codes
	/// </summary>
	public static class Iso__639_1
	{
		/// <summary>
		/// Contains one record of the ISO 639-1 data set.
		/// </summary>
		public class Record
		{
			/// <summary>
			/// Contains one record of the ISO 639-1 data set.
			/// </summary>
			/// <param name="Language">Language name</param>
			/// <param name="Code">Language Code</param>
			public Record(string Language, string Code)
			{
				this.Language = Language;
				this.Code = Code;
			}

			/// <summary>
			/// Language
			/// </summary>
			public string Language { get; set; }

			/// <summary>
			/// ISO 639-1 language code
			/// </summary>
			public string Code { get; set; }
		}

		private static Dictionary<string, string> languageByCode = null;

		/// <summary>
		/// Tries to get the language name corresponding to an ISO 639-1 language code.
		/// </summary>
		/// <param name="Code">Language code</param>
		/// <param name="Language">Language, if found.</param>
		/// <returns>If a corresponding language code was found.</returns>
		public static bool CodeToLanguage(string Code, out string Language)
		{
			if (languageByCode is null)
			{
				Dictionary<string, string> Temp = new(StringComparer.CurrentCultureIgnoreCase);

				foreach (Record Rec in Data)
					Temp[Rec.Code] = Rec.Language;

				languageByCode = Temp;
			}

			return languageByCode.TryGetValue(Code, out Language);
		}

		/// <summary>
		/// Available language codes
		/// </summary>
		public static readonly Record[] Data = new Record[]
		{
			new Record("Abkhaz", "ab"),
			new Record("Afar", "aa"),
			new Record("Afrikaans", "af"),
			new Record("Akan", "ak"),
			new Record("Albanian", "sq"),
			new Record("Amharic", "am"),
			new Record("Arabic", "ar"),
			new Record("Aragonese", "an"),
			new Record("Armenian", "hy"),
			new Record("Assamese", "as"),
			new Record("Avaric", "av"),
			new Record("Aymara", "ay"),
			new Record("Azerbaijani", "az"),
			new Record("Bambara", "bm"),
			new Record("Bashkir", "ba"),
			new Record("Basque", "eu"),
			new Record("Belarusian", "be"),
			new Record("Bengali", "bn"),
			new Record("Bihari", "bh"),
			new Record("Bislama", "bi"),
			new Record("Bosnian", "bs"),
			new Record("Breton", "br"),
			new Record("Bulgarian", "bg"),
			new Record("Burmese", "my"),
			new Record("Catalan", "ca"),
			new Record("Chamorro", "ch"),
			new Record("Chechen", "ce"),
			new Record("Chichewa", "ny"),
			new Record("Chinese", "zh"),
			new Record("Chuvash", "cv"),
			new Record("Cornish", "kw"),
			new Record("Corsican", "co"),
			new Record("Cree", "cr"),
			new Record("Croatian", "hr"),
			new Record("Czech", "cs"),
			new Record("Danish", "da"),
			new Record("Divehi", "dv"),
			new Record("Dutch", "nl"),
			new Record("Dzongkha", "dz"),
			new Record("English", "en"),
			new Record("Estonian", "et"),
			new Record("Ewe", "ee"),
			new Record("Faroese", "fo"),
			new Record("Fijian", "fj"),
			new Record("Finnish", "fi"),
			new Record("French", "fr"),
			new Record("Fula", "ff"),
			new Record("Galician", "gl"),
			new Record("Ganda", "lg"),
			new Record("Georgian", "ka"),
			new Record("German", "de"),
			new Record("Greek", "el"),
			new Record("Guaraní", "gn"),
			new Record("Gujarati", "gu"),
			new Record("Haitian", "ht"),
			new Record("Hausa", "ha"),
			new Record("Hebrew", "he"),
			new Record("Herero", "hz"),
			new Record("Hindi", "hi"),
			new Record("Hiri Motu", "ho"),
			new Record("Hungarian", "hu"),
			new Record("Icelandic", "is"),
			new Record("Ido", "io"),
			new Record("Igbo", "ig"),
			new Record("Indonesian", "id"),
			new Record("Inuktitut", "iu"),
			new Record("Inupiaq", "ik"),
			new Record("Irish", "ga"),
			new Record("Italian", "it"),
			new Record("Japanese", "ja"),
			new Record("Javanese", "jv"),
			new Record("Kalaallisut", "kl"),
			new Record("Kannada", "kn"),
			new Record("Kanuri", "kr"),
			new Record("Kashmiri", "ks"),
			new Record("Kazakh", "kk"),
			new Record("Khmer", "km"),
			new Record("Kikuyu", "ki"),
			new Record("Kinyarwanda", "rw"),
			new Record("Kirundi", "rn"),
			new Record("Komi", "kv"),
			new Record("Kongo", "kg"),
			new Record("Korean", "ko"),
			new Record("Kurdish", "ku"),
			new Record("Kwanyama", "kj"),
			new Record("Kyrgyz", "ky"),
			new Record("Lao", "lo"),
			new Record("Latvian", "lv"),
			new Record("Limburgish", "li"),
			new Record("Lingala", "ln"),
			new Record("Lithuanian", "lt"),
			new Record("Luba-Katanga", "lu"),
			new Record("Luxembourgish", "lb"),
			new Record("Macedonian", "mk"),
			new Record("Malagasy", "mg"),
			new Record("Malay", "ms"),
			new Record("Malayalam", "ml"),
			new Record("Maltese", "mt"),
			new Record("Manx", "gv"),
			new Record("Māori", "mi"),
			new Record("Marathi", "mr"),
			new Record("Marshallese", "mh"),
			new Record("Mongolian", "mn"),
			new Record("Nauru", "na"),
			new Record("Navajo", "nv"),
			new Record("Ndonga", "ng"),
			new Record("Nepali", "ne"),
			new Record("North Ndebele", "nd"),
			new Record("Northern Sami", "se"),
			new Record("Norwegian Bokmål", "nb"),
			new Record("Norwegian Nynorsk", "nn"),
			new Record("Norwegian", "no"),
			new Record("Nuosu", "ii"),
			new Record("Occitan", "oc"),
			new Record("Ojibwe", "oj"),
			new Record("Oriya", "or"),
			new Record("Oromo", "om"),
			new Record("Ossetian", "os"),
			new Record("Panjabi", "pa"),
			new Record("Pashto", "ps"),
			new Record("Persian", "fa"),
			new Record("Polish", "pl"),
			new Record("Portuguese", "pt"),
			new Record("Quechua", "qu"),
			new Record("Romanian", "ro"),
			new Record("Romansh", "rm"),
			new Record("Russian", "ru"),
			new Record("Samoan", "sm"),
			new Record("Sango", "sg"),
			new Record("Sanskrit", "sa"),
			new Record("Sardinian", "sc"),
			new Record("Scottish Gaelic", "gd"),
			new Record("Serbian", "sr"),
			new Record("Shona", "sn"),
			new Record("Sindhi", "sd"),
			new Record("Sinhala", "si"),
			new Record("Slovak", "sk"),
			new Record("Slovenian", "sl"),
			new Record("Somali", "so"),
			new Record("South Ndebele", "nr"),
			new Record("Southern Sotho", "st"),
			new Record("Spanish", "es"),
			new Record("Sundanese", "su"),
			new Record("Swahili", "sw"),
			new Record("Swati", "ss"),
			new Record("Swedish", "sv"),
			new Record("Tagalog", "tl"),
			new Record("Tahitian", "ty"),
			new Record("Tajik", "tg"),
			new Record("Tamil", "ta"),
			new Record("Tatar", "tt"),
			new Record("Telugu", "te"),
			new Record("Thai", "th"),
			new Record("Tibetan", "bo"),
			new Record("Tigrinya", "ti"),
			new Record("Tonga", "to"),
			new Record("Tsonga", "ts"),
			new Record("Tswana", "tn"),
			new Record("Turkish", "tr"),
			new Record("Turkmen", "tk"),
			new Record("Twi", "tw"),
			new Record("Uighur", "ug"),
			new Record("Ukrainian", "uk"),
			new Record("Urdu", "ur"),
			new Record("Uzbek", "uz"),
			new Record("Venda", "ve"),
			new Record("Vietnamese", "vi"),
			new Record("Walloon", "wa"),
			new Record("Welsh", "cy"),
			new Record("Western Frisian", "fy"),
			new Record("Wolof", "wo"),
			new Record("Xhosa", "xh"),
			new Record("Yiddish", "yi"),
			new Record("Yoruba", "yo"),
			new Record("Zhuang", "za"),
			new Record("Zulu", "zu")
		};
	}
}
