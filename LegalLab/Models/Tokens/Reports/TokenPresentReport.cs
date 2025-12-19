using NeuroFeatures;
using NeuroFeatures.EventArguments;
using System;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Wpf;

namespace LegalLab.Models.Tokens.Reports
{
	/// <summary>
	/// Represent a report of the present state of a token and the underlying state-machine.
	/// </summary>
	public class TokenPresentReport(NeuroFeaturesClient Client, string TokenId) 
		: TokenReport(Client, TokenId)
	{
		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult("Present");

		/// <summary>
		/// Gets the XAML for the report.
		/// </summary>
		/// <returns>String-representation of XAML of report.</returns>
		public override async Task<string> GetReportXaml()
		{
			ReportEventArgs e = await this.client.GeneratePresentReportAsync(this.TokenId, ReportFormat.Markdown);
			if (!e.Ok)
				throw e.StanzaError ?? new Exception("Unable to get present report.");

			string Markdown = e.ReportText;
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			string Xaml = await Doc.GenerateXAML();

			return Xaml;
		}
	}
}
