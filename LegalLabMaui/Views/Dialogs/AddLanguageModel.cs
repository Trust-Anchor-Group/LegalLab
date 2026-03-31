using LegalLabMaui.Models;
using LegalLabMaui.Models.Standards;
using System.Threading.Tasks;
using System.Windows.Input;

using Command = LegalLabMaui.Models.Command;

namespace LegalLabMaui.Views.Dialogs
{
	/// <summary>
	/// View model for the Add Language dialog.
	/// </summary>
	public class AddLanguageModel : Model
	{
		private readonly Property<string> selectedLanguage;
		private readonly Command add;
		private readonly Command cancel;

		/// <summary>
		/// Completion source resolved when the user confirms or cancels.
		/// True = confirmed, False = cancelled.
		/// </summary>
		private System.Threading.Tasks.TaskCompletionSource<bool> tcs = new();

		public AddLanguageModel()
		{
			this.selectedLanguage = new Property<string>(nameof(this.SelectedLanguage), string.Empty, this);

			this.add = new Command(this.CanExecuteAdd, this.ExecuteAdd);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// Language selected
		/// </summary>
		public string SelectedLanguage
		{
			get => this.selectedLanguage.Value;
			set
			{
				this.selectedLanguage.Value = value;
				this.add.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// ISO 639-1 language codes
		/// </summary>
		public static Iso__639_1.Record[] Languages => Iso__639_1.Data;

		/// <summary>
		/// Add Command
		/// </summary>
		public ICommand Add => this.add;

		/// <summary>
		/// Cancel Command
		/// </summary>
		public ICommand Cancel => this.cancel;

		private bool CanExecuteAdd()
		{
			return !string.IsNullOrEmpty(this.SelectedLanguage) && Iso__639_1.CodeToLanguage(this.SelectedLanguage, out _);
		}

		private Task ExecuteAdd()
		{
			this.tcs.TrySetResult(true);
			return Task.CompletedTask;
		}

		private Task ExecuteCancel()
		{
			this.tcs.TrySetResult(false);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits for the user to confirm or cancel.
		/// Returns true if the user clicked Add, false if cancelled.
		/// </summary>
		public Task<bool> WaitForResult() => this.tcs.Task;

		/// <summary>
		/// Resets the completion source so the model can be reused.
		/// </summary>
		public void Reset() => this.tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
	}
}
