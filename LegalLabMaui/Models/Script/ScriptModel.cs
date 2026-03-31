using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace LegalLabMaui.Models.Script
{
	using Command = LegalLabMaui.Models.Command;

	/// <summary>
	/// Interaction logic for the script view.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public class ScriptModel : Model, IDisposable
	{
		private static readonly Variables variables = [];

		private readonly Property<string> referenceUri;
		private readonly Property<string> input;
		private readonly Property<string> output;
		private readonly Property<ObservableCollection<ScriptHistoryItem>> history;
		private readonly ICommand run;

		/// <summary>
		/// Interaction logic for the script view.
		/// </summary>
		public ScriptModel()
		{
			this.referenceUri = new Property<string>(nameof(this.ReferenceUri), "https://lab.tagroot.io/Script.md", this);
			this.input = new Property<string>(nameof(this.Input), string.Empty, this);
			this.output = new Property<string>(nameof(this.Output), string.Empty, this);
			this.history = new Property<ObservableCollection<ScriptHistoryItem>>(nameof(this.History), [], this);
			this.run = new Command(this.ExecuteRunAsync);

			variables.ConsoleOut = new PrintOutput(this);
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
		/// Script output / history as a single string (for display in a label/editor).
		/// </summary>
		public string Output
		{
			get => this.output.Value;
			set => this.output.Value = value;
		}

		/// <summary>
		/// Script execution history items (for structured display).
		/// </summary>
		public ObservableCollection<ScriptHistoryItem> History => this.history.Value;

		/// <summary>
		/// Current set of variables.
		/// </summary>
		public static Variables Variables => variables;

		/// <summary>
		/// Command used to execute the current script input.
		/// </summary>
		public ICommand Run => this.run;

		public static HtmlSettings GetHtmlSettings()
		{
			return new HtmlSettings()
			{
				XmlEntitiesOnly = true
			};
		}

		private Task ExecuteRunAsync()
		{
			this.Execute();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the current script input.
		/// </summary>
		public void Execute()
		{
			Waher.Script.Expression Exp;
			string ScriptText = this.Input;

			try
			{
				Exp = new Waher.Script.Expression(ScriptText);
				this.Input = string.Empty;
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				AppService.ErrorBox(ex.Message);
				return;
			}

			ScriptHistoryItem HistoryItem = new() { Script = ScriptText };
			this.History.Add(HistoryItem);

			Task.Run(async () =>
			{
				try
				{
					Waher.Script.Abstraction.Elements.IElement Ans;

					Task Preview(object sender2, PreviewEventArgs e2)
					{
						AppService.UpdateGui(async () =>
						{
							string ResultText = await this.FormatResult(e2.Preview);
							HistoryItem.Result = ResultText;
							this.AppendOutput(ScriptText, ResultText);
						});

						return Task.CompletedTask;
					}

					variables.OnPreview += Preview;
					try
					{
						Ans = await Exp.Root.EvaluateAsync(variables);
					}
					catch (ScriptReturnValueException ex)
					{
						Ans = ex.ReturnValue;
					}
					catch (Exception ex)
					{
						Ans = new ObjectValue(ex);
					}
					finally
					{
						variables.OnPreview -= Preview;
					}

					variables["Ans"] = Ans;

					await AppService.UpdateGui(async () =>
					{
						string ResultText = await this.FormatResult(Ans);
						HistoryItem.Result = ResultText;
						this.AppendOutput(ScriptText, ResultText);
					});
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);
					await AppService.MessageBox(ex.Message, "Unable to parse script.");
				}
			});
		}

		private async Task<string> FormatResult(Waher.Script.Abstraction.Elements.IElement Ans)
		{
			try
			{
				if (Ans.AssociatedObjectValue is Exception ex)
				{
					ex = Log.UnnestException(ex);

					if (ex is AggregateException ex2)
					{
						StringBuilder sb = new();
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							sb.AppendLine(ex3.Message);
						}
						return sb.ToString().TrimEnd();
					}
					else
						return ex.Message;
				}
				else if (Ans.AssociatedObjectValue is ObjectMatrix M && M.ColumnNames != null)
				{
					StringBuilder Markdown = new();

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
								if (Item is not string s2)
									s2 = Waher.Script.Expression.ToString(Item);

								s2 = s2.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "<br/>");
								Markdown.Append(MarkdownDocument.Encode(s2));
							}
						}

						Markdown.AppendLine(" |");
					}

					return Markdown.ToString().TrimEnd();
				}
				else if (Ans is Graph G)
				{
					// In MAUI, graphs are returned as formatted text description
					return "(Graph: " + G.ToString() + ")";
				}
				else
					return Ans.ToString();
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				return ex.Message;
			}
		}

		private void AppendOutput(string Script, string Result)
		{
			StringBuilder sb = new(this.Output);
			if (sb.Length > 0)
				sb.AppendLine();
			sb.Append("> ");
			sb.AppendLine(Script);
			sb.AppendLine(Result);
			this.Output = sb.ToString();
		}

		/// <summary>
		/// Clears the output and history.
		/// </summary>
		public void Clear()
		{
			this.Output = string.Empty;
			this.History.Clear();
		}

		/// <summary>
		/// Prints a value to the output (called by <see cref="PrintOutput"/>).
		/// </summary>
		internal void Print(string Value)
		{
			AppService.UpdateGui(() =>
			{
				StringBuilder sb = new(this.Output);
				sb.Append(Value);
				this.Output = sb.ToString();
				return Task.CompletedTask;
			});
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.History.Clear();
			this.Output = string.Empty;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override Task Start()
		{
			return base.Start();
		}
	}

	/// <summary>
	/// Represents a single script execution entry in the history.
	/// </summary>
	public class ScriptHistoryItem : System.ComponentModel.INotifyPropertyChanged
	{
		private string script = string.Empty;
		private string result = string.Empty;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// The script expression that was executed.
		/// </summary>
		public string Script
		{
			get => this.script;
			set
			{
				if (this.script != value)
				{
					this.script = value;
					this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(this.Script)));
				}
			}
		}

		/// <summary>
		/// The result of executing the script.
		/// </summary>
		public string Result
		{
			get => this.result;
			set
			{
				if (this.result != value)
				{
					this.result = value;
					this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(this.Result)));
				}
			}
		}

		/// <inheritdoc/>
		public override string ToString() => this.Script;
	}
}
