using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Events;
using Waher.Runtime.IO;

namespace LegalLab.Models.XmlEditor
{
	/// <summary>
	/// Interaction logic for the XML editor view.
	/// </summary>
	public class XmlEditorModel : Model
	{
		private readonly Property<string> xml;
		private readonly Property<string> fileName;
		private readonly Property<bool> changed;
		private readonly PersistedProperty<int> editorWidth;

		private readonly Command newDocument;
		private readonly Command loadDocument;
		private readonly Command saveDocument;

		/// <summary>
		/// Interaction logic for the XML editor view.
		/// </summary>
		public XmlEditorModel()
		{
			this.xml = new Property<string>(nameof(this.Xml), string.Empty, this);
			this.fileName = new Property<string>(nameof(this.FileName), string.Empty, this);
			this.changed = new Property<bool>(nameof(this.Changed), false, this);
			this.editorWidth = new PersistedProperty<int>("XML", nameof(this.EditorWidth), true, 500, this);

			this.newDocument = new Command(this.ExecuteNewDocument);
			this.loadDocument = new Command(this.ExecuteLoadDocument);
			this.saveDocument = new Command(this.ExecuteSaveDocument);
		}

		/// <summary>
		/// XML input
		/// </summary>
		public string Xml
		{
			get => this.xml.Value;
			set
			{
				this.xml.Value = value;
				this.changed.Value = true;
			}
		}

		/// <summary>
		/// File Name
		/// </summary>
		public string FileName
		{
			get => this.fileName.Value;
			set => this.fileName.Value = value;
		}

		/// <summary>
		/// If the contents has been changed.
		/// </summary>
		public bool Changed
		{
			get => this.changed.Value;
			set => this.changed.Value = value;
		}

		/// <summary>
		/// File Name
		/// </summary>
		public int EditorWidth
		{
			get => this.editorWidth.Value;
			set => this.editorWidth.Value = Math.Max(value, 50);
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override async Task Start()
		{
			this.SetDataContext();
			await base.Start();
		}

		private async void SetDataContext()
		{
			try
			{
				while (MainWindow.currentInstance.XmlEditorTab is null)
					await Task.Delay(100);

				await MainWindow.UpdateGui(() =>
				{
					MainWindow.currentInstance.XmlEditorTab.DataContext = this;
					return Task.CompletedTask;
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Command for creating a new document.
		/// </summary>
		public ICommand NewDocument => this.newDocument;

		/// <summary>
		/// Creates a new document.
		/// </summary>
		public async Task ExecuteNewDocument()
		{
			if (this.Changed && await this.CanContinue())
			{
				this.Xml = string.Empty;
				this.FileName = string.Empty;
				this.Changed = false;
			}
		}

		private async Task<bool> CanContinue()
		{
			if (this.Changed)
			{
				switch (await MainWindow.MessageBox("Do you want to save your unsaved changes?",
					"Confirm", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question))
				{
					case System.Windows.MessageBoxResult.Yes:
						bool? Result = await this.Save();
						if (!Result.HasValue)
							return false;
						else
							return true;

					case System.Windows.MessageBoxResult.No:
						return true;

					case System.Windows.MessageBoxResult.Cancel:
					default:
						return false;
				}
			}
			else
				return true;
		}

		/// <summary>
		/// Command for loading an existing document.
		/// </summary>
		public ICommand LoadDocument => this.loadDocument;

		/// <summary>
		/// Loads an existing document.
		/// </summary>
		public async Task ExecuteLoadDocument()
		{
			try
			{
				if (!await this.CanContinue())
					return;

				OpenFileDialog Dialog = new()
				{
					AddExtension = true,
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "xml",
					FileName = this.FileName,
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					Title = "Load XML File"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (Result.HasValue && Result.Value)
				{
					this.Xml = await Files.ReadAllTextAsync(Dialog.FileName);
					this.FileName = Dialog.FileName;
					this.Changed = false;
				}
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Command for saving a document.
		/// </summary>
		public ICommand SaveDocument => this.saveDocument;

		/// <summary>
		/// Saves a document.
		/// </summary>
		public async Task ExecuteSaveDocument()
		{
			await this.Save();
		}

		private async Task<bool?> Save()
		{
			try
			{
				SaveFileDialog Dialog = new()
				{
					AddExtension = true,
					CheckFileExists = false,
					CheckPathExists = true,
					CreatePrompt = false,
					DefaultExt = "xml",
					FileName = this.FileName,
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					OverwritePrompt = true,
					Title = "Save XML File"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.currentInstance);
				if (Result.HasValue && Result.Value)
				{
					await Files.WriteAllTextAsync(Dialog.FileName, this.Xml);

					this.FileName = Dialog.FileName;
				}

				return Result;
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
				return null;
			}
		}

	}
}
