using NeuroFeatures;
using System;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Wpf;

namespace LegalLab.Models.Tokens.Reports
{
	/// <summary>
	/// Represent a profiling report of a token and the underlying state-machine.
	/// </summary>
	public class TokenProfilingReport : TokenReport
	{
		public TokenProfilingReport(NeuroFeaturesClient Client, string TokenId)
			: base(Client, TokenId)
		{
		}

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult<string>("Profiling");

		/// <summary>
		/// Gets the XAML for the report.
		/// </summary>
		/// <returns>String-representation of XAML of report.</returns>
		public override async Task<string> GetReportXaml()
		{
			ReportEventArgs e = await this.client.GenerateProfilingReportAsync(this.TokenId, ReportFormat.Markdown);
			if (!e.Ok)
				throw e.StanzaError ?? new Exception("Unable to get profiling report.");

			string Markdown = e.ReportText;
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			string Xaml = await Doc.GenerateXAML();

			return Xaml;
		}
	}
}
