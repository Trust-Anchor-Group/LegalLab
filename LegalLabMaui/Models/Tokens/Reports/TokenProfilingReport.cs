using NeuroFeatures;
using NeuroFeatures.EventArguments;
using System;
using System.Threading.Tasks;

namespace LegalLabMaui.Models.Tokens.Reports
{
	/// <summary>
	/// Represents a profiling report of a token and the underlying state-machine.
	/// </summary>
	public class TokenProfilingReport(NeuroFeaturesClient Client, string TokenId)
		: TokenReport(Client, TokenId)
	{
		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult("Profiling");

		/// <summary>
		/// Gets the Markdown text for the report.
		/// </summary>
		public override async Task<string> GetReportMarkdown()
		{
			ReportEventArgs e = await this.client.GenerateProfilingReportAsync(this.TokenId, ReportFormat.Markdown);
			if (!e.Ok)
				throw e.StanzaError ?? new Exception("Unable to get profiling report.");

			return e.ReportText;
		}
	}
}
