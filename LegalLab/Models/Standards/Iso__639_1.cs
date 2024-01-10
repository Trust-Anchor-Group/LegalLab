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
			new("Abkhaz", "ab"),
			new("Afar", "aa"),
			new("Afrikaans", "af"),
			new("Akan", "ak"),
			new("Albanian", "sq"),
			new("Amharic", "am"),
			new("Arabic", "ar"),
			new("Aragonese", "an"),
			new("Armenian", "hy"),
			new("Assamese", "as"),
			new("Avaric", "av"),
			new("Aymara", "ay"),
			new("Azerbaijani", "az"),
			new("Bambara", "bm"),
			new("Bashkir", "ba"),
			new("Basque", "eu"),
			new("Belarusian", "be"),
			new("Bengali", "bn"),
			new("Bihari", "bh"),
			new("Bislama", "bi"),
			new("Bosnian", "bs"),
			new("Breton", "br"),
			new("Bulgarian", "bg"),
			new("Burmese", "my"),
			new("Catalan", "ca"),
			new("Chamorro", "ch"),
			new("Chechen", "ce"),
			new("Chichewa", "ny"),
			new("Chinese", "zh"),
			new("Chuvash", "cv"),
			new("Cornish", "kw"),
			new("Corsican", "co"),
			new("Cree", "cr"),
			new("Croatian", "hr"),
			new("Czech", "cs"),
			new("Danish", "da"),
			new("Divehi", "dv"),
			new("Dutch", "nl"),
			new("Dzongkha", "dz"),
			new("English", "en"),
			new("Estonian", "et"),
			new("Ewe", "ee"),
			new("Faroese", "fo"),
			new("Fijian", "fj"),
			new("Finnish", "fi"),
			new("French", "fr"),
			new("Fula", "ff"),
			new("Galician", "gl"),
			new("Ganda", "lg"),
			new("Georgian", "ka"),
			new("German", "de"),
			new("Greek", "el"),
			new("Guaraní", "gn"),
			new("Gujarati", "gu"),
			new("Haitian", "ht"),
			new("Hausa", "ha"),
			new("Hebrew", "he"),
			new("Herero", "hz"),
			new("Hindi", "hi"),
			new("Hiri Motu", "ho"),
			new("Hungarian", "hu"),
			new("Icelandic", "is"),
			new("Ido", "io"),
			new("Igbo", "ig"),
			new("Indonesian", "id"),
			new("Inuktitut", "iu"),
			new("Inupiaq", "ik"),
			new("Irish", "ga"),
			new("Italian", "it"),
			new("Japanese", "ja"),
			new("Javanese", "jv"),
			new("Kalaallisut", "kl"),
			new("Kannada", "kn"),
			new("Kanuri", "kr"),
			new("Kashmiri", "ks"),
			new("Kazakh", "kk"),
			new("Khmer", "km"),
			new("Kikuyu", "ki"),
			new("Kinyarwanda", "rw"),
			new("Kirundi", "rn"),
			new("Komi", "kv"),
			new("Kongo", "kg"),
			new("Korean", "ko"),
			new("Kurdish", "ku"),
			new("Kwanyama", "kj"),
			new("Kyrgyz", "ky"),
			new("Lao", "lo"),
			new("Latvian", "lv"),
			new("Limburgish", "li"),
			new("Lingala", "ln"),
			new("Lithuanian", "lt"),
			new("Luba-Katanga", "lu"),
			new("Luxembourgish", "lb"),
			new("Macedonian", "mk"),
			new("Malagasy", "mg"),
			new("Malay", "ms"),
			new("Malayalam", "ml"),
			new("Maltese", "mt"),
			new("Manx", "gv"),
			new("Māori", "mi"),
			new("Marathi", "mr"),
			new("Marshallese", "mh"),
			new("Mongolian", "mn"),
			new("Nauru", "na"),
			new("Navajo", "nv"),
			new("Ndonga", "ng"),
			new("Nepali", "ne"),
			new("North Ndebele", "nd"),
			new("Northern Sami", "se"),
			new("Norwegian Bokmål", "nb"),
			new("Norwegian Nynorsk", "nn"),
			new("Norwegian", "no"),
			new("Nuosu", "ii"),
			new("Occitan", "oc"),
			new("Ojibwe", "oj"),
			new("Oriya", "or"),
			new("Oromo", "om"),
			new("Ossetian", "os"),
			new("Panjabi", "pa"),
			new("Pashto", "ps"),
			new("Persian", "fa"),
			new("Polish", "pl"),
			new("Portuguese", "pt"),
			new("Quechua", "qu"),
			new("Romanian", "ro"),
			new("Romansh", "rm"),
			new("Russian", "ru"),
			new("Samoan", "sm"),
			new("Sango", "sg"),
			new("Sanskrit", "sa"),
			new("Sardinian", "sc"),
			new("Scottish Gaelic", "gd"),
			new("Serbian", "sr"),
			new("Shona", "sn"),
			new("Sindhi", "sd"),
			new("Sinhala", "si"),
			new("Slovak", "sk"),
			new("Slovenian", "sl"),
			new("Somali", "so"),
			new("South Ndebele", "nr"),
			new("Southern Sotho", "st"),
			new("Spanish", "es"),
			new("Sundanese", "su"),
			new("Swahili", "sw"),
			new("Swati", "ss"),
			new("Swedish", "sv"),
			new("Tagalog", "tl"),
			new("Tahitian", "ty"),
			new("Tajik", "tg"),
			new("Tamil", "ta"),
			new("Tatar", "tt"),
			new("Telugu", "te"),
			new("Thai", "th"),
			new("Tibetan", "bo"),
			new("Tigrinya", "ti"),
			new("Tonga", "to"),
			new("Tsonga", "ts"),
			new("Tswana", "tn"),
			new("Turkish", "tr"),
			new("Turkmen", "tk"),
			new("Twi", "tw"),
			new("Uighur", "ug"),
			new("Ukrainian", "uk"),
			new("Urdu", "ur"),
			new("Uzbek", "uz"),
			new("Venda", "ve"),
			new("Vietnamese", "vi"),
			new("Walloon", "wa"),
			new("Welsh", "cy"),
			new("Western Frisian", "fy"),
			new("Wolof", "wo"),
			new("Xhosa", "xh"),
			new("Yiddish", "yi"),
			new("Yoruba", "yo"),
			new("Zhuang", "za"),
			new("Zulu", "zu")
		};
	}
}
