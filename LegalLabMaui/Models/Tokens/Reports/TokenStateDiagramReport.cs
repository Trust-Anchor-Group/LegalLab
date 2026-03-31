using NeuroFeatures;
using NeuroFeatures.EventArguments;
using System;
using System.Threading.Tasks;

namespace LegalLabMaui.Models.Tokens.Reports
{
	/// <summary>
	/// Represents a state diagram of a token and the underlying state-machine.
	/// </summary>
	public class TokenStateDiagramReport(NeuroFeaturesClient Client, string TokenId)
		: TokenReport(Client, TokenId)
	{
		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult("State Diagram");

		/// <summary>
		/// Gets the Markdown text for the report.
		/// </summary>
		public override async Task<string> GetReportMarkdown()
		{
			ReportEventArgs e = await this.client.GenerateStateDiagramAsync(this.TokenId, ReportFormat.Markdown);
			if (!e.Ok)
				throw e.StanzaError ?? new Exception("Unable to get state diagram report.");

			return e.ReportText;
		}
	}
}
