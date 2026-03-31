using NeuroFeatures;
using NeuroFeatures.EventArguments;
using System;
using System.Threading.Tasks;

namespace LegalLabMaui.Models.Tokens.Reports
{
	/// <summary>
	/// Abstract base class for token reports.
	/// </summary>
	public abstract class TokenReport(NeuroFeaturesClient Client, string TokenId)
		: IDelayedAction
	{
		private readonly string tokenId = TokenId;

		/// <summary>
		/// Reference to the Neuro-Features client.
		/// </summary>
		protected readonly NeuroFeaturesClient client = Client;

		/// <summary>
		/// Token ID associated with the state-machine.
		/// </summary>
		public string TokenId => this.tokenId;

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public abstract Task<string> GetTitle();

		/// <summary>
		/// Gets the Markdown text for the report.
		/// </summary>
		/// <returns>Markdown string.</returns>
		public abstract Task<string> GetReportMarkdown();

		/// <summary>
		/// Method called when the underlying state-machine has changed state.
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		public virtual Task OnNewState(object Sender, NewStateEventArgs e)
		{
			DelayedActions.Add(this, DateTime.Now.AddSeconds(1));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Method called when variables in the underlying state-machine have been updated.
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		public virtual Task OnVariablesUpdated(object Sender, VariablesUpdatedEventArgs e)
		{
			DelayedActions.Add(this, DateTime.Now.AddSeconds(1));
			return Task.CompletedTask;
		}

		/// <summary>
		/// When action is scheduled.
		/// </summary>
		public DateTime ScheduledFor { get; set; }

		/// <summary>
		/// Whether the action has changed since last executed.
		/// </summary>
		public bool Changed => false;

		/// <summary>
		/// Method called when it is time to execute action.
		/// This raises <see cref="ReportUpdated"/> so the UI layer can refresh.
		/// </summary>
		public async Task Action()
		{
			try
			{
				string Markdown = await this.GetReportMarkdown();
				this.ReportUpdated?.Invoke(this, new ReportUpdatedEventArgs(Markdown));
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Raised when a fresh report Markdown is available (e.g. after a state-machine update).
		/// </summary>
		public event EventHandler<ReportUpdatedEventArgs>? ReportUpdated;
	}

	/// <summary>
	/// Event arguments carrying the refreshed Markdown text of a report.
	/// </summary>
	public sealed class ReportUpdatedEventArgs(string Markdown) : EventArgs
	{
		/// <summary>Updated Markdown content.</summary>
		public string Markdown { get; } = Markdown;
	}
}
