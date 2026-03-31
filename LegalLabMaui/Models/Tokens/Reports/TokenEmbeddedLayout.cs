using NeuroFeatures;
using SkiaSharp;
using System;
using System.Threading.Tasks;
using Waher.Script;

namespace LegalLabMaui.Models.Tokens.Reports
{
	/// <summary>
	/// Represents the embedded layout of a token.
	/// </summary>
	public class TokenEmbeddedLayout(NeuroFeaturesClient Client, Token Token)
		: TokenReport(Client, Token.TokenId)
	{
		private readonly Token token = Token;

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult("Layout");

		/// <summary>
		/// Gets the report as a Markdown string embedding a base64-encoded PNG image.
		/// </summary>
		public override async Task<string> GetReportMarkdown()
		{
			Variables Variables = [];
			using SKImage Image = await this.token.RenderEmbeddedLayout(Variables);
			using SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100);
			byte[] Bin = Data.ToArray();
			string Base64 = Convert.ToBase64String(Bin);

			// Return Markdown with an embedded data URI image so the UI can render it.
			return $"![Layout](data:image/png;base64,{Base64})";
		}
	}
}
