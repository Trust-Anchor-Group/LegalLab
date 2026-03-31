namespace LegalLabMaui.Views.Dialogs
{
	using System.Windows.Input;
	using Command = LegalLabMaui.Models.Command;

	/// <summary>
	/// View model for the prompt dialog.
	/// Replaces the WPF PromptDialog / ShowDialog pattern — callers should use
	/// <see cref="AppService.PromptUser"/> which delegates to
	/// Shell.Current.DisplayPromptAsync on MAUI.
	/// This class is retained as a plain data-holder for callers that construct
	/// a prompt model explicitly before navigation.
	/// </summary>
	public class PromptModel
	{
		private readonly ICommand ok;
		private readonly ICommand cancel;

		public PromptModel()
		{
			this.ok = new Command(() => Task.CompletedTask);
			this.cancel = new Command(() => Task.CompletedTask);
		}

		/// <summary>
		/// Title of dialog
		/// </summary>
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Text label presented to user
		/// </summary>
		public string Label { get; set; } = string.Empty;

		/// <summary>
		/// Text being edited / result after prompt completes
		/// </summary>
		public string Text { get; set; } = string.Empty;

		/// <summary>
		/// Text of the OK button
		/// </summary>
		public string OkText { get; set; } = "OK";

		/// <summary>
		/// Text of the Cancel button
		/// </summary>
		public string CancelText { get; set; } = "Cancel";

		public ICommand Ok => this.ok;

		public ICommand Cancel => this.cancel;

		/// <summary>
		/// Shows the prompt using the MAUI Shell and stores the result in <see cref="Text"/>.
		/// Returns true if the user confirmed, false if cancelled.
		/// </summary>
		public async System.Threading.Tasks.Task<bool> ShowAsync()
		{
			string? Result = await AppService.PromptUser(this.Title, this.Label, this.Text, this.OkText, this.CancelText);
			if (Result is null)
				return false;

			this.Text = Result;
			return true;
		}
	}
}
