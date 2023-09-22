namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Contains a detail of information about a token.
	/// </summary>
	public class TokenDetail
	{
		/// <summary>
		/// Contains a detail of information about a token.
		/// </summary>
		/// <param name="Name">Detail name</param>
		/// <param name="Value">Detail value.</param>
		/// <param name="NoteCommand">If detail represents a note command</param>
		public TokenDetail(string Name, object Value, bool NoteCommand)
		{
			this.Name = Name;
			this.Value = Value;
			this.NoteCommand = NoteCommand;
		}

		public string Name { get; }
		public object Value { get; }
		public bool NoteCommand { get; }
	}
}
