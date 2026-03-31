using LegalLabMaui.Models.Script;
using System;
using System.IO;
using System.Windows.Input;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Script.Graphs;

namespace LegalLabMaui.Models.XmlEditor
{
	/// <summary>
	/// Interaction logic for the XML editor view.
	/// </summary>
	public class XmlEditorModel : PersistedModel
	{
		private readonly Property<string> xml;
		private readonly Property<string> fileName;
		private readonly Property<bool> changed;
		private readonly Property<string> visualizationText;
		private readonly Property<bool> hasVisualizationError;
		private readonly Property<double> zoom;
		private readonly PersistedProperty<double> selectedZoom;
		private readonly PersistedProperty<double> editorWidth;

		private readonly Command newDocument;
		private readonly Command loadDocument;
		private readonly Command saveDocument;
		private readonly Command copyXmlCommand;

		private Timer? timer;

		/// <summary>
		/// Interaction logic for the XML editor view.
		/// </summary>
		public XmlEditorModel()
			: base()
		{
			this.xml = new Property<string>(nameof(this.Xml), string.Empty, this);
			this.fileName = new Property<string>(nameof(this.FileName), string.Empty, this);
			this.changed = new Property<bool>(nameof(this.Changed), false, this);
			this.visualizationText = new Property<string>(nameof(this.VisualizationText), string.Empty, this);
			this.hasVisualizationError = new Property<bool>(nameof(this.HasVisualizationError), false, this);
			this.zoom = new Property<double>(nameof(this.Zoom), 1, this);

			this.Add(this.selectedZoom = new PersistedProperty<double>("XML", nameof(this.SelectedZoom), true, 1, this));
			this.Add(this.editorWidth = new PersistedProperty<double>("XML", nameof(this.EditorWidth), true, 500, this));

			this.newDocument = new Command(this.ExecuteNewDocument);
			this.loadDocument = new Command(this.ExecuteLoadDocument);
			this.saveDocument = new Command(this.ExecuteSaveDocument);
			this.copyXmlCommand = new Command(this.ExecuteCopyXml);
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
				this.ContentsChanged();
			}
		}

		/// <summary>
		/// Selected Visualization Zoom level
		/// </summary>
		public double SelectedZoom
		{
			get => this.selectedZoom.Value;
			set
			{
				this.selectedZoom.Value = value;
				this.Zoom = value > 0 ? value : 1;
			}
		}

		/// <summary>
		/// Visualization Zoom
		/// </summary>
		public double Zoom
		{
			get => this.zoom.Value;
			set => this.zoom.Value = value;
		}

		/// <summary>
		/// Text result of XML visualization / parsing (error message or formatted content).
		/// </summary>
		public string VisualizationText
		{
			get => this.visualizationText.Value;
			set => this.visualizationText.Value = value;
		}

		/// <summary>
		/// True when VisualizationText contains an error message.
		/// </summary>
		public bool HasVisualizationError
		{
			get => this.hasVisualizationError.Value;
			set => this.hasVisualizationError.Value = value;
		}

		private void ContentsChanged()
		{
			this.changed.Value = true;

			if (this.timer is not null)
			{
				this.timer.Dispose();
				this.timer = null;
			}

			this.timer = new Timer(this.UpdateVisualization, null, 1000, Timeout.Infinite);
		}

		/// <summary>
		/// Triggers XML visualization update immediately.
		/// </summary>
		public void UpdateVisualization()
		{
			this.UpdateVisualization(null);
		}

