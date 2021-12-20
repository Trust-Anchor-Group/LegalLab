using LegalLab.Models;
using LegalLab.Models.Standards;
using System;
using System.Windows.Input;

namespace LegalLab.Dialogs.AddLanguage
{
	/// <summary>
	/// View model for the Add Language dialog.
	/// </summary>
	public class AddLanguageModel : Model
	{
		private readonly Property<string> language;
		private readonly AddLanguageDialog dialog;
		private readonly Command add;
		private readonly Command cancel;

		public AddLanguageModel(AddLanguageDialog Dialog)
		{
			this.language = new Property<string>(nameof(Language), string.Empty, this);

			this.dialog = Dialog;
			this.dialog.DataContext = this;

			this.add = new Command(this.CanExecuteAdd, this.ExecuteAdd);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// Language selected
		/// </summary>
		public string Language
		{
			get => this.language.Value;
			set
			{
				this.language.Value = value;
				this.add.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// ISO 639-1 language codes
		/// </summary>
		public Iso__639_1.Record[] Languages
		{
			get => Iso__639_1.Data;
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
			return !string.IsNullOrEmpty(this.Language) && Iso__639_1.CodeToLanguage(this.Language, out _);
		}

		private void ExecuteAdd()
		{
			this.dialog.DialogResult = true;
		}

		private void ExecuteCancel()
		{
			this.dialog.DialogResult = false;
		}
	}
}
