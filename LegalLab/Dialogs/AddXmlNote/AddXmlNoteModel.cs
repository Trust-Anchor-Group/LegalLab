using LegalLab.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content.Xml;

namespace LegalLab.Dialogs.AddXmlNote
{
	/// <summary>
	/// View model for the Add Language dialog.
	/// </summary>
	public class AddXmlNoteModel : Model
	{
		private readonly Property<string> xmlInput;
		private readonly AddXmlNoteDialog dialog;
		private readonly Command add;
		private readonly Command cancel;

		public AddXmlNoteModel(AddXmlNoteDialog Dialog)
		{
			this.xmlInput = new Property<string>(nameof(this.XmlInput), string.Empty, this);

			this.dialog = Dialog;
			this.dialog.DataContext = this;

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
				this.dialog.DialogResult = true;

			return Task.CompletedTask;
		}

		private Task ExecuteCancel()
		{
			this.dialog.DialogResult = false;
			return Task.CompletedTask;
		}
	}
}
