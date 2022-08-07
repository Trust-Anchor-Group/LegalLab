﻿using NeuroFeatures;
using System;
using System.Threading.Tasks;

namespace LegalLab.Models.Tokens.Reports
{
	/// <summary>
	/// Represent a report of the historical states of a token and the underlying state-machine.
	/// </summary>
	public class TokenHistoryReport : TokenReport
	{
		public TokenHistoryReport(NeuroFeaturesClient Client, string TokenId)
			: base(Client, TokenId)
		{
		}

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult<string>("History");

		/// <summary>
		/// Gets the XAML for the report.
		/// </summary>
		/// <returns>String-representation of XAML of report.</returns>
		public override async Task<string> GetReportXaml()
		{
			ReportEventArgs e = await this.client.GenerateHistoryReportAsync(this.TokenId, ReportFormat.Xaml);
			if (!e.Ok)
				throw e.StanzaError ?? new Exception("Unable to get history report.");

			return e.ReportText;
		}
	}
}