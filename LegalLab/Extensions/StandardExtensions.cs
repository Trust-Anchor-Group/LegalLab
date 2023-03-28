using LegalLab.Models.Standards;

namespace LegalLab.Extensions
{
	/// <summary>
	/// Static class with standard extensions.
	/// </summary>
	public static class StandardExtensions
	{
		/// <summary>
		/// Converts a language code to an ISO 639-1 record.
		/// </summary>
		/// <param name="LanguageCode">Language code</param>
		/// <returns>ISO 639-1 record</returns>
		public static Iso__639_1.Record ToIso639_1(this string LanguageCode)
		{
			if (Iso__639_1.CodeToLanguage(LanguageCode, out string Language))
				return new Iso__639_1.Record(Language, LanguageCode);
			else
				return new Iso__639_1.Record(LanguageCode, LanguageCode);
		}

		/// <summary>
		/// Converts an array of language codes to an array of ISO 639-1 records.
		/// </summary>
		/// <param name="LanguageCodes">Language codes</param>
		/// <returns>ISO 639-1 records</returns>
		public static Iso__639_1.Record[] ToIso639_1(this string[] LanguageCodes)
		{
			int i, c = LanguageCodes.Length;
			Iso__639_1.Record[] Result = new Iso__639_1.Record[c];

			for (i = 0; i < c; i++)
				Result[i] = LanguageCodes[i].ToIso639_1();

			return Result;
		}

		/// <summary>
		/// Converts an array of ISO 639-1 language records to an array of language codes.
		/// </summary>
		/// <param name="Languages">Language records.</param>
		/// <returns>Language codes.</returns>
		public static string[] ToCodes(this Iso__639_1.Record[] Languages)
		{
			int i, c = Languages.Length;
			string[] Result = new string[c];

			for (i = 0; i < c; i++)
				Result[i] = Languages[i].Code;

			return Result;
		}
	}
}
