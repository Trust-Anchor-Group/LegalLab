using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace LegalLab.Models.Script
{
	/// <summary>
	/// Interaction logic for the script view.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class ScriptModel : Model, IDisposable
	{
		private readonly Property<string> referenceUri;
		private readonly Property<string> input;

		private readonly Variables variables;
		private readonly StackPanel historyPanel;

		/// <summary>
		/// Interaction logic for the script view.
		/// From the IoTGateway project, with permission.
		/// </summary>
		/// <param name="HistoryPanel">History panel</param>
		public ScriptModel(StackPanel HistoryPanel)
		{
			this.referenceUri = new Property<string>(nameof(this.ReferenceUri), "https://lab.tagroot.io/Script.md", this);
			this.input = new Property<string>(nameof(this.Input), string.Empty, this);

			this.historyPanel = HistoryPanel;
			this.variables = new Variables()
			{
				ConsoleOut = new PrintOutput(this)
			};
		}

		/// <summary>
		/// URI to reference documentation.
		/// </summary>
		public string ReferenceUri
		{
			get => this.referenceUri.Value;
			set => this.referenceUri.Value = value;
		}

		/// <summary>
		/// Script input
		/// </summary>
		public string Input
		{
			get => this.input.Value;
			set => this.input.Value = value;
		}

		/// <summary>
		/// Event handler for the input PreviewKeyDown event.
		/// </summary>
		public void InputPreviewKeyDown(KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Enter)
				{
					if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
					{
						this.ExecuteButton_Click(this, e);
						e.Handled = true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private void ExecuteButton_Click(object sender, RoutedEventArgs e)
		{
			Waher.Script.Expression Exp;
			TextBlock ScriptBlock;
			UIElement ResultBlock = null;

			try
			{
				Exp = new Waher.Script.Expression(this.Input);

				ScriptBlock = new TextBlock()
				{
					Text = this.Input,
					FontFamily = new FontFamily("Courier New"),
					TextWrapping = TextWrapping.Wrap,
					Tag = Exp
				};

				ScriptBlock.PreviewMouseDown += TextBlock_PreviewMouseDown;

				this.historyPanel.Children.Add(ScriptBlock);

				if (this.historyPanel.Parent is ScrollViewer ScrollViewer)
					ScrollViewer.ScrollToBottom();

				this.Input = string.Empty;
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MainWindow.ErrorBox(ex.Message);
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					IElement Ans;

					Exp.OnPreview += (sender2, e2) =>
					{
						MainWindow.UpdateGui(async () =>
						{
							ResultBlock = await this.ShowResult(ResultBlock, e2.Preview, ScriptBlock);
						});
					};

					try
					{
						Ans = await Exp.Root.EvaluateAsync(this.variables);
					}
					catch (ScriptReturnValueException ex)
					{
						Ans = ex.ReturnValue;
					}
					catch (Exception ex)
					{
						Ans = new ObjectValue(ex);
					}

					this.variables["Ans"] = Ans;

					MainWindow.UpdateGui(async () =>
					{
						ResultBlock = await this.ShowResult(ResultBlock, Ans, ScriptBlock);
					});
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);
					MainWindow.MessageBox(ex.Message, "Unable to parse script.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			});
		}

		private async Task<UIElement> ShowResult(UIElement ResultBlock, IElement Ans, TextBlock ScriptBlock)
		{
			try
			{
				if (Ans is Graph G)
				{
					PixelInformation Pixels = G.CreatePixels(this.variables, out object[] States);
					return this.AddImageBlock(ScriptBlock, Pixels, G, States, ResultBlock);
				}
				else if (Ans.AssociatedObjectValue is SKImage Img)
					return this.AddImageBlock(ScriptBlock, PixelInformation.FromImage(Img), null, null, ResultBlock);
				else if (Ans.AssociatedObjectValue is Exception ex)
				{
					UIElement Last = ResultBlock ?? ScriptBlock;

					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
							Last = this.AddTextBlock(Last, ex3.Message, Colors.Red, FontWeights.Bold, null, ex3);
					}
					else
						Last = this.AddTextBlock(ScriptBlock, ex.Message, Colors.Red, FontWeights.Bold, ResultBlock, ex);

					return Last;
				}
				else if (Ans.AssociatedObjectValue is ObjectMatrix M && M.ColumnNames != null)
				{
					StringBuilder Markdown = new StringBuilder();

					foreach (string s2 in M.ColumnNames)
					{
						Markdown.Append("| ");
						Markdown.Append(MarkdownDocument.Encode(s2));
					}

					Markdown.AppendLine(" |");

					foreach (string s2 in M.ColumnNames)
						Markdown.Append("|---");

					Markdown.AppendLine("|");

					int x, y;

					for (y = 0; y < M.Rows; y++)
					{
						for (x = 0; x < M.Columns; x++)
						{
							Markdown.Append("| ");

							object Item = M.GetElement(x, y).AssociatedObjectValue;
							if (Item != null)
							{
								if (!(Item is string s2))
									s2 = Waher.Script.Expression.ToString(Item);

								s2 = s2.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");
								Markdown.Append(MarkdownDocument.Encode(s2));
							}
						}

						Markdown.AppendLine(" |");
					}

					MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString(), GetMarkdownSettings());
					string XAML = await Doc.GenerateXAML();

					if (XamlReader.Parse(XAML) is UIElement Parsed)
						return this.AddBlock(ScriptBlock, Parsed);

					return null;
				}
				else
					return this.AddTextBlock(ScriptBlock, Ans.ToString(), Colors.Red, FontWeights.Normal, ResultBlock, Ans);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				Ans = new ObjectValue(ex);
				this.variables["Ans"] = Ans;

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
						ScriptBlock = this.AddTextBlock(ScriptBlock, ex3.Message, Colors.Red, FontWeights.Bold, null, ex3);
				}
				else
					this.AddTextBlock(ScriptBlock, ex.Message, Colors.Red, FontWeights.Bold, ResultBlock, ex);

				return null;
			}
		}

		public static MarkdownSettings GetMarkdownSettings()
		{
			return new MarkdownSettings(null, false)
			{
				HtmlSettings = new HtmlSettings()
				{
					XmlEntitiesOnly = true
				},
				XamlSettings = new XamlSettings()
				{
					TableCellRowBackgroundColor1 = "#20404040",
					TableCellRowBackgroundColor2 = "#10808080"
				}
			};
		}

		private TextBlock AddTextBlock(UIElement ScriptBlock, string s, Color cl, FontWeight Weight, UIElement ResultBlock, object Tag)
		{
			if (ResultBlock is TextBlock TextBlock)
			{
				TextBlock.Text = s;
				TextBlock.Tag = Tag;
			}
			else
			{
				TextBlock = new TextBlock()
				{
					Text = s,
					FontFamily = new FontFamily("Courier New"),
					Foreground = new SolidColorBrush(cl),
					TextWrapping = TextWrapping.Wrap,
					FontWeight = Weight,
					Tag = Tag
				};

				TextBlock.PreviewMouseDown += TextBlock_PreviewMouseDown;

				this.AddBlock(ScriptBlock, TextBlock);
			}

			return TextBlock;
		}

		private UIElement AddBlock(UIElement ScriptBlock, UIElement ResultBlock)
		{
			if (ScriptBlock is null)
				this.historyPanel.Children.Add(ResultBlock);
			else
				this.historyPanel.Children.Insert(this.historyPanel.Children.IndexOf(ScriptBlock) + 1, ResultBlock);

			return ResultBlock;
		}

		private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.Input = ((TextBlock)sender).Text;
			e.Handled = true;
		}

		private UIElement AddImageBlock(UIElement ScriptBlock, PixelInformation Pixels, Graph Graph, object[] States, UIElement ResultBlock)
		{
			BitmapImage BitmapImage;
			byte[] Bin = Pixels.EncodeAsPng();

			using (MemoryStream ms = new MemoryStream(Bin))
			{
				BitmapImage = new BitmapImage();
				BitmapImage.BeginInit();
				BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				BitmapImage.StreamSource = ms;
				BitmapImage.EndInit();
			}

			if (ResultBlock is Image ImageBlock)
			{
				ImageBlock.Source = BitmapImage;
				ImageBlock.Width = Pixels.Width;
				ImageBlock.Height = Pixels.Height;
				ImageBlock.Tag = new Tuple<byte[], int, int, Graph, object[]>(Bin, Pixels.Width, Pixels.Height, Graph, States);
			}
			else
			{
				ImageBlock = new Image()
				{
					Source = BitmapImage,
					Width = Pixels.Width,
					Height = Pixels.Height,
					Tag = new Tuple<byte[], int, int, Graph, object[]>(Bin, Pixels.Width, Pixels.Height, Graph, States)
				};

				ImageBlock.PreviewMouseDown += ImageBlock_PreviewMouseDown;

				this.historyPanel.Children.Insert(this.historyPanel.Children.IndexOf(ScriptBlock) + 1, ImageBlock);
			}

			return ImageBlock;
		}

		private void ImageBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Image ImageBlock = (Image)sender;

			if (e.ChangedButton == MouseButton.Left)
			{
				Point P = e.GetPosition(ImageBlock);
				string Script;

				if (ImageBlock.Tag is Tuple<byte[], int, int, Graph, object[]> Image && Image.Item4 != null && Image.Item5 != null)
				{
					double X = ((double)P.X) * Image.Item2 / ImageBlock.ActualWidth;
					double Y = ((double)P.Y) * Image.Item3 / ImageBlock.ActualHeight;

					Script = Image.Item4.GetBitmapClickScript(X, Y, Image.Item5);
				}
				else
					Script = "[" + P.X.ToString() + "," + P.Y.ToString() + "]";

				this.Input = Script;
				this.ExecuteButton_Click(this, e);
			}
		}

		internal void Print(string Output)
		{
			MainWindow.UpdateGui(() =>
			{
				this.AddTextBlock(null, Output, Colors.Blue, FontWeights.Normal, null, false);
				return Task.CompletedTask;
			});
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.historyPanel.Children.Clear();
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.ScriptTab.DataContext = this;
				return Task.CompletedTask;
			});

			await base.Start();
		}
	}
}
