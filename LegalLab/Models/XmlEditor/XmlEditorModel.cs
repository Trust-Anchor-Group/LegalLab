using LegalLab.Models.Script;
using Microsoft.Win32;
using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Wpf;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Script.Graphs;

namespace LegalLab.Models.XmlEditor
{
	/// <summary>
	/// Interaction logic for the XML editor view.
	/// </summary>
	public class XmlEditorModel : PersistedModel
	{
		private readonly ScrollViewer scrollViewer;

		private readonly Property<string> xml;
		private readonly Property<string> fileName;
		private readonly Property<bool> changed;
		private readonly Property<object> visualization;
		private readonly Property<double> zoom;
		private readonly PersistedProperty<double> selectedZoom;
		private readonly PersistedProperty<double> editorWidth;

		private readonly Command newDocument;
		private readonly Command loadDocument;
		private readonly Command saveDocument;

		private Timer timer = null;

		/// <summary>
		/// Interaction logic for the XML editor view.
		/// </summary>
		/// <param name="ScrollViewer">Scroll Viewer that will display visualization.</param>
		public XmlEditorModel(ScrollViewer ScrollViewer)
			: base()
		{
			this.scrollViewer = ScrollViewer;

			this.xml = new Property<string>(nameof(this.Xml), string.Empty, this);
			this.fileName = new Property<string>(nameof(this.FileName), string.Empty, this);
			this.changed = new Property<bool>(nameof(this.Changed), false, this);
			this.visualization = new Property<object>(nameof(this.Visualization), null, this);
			this.zoom = new Property<double>(nameof(this.Zoom), 1, this);

			this.Add(this.selectedZoom = new PersistedProperty<double>("XML", nameof(this.SelectedZoom), true, 1, this));
			this.Add(this.editorWidth = new PersistedProperty<double>("XML", nameof(this.EditorWidth), true, 500, this));

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
				this.CalcZoom(value, null, null);
			}
		}

		private void CalcZoom(double SelectedZoom, double? ActualWidth, double? ActualHeight)
		{
			if (SelectedZoom <= 0)
			{
				if (this.Visualization is not FrameworkElement Element)
					this.Zoom = 1;
				else
				{
					double AvailableWidth = this.scrollViewer.ViewportWidth;
					double AvailableHeight = this.scrollViewer.ViewportHeight;

					ActualWidth ??= Element.ActualWidth;
					ActualHeight ??= Element.ActualHeight;

					if (ActualWidth.Value > 0 && ActualHeight.Value > 0)
					{
						double ScaleWidth = AvailableWidth / ActualWidth.Value;
						double ScaleHeight = AvailableHeight / ActualHeight.Value;

						this.Zoom = Math.Min(ScaleWidth, ScaleHeight);
					}
				}
			}
			else
				this.Zoom = SelectedZoom;
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
		/// Visualization of XML input, if any.
		/// </summary>
		public object Visualization
		{
			get => this.visualization.Value;
			set
			{
				object Prev = this.visualization.Value;
				this.visualization.Value = value;

				if (Prev is IDisposable Disposable)
					Disposable.Dispose();
			}
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

		public void UpdateVisualization()
		{
			this.UpdateVisualization(null);
		}

		private async void UpdateVisualization(object _)
		{
			try
			{
				double? ActualWidth = null;
				double? ActualHeight = null;
				object Visualization = null;
				IXmlVisualizer Visualizer = null;
				Exception XmlParsingException = null;
				XmlDocument Doc = null;
				Variables Variables = null;

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
							Visualization = await Visualizer.TransformXml(Doc, Variables);
						}
					}
					catch (Exception ex)
					{
						XmlParsingException = ex;
					}
				}

				await MainWindow.UpdateGui(async () =>
				{
					try
					{
						if (Visualization is null)
						{
							if (Doc is null)
							{
								this.Visualization = new TextBlock()
								{
									Text = "No visualizer available for this type of XML document.",
									HorizontalAlignment = HorizontalAlignment.Center,
									VerticalAlignment = VerticalAlignment.Center
								};
							}
							else if (XmlParsingException is not null)
							{
								StringBuilder sb = new();

								if (XmlParsingException is XmlException XmlException)
								{
									sb.Append("Line: ");
									sb.AppendLine(XmlException.LineNumber.ToString());
									sb.Append("Position: ");
									sb.AppendLine(XmlException.LinePosition.ToString());
									sb.AppendLine();
								}

								sb.Append(XmlParsingException.Message);

								StackPanel View = new();
								View.Children.Add(new TextBlock()
								{
									Text = sb.ToString(),
									Foreground = System.Windows.Media.Brushes.Red,
									HorizontalAlignment = HorizontalAlignment.Center,
									VerticalAlignment = VerticalAlignment.Center
								});

								this.Visualization = View;
							}
							else
							{
								this.Visualization = null;
							}
						}
						if (Visualization is PixelInformation Pixels)
						{
							this.Show(Pixels);
							ActualWidth = Pixels.Width;
							ActualHeight = Pixels.Height;
						}
						else if (Visualization is Graph Graph)
						{
							Pixels = Graph.CreatePixels(Variables ?? []);
							this.Show(Pixels);
							ActualWidth = Pixels.Width;
							ActualHeight = Pixels.Height;
						}
						else if (Visualization is SKImage Image)
						{
							Pixels = PixelInformation.FromImage(Image);
							Image.Dispose();
							this.Show(Pixels);
							ActualWidth = Pixels.Width;
							ActualHeight = Pixels.Height;
						}
						else if (Visualization is MarkdownDocument Markdown)
						{
							string Xaml = await Markdown.GenerateXAML();
							this.Visualization = XamlReader.Parse(Xaml);
						}
						else if (Visualization is string s)
						{
							this.Visualization = new TextBlock()
							{
								Text = s,
								HorizontalAlignment = HorizontalAlignment.Center,
								VerticalAlignment = VerticalAlignment.Center
							};
						}
						else
							this.Visualization = Visualization;
					}
					catch (Exception ex)
					{
						StackPanel View = new();
						View.Children.Add(new TextBlock()
						{
							Text = ex.Message,
							Foreground = System.Windows.Media.Brushes.Red,
							HorizontalAlignment = HorizontalAlignment.Center,
							VerticalAlignment = VerticalAlignment.Center
						});

						this.Visualization = View;
					}

					this.CalcZoom(this.SelectedZoom, ActualWidth, ActualHeight);
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void Show(PixelInformation Pixels)
		{
			BitmapImage BitmapImage;
			byte[] Bin = Pixels.EncodeAsPng();

			using MemoryStream ms = new(Bin);

			BitmapImage = new BitmapImage();
			BitmapImage.BeginInit();
			BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			BitmapImage.StreamSource = ms;
			BitmapImage.EndInit();

			this.Visualization = new Image()
			{
				Source = BitmapImage,
				Width = Pixels.Width,
				Height = Pixels.Height
			};
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
		public double EditorWidth
		{
			get => this.editorWidth.Value;
			set
			{
				this.editorWidth.Value = Math.Max(value, 50);
				this.CalcZoom(this.SelectedZoom, null, null);
			}
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
				switch (await MainWindow.MessageBox("Do you want to save your unsaved changes?",
					"Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
				{
					case MessageBoxResult.Yes:
						bool? Result = await this.SaveXml();
						if (!Result.HasValue)
							return false;
						else
							return true;

					case MessageBoxResult.No:
						return true;

					case MessageBoxResult.Cancel:
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
			await this.SaveXml();
		}

		private async Task<bool?> SaveXml()
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
