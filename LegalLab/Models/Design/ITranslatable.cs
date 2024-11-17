using System.Threading.Tasks;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Interface for translatable objects.
	/// </summary>
	public interface ITranslatable
	{
		/// <summary>
		/// Gets associated texts to translate.
		/// </summary>
		/// <param name="Language">Language to translate from.</param>
		/// <returns>Array of translatable texts, or null if none.</returns>
		Task<string[]> GetTranslatableTexts(string Language);

		/// <summary>
		/// Sets translated texts.
		/// </summary>
		/// <param name="Texts">Available translated texts.</param>
		/// <param name="Language">Language translated to.</param>
		void SetTranslatableTexts(string[] Texts, string Language);
	}
}
