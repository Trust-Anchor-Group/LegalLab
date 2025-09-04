namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Contains a detail of information about a token.
	/// </summary>
	/// <param name="Name">Detail name</param>
	/// <param name="Value">Detail value.</param>
	/// <param name="NoteCommand">If detail represents a note command</param>
	public class TokenDetail(string Name, object Value, bool NoteCommand)
	{
		public string Name { get; } = Name;
		public object Value { get; } = Value;
		public bool NoteCommand { get; } = NoteCommand;
	}
}
