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
				Dictionary<string, string> Temp = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

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
			new Record("Afar", "aa"),
			new Record("Abkhaz", "ab"),
			new Record("Afrikaans", "af"),
			new Record("Akan", "ak"),
			new Record("Amharic", "am"),
			new Record("Aragonese", "an"),
			new Record("Arabic", "ar"),
			new Record("Assamese", "as"),
			new Record("Avaric", "av"),
			new Record("Aymara", "ay"),
			new Record("Azerbaijani", "az"),
			new Record("Bashkir", "ba"),
			new Record("Belarusian", "be"),
			new Record("Bulgarian", "bg"),
			new Record("Bihari", "bh"),
			new Record("Bislama", "bi"),
			new Record("Bambara", "bm"),
			new Record("Bengali", "bn"),
			new Record("Tibetan", "bo"),
			new Record("Breton", "br"),
			new Record("Bosnian", "bs"),
			new Record("Catalan", "ca"),
			new Record("Chechen", "ce"),
			new Record("Chamorro", "ch"),
			new Record("Corsican", "co"),
			new Record("Cree", "cr"),
			new Record("Czech", "cs"),
			new Record("Chuvash", "cv"),
			new Record("Welsh", "cy"),
			new Record("Danish", "da"),
			new Record("German", "de"),
			new Record("Divehi", "dv"),
			new Record("Dzongkha", "dz"),
			new Record("Ewe", "ee"),
			new Record("Greek", "el"),
			new Record("English", "en"),
			new Record("Spanish", "es"),
			new Record("Estonian", "et"),
			new Record("Basque", "eu"),
			new Record("Persian", "fa"),
			new Record("Fula", "ff"),
			new Record("Finnish", "fi"),
			new Record("Fijian", "fj"),
			new Record("Faroese", "fo"),
			new Record("French", "fr"),
			new Record("Western Frisian", "fy"),
			new Record("Irish", "ga"),
			new Record("Scottish Gaelic", "gd"),
			new Record("Galician", "gl"),
			new Record("Guaraní", "gn"),
			new Record("Gujarati", "gu"),
			new Record("Manx", "gv"),
			new Record("Hausa", "ha"),
			new Record("Hebrew", "he"),
			new Record("Hindi", "hi"),
			new Record("Hiri Motu", "ho"),
			new Record("Croatian", "hr"),
			new Record("Haitian", "ht"),
			new Record("Hungarian", "hu"),
			new Record("Armenian", "hy"),
			new Record("Herero", "hz"),
			new Record("Indonesian", "id"),
			new Record("Igbo", "ig"),
			new Record("Nuosu", "ii"),
			new Record("Inupiaq", "ik"),
			new Record("Ido", "io"),
			new Record("Icelandic", "is"),
			new Record("Italian", "it"),
			new Record("Inuktitut", "iu"),
			new Record("Japanese", "ja"),
			new Record("Javanese", "jv"),
			new Record("Georgian", "ka"),
			new Record("Kongo", "kg"),
			new Record("Kikuyu", "ki"),
			new Record("Kwanyama", "kj"),
			new Record("Kazakh", "kk"),
			new Record("Kalaallisut", "kl"),
			new Record("Khmer", "km"),
			new Record("Kannada", "kn"),
			new Record("Korean", "ko"),
			new Record("Kanuri", "kr"),
			new Record("Kashmiri", "ks"),
			new Record("Kurdish", "ku"),
			new Record("Komi", "kv"),
			new Record("Cornish", "kw"),
			new Record("Kyrgyz", "ky"),
			new Record("Luxembourgish", "lb"),
			new Record("Ganda", "lg"),
			new Record("Limburgish", "li"),
			new Record("Lingala", "ln"),
			new Record("Lao", "lo"),
			new Record("Lithuanian", "lt"),
			new Record("Luba-Katanga", "lu"),
			new Record("Latvian", "lv"),
			new Record("Malagasy", "mg"),
			new Record("Marshallese", "mh"),
			new Record("Māori", "mi"),
			new Record("Macedonian", "mk"),
			new Record("Malayalam", "ml"),
			new Record("Mongolian", "mn"),
			new Record("Marathi", "mr"),
			new Record("Malay", "ms"),
			new Record("Maltese", "mt"),
			new Record("Burmese", "my"),
			new Record("Nauru", "na"),
			new Record("Norwegian Bokmål", "nb"),
			new Record("North Ndebele", "nd"),
			new Record("Nepali", "ne"),
			new Record("Ndonga", "ng"),
			new Record("Dutch", "nl"),
			new Record("Norwegian Nynorsk", "nn"),
			new Record("Norwegian", "no"),
			new Record("South Ndebele", "nr"),
			new Record("Navajo", "nv"),
			new Record("Chichewa", "ny"),
			new Record("Occitan", "oc"),
			new Record("Ojibwe", "oj"),
			new Record("Oromo", "om"),
			new Record("Oriya", "or"),
			new Record("Ossetian", "os"),
			new Record("Panjabi", "pa"),
			new Record("Polish", "pl"),
			new Record("Pashto", "ps"),
			new Record("Portuguese", "pt"),
			new Record("Quechua", "qu"),
			new Record("Romansh", "rm"),
			new Record("Kirundi", "rn"),
			new Record("Romanian", "ro"),
			new Record("Russian", "ru"),
			new Record("Kinyarwanda", "rw"),
			new Record("Sanskrit", "sa"),
			new Record("Sardinian", "sc"),
			new Record("Sindhi", "sd"),
			new Record("Northern Sami", "se"),
			new Record("Sango", "sg"),
			new Record("Sinhala", "si"),
			new Record("Slovak", "sk"),
			new Record("Slovenian", "sl"),
			new Record("Samoan", "sm"),
			new Record("Shona", "sn"),
			new Record("Somali", "so"),
			new Record("Albanian", "sq"),
			new Record("Serbian", "sr"),
			new Record("Swati", "ss"),
			new Record("Southern Sotho", "st"),
			new Record("Sundanese", "su"),
			new Record("Swedish", "sv"),
			new Record("Swahili", "sw"),
			new Record("Tamil", "ta"),
			new Record("Telugu", "te"),
			new Record("Tajik", "tg"),
			new Record("Thai", "th"),
			new Record("Tigrinya", "ti"),
			new Record("Turkmen", "tk"),
			new Record("Tagalog", "tl"),
			new Record("Tswana", "tn"),
			new Record("Tonga", "to"),
			new Record("Turkish", "tr"),
			new Record("Tsonga", "ts"),
			new Record("Tatar", "tt"),
			new Record("Twi", "tw"),
			new Record("Tahitian", "ty"),
			new Record("Uighur", "ug"),
			new Record("Ukrainian", "uk"),
			new Record("Urdu", "ur"),
			new Record("Uzbek", "uz"),
			new Record("Venda", "ve"),
			new Record("Vietnamese", "vi"),
			new Record("Walloon", "wa"),
			new Record("Wolof", "wo"),
			new Record("Xhosa", "xh"),
			new Record("Yiddish", "yi"),
			new Record("Yoruba", "yo"),
			new Record("Zhuang", "za"),
			new Record("Chinese", "zh"),
			new Record("Zulu", "zu")
		};
	}
}
