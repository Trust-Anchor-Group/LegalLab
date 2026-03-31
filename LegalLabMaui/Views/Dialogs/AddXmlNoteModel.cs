using LegalLabMaui.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content.Xml;
using Command = LegalLabMaui.Models.Command;

namespace LegalLabMaui.Views.Dialogs
{
	/// <summary>
	/// View model for the Add XML Note dialog.
	/// </summary>
	public class AddXmlNoteModel : Model
	{
		private readonly Property<string> xmlInput;
		private readonly Command add;
		private readonly Command cancel;

		/// <summary>
		/// Completion source resolved when the user confirms or cancels.
		/// True = confirmed, False = cancelled.
		/// </summary>
		private System.Threading.Tasks.TaskCompletionSource<bool> tcs = new();

		public AddXmlNoteModel()
		{
			this.xmlInput = new Property<string>(nameof(this.XmlInput), string.Empty, this);

			this.add = new Command(this.CanExecuteAdd, this.ExecuteAdd);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// XML Input
		/// </summary>
		public string XmlInput
		{
			get => this.xmlInput.Value;
			set
			{
				this.xmlInput.Value = value;
				this.add.RaiseCanExecuteChanged();
			}
		}

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
			return this.IsXmlValid;
		}

		private bool IsXmlValid => XML.IsValidXml(this.xmlInput.Value);

		private Task ExecuteAdd()
		{
			if (this.IsXmlValid)
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
		/// Returns true if the user clicked Add with valid XML, false if cancelled.
		/// </summary>
		public Task<bool> WaitForResult() => this.tcs.Task;

		/// <summary>
		/// Resets the completion source so the model can be reused.
		/// </summary>
		public void Reset() => this.tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
	}
}
