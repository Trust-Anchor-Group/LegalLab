using LegalLab.Models;
using System;
using System.Windows.Input;

namespace LegalLab.Dialogs.Prompt
{
	/// <summary>
	/// View model for the prompt dialog.
	/// </summary>
	public class PromptModel
	{
		private readonly PromptDialog dialog;
		private readonly Command ok;
		private readonly Command cancel;

		public PromptModel(PromptDialog Dialog)
		{
			this.dialog = Dialog;
			this.dialog.DataContext = this;

			this.ok = new Command(this.ExecuteOk);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// Title of window
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Text label presented to user
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Text being edited
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Text of the OK button
		/// </summary>
		public string OkText { get; set; }

		/// <summary>
		/// Text of the Cancel button
		/// </summary>
		public string CancelText { get; set; }

		/// <summary>
		/// OK Command
		/// </summary>
		public ICommand Ok => this.ok;

		/// <summary>
		/// Cancel Command
		/// </summary>
		public ICommand Cancel => this.cancel;

		private void ExecuteOk()
		{
			this.dialog.DialogResult = true;
		}

		private void ExecuteCancel()
		{
			this.dialog.DialogResult = false;
		}
	}
}