		private async void UpdateVisualization(object _)
		{
			try
			{
				string? VisualizationResult = null;
				bool IsError = false;
				IXmlVisualizer? Visualizer = null;
				XmlDocument? Doc = null;
				Variables? Variables = null;

				if (!string.IsNullOrWhiteSpace(this.Xml))
				{
					try
					{
						Doc = new()
						{
							PreserveWhitespace = true
						};

						Doc.LoadXml(this.Xml);

						Visualizer = Types.FindBest<IXmlVisualizer, XmlDocument>(Doc);

						if (Visualizer is not null)
						{
							Variables = ScriptModel.Variables ?? [];
							object Result = await Visualizer.TransformXml(Doc, Variables);

							if (Result is MarkdownDocument Markdown)
							{
								VisualizationResult = await Markdown.GenerateHTML(ScriptModel.GetHtmlSettings());
							}
							else if (Result is string s)
							{
								VisualizationResult = s;
							}
							else if (Result is Graph G)
							{
								VisualizationResult = "(Graph result - not renderable as text)";
							}
							else if (Result is not null)
							{
								VisualizationResult = Result.ToString();
							}
						}
					}
					catch (Exception ex)
					{
						if (ex is XmlException XmlException)
						{
							StringBuilder sb = new();
							sb.Append("Line: ");
							sb.AppendLine(XmlException.LineNumber.ToString());
							sb.Append("Position: ");
							sb.AppendLine(XmlException.LinePosition.ToString());
							sb.AppendLine();
							sb.Append(XmlException.Message);
							VisualizationResult = sb.ToString();
						}
						else
						{
							VisualizationResult = ex.Message;
						}
						IsError = true;
					}
				}

				await AppService.UpdateGui(() =>
				{
					if (VisualizationResult is null)
					{
						if (Doc is null)
						{
							this.VisualizationText = "No visualizer available for this type of XML document.";
							this.HasVisualizationError = false;
						}
						else
						{
							this.VisualizationText = string.Empty;
							this.HasVisualizationError = false;
						}
					}
					else
					{
						this.VisualizationText = VisualizationResult;
						this.HasVisualizationError = IsError;
					}

					return Task.CompletedTask;
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
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
		/// Editor width preference
		/// </summary>
		public double EditorWidth
		{
			get => this.editorWidth.Value;
			set => this.editorWidth.Value = Math.Max(value, 50);
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override Task Start()
		{
			return base.Start();
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
			if (!this.Changed || await this.CanContinue())
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
				bool Save = await AppService.MessageBox(
					"Do you want to save your unsaved changes?",
					"Confirm",
					IsQuestion: true);

				if (Save)
				{
					bool? Result = await this.SaveXml();
					return Result.HasValue;
				}
				else
				{
					// Treat "Cancel" as do-not-continue; "No" (false) means discard and continue.
					// Since MessageBox returns bool (OK=true, Cancel=false), false here means cancel.
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

				FileResult Result = await FilePicker.PickAsync(new PickOptions()
				{
					PickerTitle = "Load XML File",
					FileTypes = new FilePickerFileType(new System.Collections.Generic.Dictionary<DevicePlatform, System.Collections.Generic.IEnumerable<string>>
					{
						{ DevicePlatform.WinUI, new[] { ".xml" } },
						{ DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
						{ DevicePlatform.iOS, new[] { "public.xml" } },
						{ DevicePlatform.MacCatalyst, new[] { "public.xml" } }
					})
				});

				if (Result is null)
					return;

				string Content;
				using (Stream stream = await Result.OpenReadAsync())
				using (StreamReader reader = new(stream, Encoding.UTF8))
				{
					Content = await reader.ReadToEndAsync();
				}

				this.Xml = Content;
				this.FileName = Result.FileName;
				this.Changed = false;
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
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
			await this.SaveXml();
		}

		private async Task<bool?> SaveXml()
		{
			try
			{
				string SafeName = string.IsNullOrEmpty(this.FileName)
					? "document.xml"
					: Path.GetFileName(this.FileName);

				string TargetPath = Path.Combine(FileSystem.AppDataDirectory, SafeName);

				await Files.WriteAllTextAsync(TargetPath, this.Xml);

				this.FileName = TargetPath;
				this.Changed = false;

				AppService.SuccessBox("File saved to: " + TargetPath);
				return true;
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Command for copying XML to clipboard.
		/// </summary>
		public ICommand CopyXmlCommand => this.copyXmlCommand;

		/// <summary>
		/// Copies current XML to clipboard.
		/// </summary>
		public async Task ExecuteCopyXml()
		{
			try
			{
				await Clipboard.SetTextAsync(this.Xml);
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
		}
	}
}
