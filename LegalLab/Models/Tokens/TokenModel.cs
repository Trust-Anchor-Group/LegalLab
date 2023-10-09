using LegalLab.Dialogs.Parameters;
using LegalLab.Extensions;
using LegalLab.Models.Items;
using LegalLab.Models.Legal;
using LegalLab.Models.Tokens.Reports;
using LegalLab.Tabs;
using NeuroFeatures;
using NeuroFeatures.NoteCommands;
using NeuroFeatures.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;
using Waher.Script.Model;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly NeuroFeaturesClient client;
		private readonly Token token;
		private readonly string language;
		private NoteCommand[] noteCommands;
		private TokenDetail[] details;
		private BitmapImage glyph;
		private string currentState = null;
		private Variables currentVariables = null;

		private readonly Command viewPresentReport;
		private readonly Command viewHistoryReport;
		private readonly Command viewStateDiagramReport;
		private readonly Command viewProfilingReport;
		private readonly ParametrizedCommand executeNoteCommand;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Client">Client</param>
		/// <param name="Token">Neuro-Feature token</param>
		/// <param name="Language">Language</param>
		private TokenModel(NeuroFeaturesClient Client, Token Token, string Language)
		{
			this.client = Client;
			this.token = Token;
			this.language = Language;

			this.viewPresentReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewPresentReport);
			this.viewHistoryReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewHistoryReport);
			this.viewStateDiagramReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewStateDiagramReport);
			this.viewProfilingReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewProfilingReport);
			this.executeNoteCommand = new ParametrizedCommand(this.CanExecuteNoteCommand, this.ExecuteNoteCommand);
		}

		/// <summary>
		/// Referenced token.
		/// </summary>
		public Token Token => this.token;

		/// <summary>
		/// Available note commands.
		/// </summary>
		public NoteCommand[] NoteCommands => this.noteCommands;

		/// <summary>
		/// Command for showing the present report.
		/// </summary>
		public Command ViewPresentReport => this.viewPresentReport;

		/// <summary>
		/// Command for showing the history report.
		/// </summary>
		public Command ViewHistoryReport => this.viewHistoryReport;

		/// <summary>
		/// Command for showing the state diagram.
		/// </summary>
		public Command ViewStateDiagramReport => this.viewStateDiagramReport;

		/// <summary>
		/// Command for showing the profiling report.
		/// </summary>
		public Command ViewProfilingReport => this.viewProfilingReport;

		/// <summary>
		/// Command for executing a note command.
		/// </summary>
		public ParametrizedCommand NoteCommand => this.executeNoteCommand;

		public static async Task<TokenModel> CreateAsync(NeuroFeaturesClient Client, Token Token, string Language)
		{
			TokenModel Result = new(Client, Token, Language);

			Result.noteCommands = await Result.token.GetNoteCommands();

			List<TokenDetail> Details = new()
			{
				new TokenDetail("Token ID", Result.token.TokenId, false),
				new TokenDetail("Token ID Method", Result.token.TokenIdMethod, false),
				new TokenDetail("Short ID", Result.token.ShortId, false),
				new TokenDetail("Ordinal", Result.token.Ordinal, false),
				new TokenDetail("Batch Size", Result.token.BatchSize, false),
				new TokenDetail("Created", Result.token.Created, false),
				new TokenDetail("Updated", Result.token.Updated, false),
				new TokenDetail("Value", Result.token.Value, false),
				new TokenDetail("Currency", Result.token.Currency, false),
				new TokenDetail("Expires", Result.token.Expires, false),
				new TokenDetail("Archivig time (Required, false)", Result.token.ArchiveRequired, false),
				new TokenDetail("Archivig time (Optional, false)", Result.token.ArchiveOptional, false),
				new TokenDetail("Signature Timestamp", Result.token.SignatureTimestamp, false),
				new TokenDetail("Signature", Convert.ToBase64String(Result.token.Signature), false),
				new TokenDetail("Definition Schema Digest", Convert.ToBase64String(Result.token.DefinitionSchemaDigest), false),
				new TokenDetail("Definition Schema Hash Function", Result.token.DefinitionSchemaHashFunction, false),
				new TokenDetail("Creator Can Destroy", Result.token.CreatorCanDestroy, false),
				new TokenDetail("Owner Can Destroy Batch", Result.token.OwnerCanDestroyBatch, false),
				new TokenDetail("Owner Can Destroy Individual", Result.token.OwnerCanDestroyIndividual, false),
				new TokenDetail("Certifier Can Destroy", Result.token.CertifierCanDestroy, false),
				new TokenDetail("Friendly Name", Result.token.FriendlyName, false),
				new TokenDetail("Category", Result.token.Category, false),
				new TokenDetail("Description", await MarkdownToXaml(Result.token.Description), false),
				new TokenDetail("Glyph", Convert.ToBase64String(Result.token.Glyph), false),
				new TokenDetail("Glyph Content Type", Result.token.GlyphContentType, false),
				new TokenDetail("Glyph Width", Result.token.GlyphWidth, false),
				new TokenDetail("Glyph Height", Result.token.GlyphHeight, false),
				new TokenDetail("Visibility", Result.token.Visibility, false),
				new TokenDetail("Creator", Result.token.Creator, false),
				new TokenDetail("CreatorJid", Result.token.CreatorJid, false),
				new TokenDetail("Owner", Result.token.Owner, false),
				new TokenDetail("OwnerJid", Result.token.OwnerJid, false),
				new TokenDetail("TrustProvider", Result.token.TrustProvider, false),
				new TokenDetail("TrustProviderJid", Result.token.TrustProviderJid, false),
				new TokenDetail("Reference", Result.token.Reference, false),
				new TokenDetail("Definition", Result.token.Definition, false),
				new TokenDetail("DefinitionNamespace", Result.token.DefinitionNamespace, false),
				new TokenDetail("CreationContract", Result.token.CreationContract, false),
				new TokenDetail("OwnershipContract", Result.token.OwnershipContract, false)
			};

			foreach (string s in Result.token.Witness)
				Details.Add(new TokenDetail("Witness", s, false));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s, false));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s, false));

			foreach (string s in Result.token.Certifier)
				Details.Add(new TokenDetail("Certifier", s, false));

			foreach (string s in Result.token.Valuator)
				Details.Add(new TokenDetail("Valuator", s, false));

			foreach (string s in Result.token.Assessor)
				Details.Add(new TokenDetail("Assessor", s, false));

			foreach (TokenTag Tag in Result.token.Tags)
				Details.Add(new TokenDetail(Tag.Name, Tag.Value, false));

			if (Result.token.HasStateMachine)
			{
				Details.Add(new TokenDetail("State-Machine Present State", new Button()
				{
					Command = Result.viewPresentReport,
					Content = "Present State...",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}, false));

				Details.Add(new TokenDetail("State-Machine History", new Button()
				{
					Command = Result.viewHistoryReport,
					Content = "History...",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}, false));

				Details.Add(new TokenDetail("State-Machine State Diagram", new Button()
				{
					Command = Result.viewStateDiagramReport,
					Content = "State Diagram...",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}, false));

				Details.Add(new TokenDetail("State-Machine Profiling", new Button()
				{
					Command = Result.viewProfilingReport,
					Content = "Profiling...",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}, false));

				await Result.AddNoteCommands(Details);
			}

			Result.details = Details.ToArray();

			return Result;
		}

		/// <summary>
		/// Updates visible note commands, based on the current context.
		/// </summary>
		public async Task UpdateNoteCommands()
		{
			List<TokenDetail> Details = new();

			foreach (TokenDetail Detail in this.details)
			{
				if (!Detail.NoteCommand)
					Details.Add(Detail);
			}

			await this.AddNoteCommands(Details);

			MainWindow.UpdateGui(() =>
			{
				this.Details = Details.ToArray();
				return Task.CompletedTask;
			});
		}

		public async Task<KeyValuePair<NoteCommand, int>[]> GetContextSpecificNoteCommands(bool IsOwner)
		{
			Variables v = new()
			{
				["State"] = this.currentState,
				["<State>"] = this.currentState
			};
			int i, c = this.noteCommands?.Length ?? 0;
			List<KeyValuePair<NoteCommand, int>> Result = new();

			this.currentVariables?.CopyTo(v);

			for (i = 0; i < c; i++)
			{
				NoteCommand NoteCommand = this.noteCommands[i];

				if (IsOwner)
				{
					if (!NoteCommand.OwnerNote)
						continue;
				}
				else
				{
					if (!NoteCommand.ExternalNote)
						continue;
				}

				if (NoteCommand.HasNoteContextScript)
				{
					try
					{
						object Obj = await NoteCommand.ParsedNoteContextScript.EvaluateAsync(v);
						if (Obj is not bool b || !b)
							continue;
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						continue;
					}
				}

				Result.Add(new KeyValuePair<NoteCommand, int>(NoteCommand, i));
			}

			return Result.ToArray();
		}

		private async Task AddNoteCommands(List<TokenDetail> Details)
		{
			KeyValuePair<NoteCommand, int>[] Commands = await this.GetContextSpecificNoteCommands(true);

			foreach (KeyValuePair<NoteCommand, int> P in Commands)
			{
				NoteCommand NoteCommand = P.Key;
				int i = P.Value;

				Details.Add(new TokenDetail(NoteCommand.ToolTip?.Find(this.language), new Button()
				{
					Command = this.executeNoteCommand,
					CommandParameter = i,
					Content = NoteCommand.Title?.Find(this.language),
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}, true));
			}
		}

		private static async Task<object> MarkdownToXaml(string Markdown)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			string Xaml = await Doc.GenerateXAML();
			return Xaml.ParseSimple();
		}

		/// <summary>
		/// Token glyph.
		/// </summary>
		public BitmapImage Glyph
		{
			get
			{
				if (this.glyph is null && this.token.Glyph is not null)
				{
					BitmapImage Result = new();
					Result.BeginInit();
					Result.CreateOptions = BitmapCreateOptions.None;
					Result.CacheOption = BitmapCacheOption.Default;
					Result.StreamSource = new MemoryStream(this.token.Glyph);
					Result.EndInit();

					this.glyph = Result;
				}

				return this.glyph;
			}
		}

		/// <summary>
		/// Friendly Name of token
		/// </summary>
		public string FriendlyName => this.token.FriendlyName;

		/// <summary>
		/// Category of token
		/// </summary>
		public string Category => this.token.Category;

		/// <summary>
		/// Description of token
		/// </summary>
		public string Description => this.token.Description;

		/// <summary>
		/// ID of token
		/// </summary>
		public string TokenId => this.token.TokenId;

		/// <summary>
		/// When token expires
		/// </summary>
		public DateTime Expires => this.token.Expires;

		/// <summary>
		/// Current value of token
		/// </summary>
		public decimal Value => this.token.Value;

		/// <summary>
		/// Currency
		/// </summary>
		public string Currency => this.token.Currency;

		/// <summary>
		/// When Token was last updated (or created)
		/// </summary>
		public DateTime Updated => this.token.Updated;

		/// <summary>
		/// Creator of token
		/// </summary>
		public string Creator => this.token.Creator;

		/// <summary>
		/// Visibility of token
		/// </summary>
		public ContractVisibility Visibility => this.token.Visibility;

		/// <summary>
		/// Token details.
		/// </summary>
		public TokenDetail[] Details
		{
			get => this.details;
			set
			{
				this.details = value;
				this.RaisePropertyChanged(nameof(this.Details));
			}
		}

		private bool CanExecuteViewStateMachineReport()
		{
			return this.token.HasStateMachine;
		}

		private async Task ExecuteViewPresentReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
				await AddReport(new TokenPresentReport(this.client, this.TokenId));
		}

		private async Task ExecuteViewHistoryReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
				await AddReport(new TokenHistoryReport(this.client, this.TokenId));
		}

		private async Task ExecuteViewStateDiagramReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
				await AddReport(new TokenStateDiagramReport(this.client, this.TokenId));
		}

		private async Task ExecuteViewProfilingReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
				await AddReport(new TokenProfilingReport(this.client, this.TokenId));
		}

		private static async Task AddReport(TokenReport Report)
		{
			MainWindow.MouseHourglass();
			try
			{
				ReportTab ReportTab = new(Report);
				string Title = await Report.GetTitle();

				await Report.GenerateReport(ReportTab);

				TabItem Tab = MainWindow.NewTab(Title);
				Tab.Content = ReportTab;

				MainWindow.currentInstance.TabControl.Items.Add(Tab);
				MainWindow.currentInstance.TabControl.SelectedItem = Tab;
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
			finally
			{
				MainWindow.MouseDefault();
			}
		}

		private bool CanExecuteNoteCommand(object Parameter)
		{
			return
				this.token.HasStateMachine &&
				this.noteCommands is not null &&
				(Parameter is null ||
				(Parameter is int i &&
				i >= 0 &&
				i < this.noteCommands.Length));
		}

		private async void ExecuteNoteCommand(object Parameter)
		{
			try
			{
				if (this.token.HasStateMachine &&
					this.noteCommands is not null &&
					Parameter is int i &&
					i >= 0 &&
					i < this.noteCommands.Length)
				{
					NoteCommand Command = this.noteCommands[i];
					Variables NoteParameters;
					string Msg;

					try
					{
						if (Command.HasParameters)
						{
							ContractParametersModel Model = new(Command.Parameters, null, this.language);
							ParametersDialog Dialog = new(Command.Title.Find(this.language), Model)
							{
								Owner = MainWindow.currentInstance
							};

							await Model.Start();
							Control First = await Model.PopulateParameters(null, Dialog.ParametersPanel, null, null);

							if (First is not null)
							{
								First.Focus();

								bool? Result = Dialog.ShowDialog();
								if (!Result.HasValue || !Result.Value)
									return;

								NoteParameters = await Model.ValidateParameters()
									?? throw new Exception("Invalid parameters.");
							}
							else
								NoteParameters = new Variables();
						}
						else
							NoteParameters = new Variables();

						Msg = Command.Confirmation?.Find(this.language);
						if (!string.IsNullOrEmpty(Msg))
						{
							MessageBoxResult Confirmation = await MainWindow.MessageBox(Msg, "Confirmation",
								MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

							if (Confirmation != MessageBoxResult.Yes)
								return;
						}

						Waher.Script.Expression Exp = Command.ParsedNoteGenerationScript
							?? (string.IsNullOrEmpty(Command.NoteGenerationScript) ?
							throw new Exception("Missing note script.")
							: new Waher.Script.Expression(Command.NoteGenerationScript));

						if (!Exp.CheckExpressionSafe(out ScriptNode Prohibited))
							throw new Exception("Command blocked. Unsafe portion of script: " + Prohibited.SubExpression);

						Task _ = Task.Run(async () =>
						{
							try
							{
								object Obj = await Exp.EvaluateAsync(NoteParameters);

								if (Obj is string s)
									await this.client.AddTextNoteAsync(this.TokenId, s, Command.Personal);
								else if (Obj is XmlDocument Xml)
									await this.client.AddXmlNoteAsync(this.TokenId, Xml.DocumentElement.OuterXml, Command.Personal);
								else if (Obj is XmlElement E)
									await this.client.AddXmlNoteAsync(this.TokenId, E.OuterXml, Command.Personal);
								else if (Obj is null)
									throw new Exception("Note command returned null.");
								else
									throw new Exception("Command generated note of uncompatible type: " + Obj.GetType().FullName);

								this.NoteAdded?.Invoke(this, EventArgs.Empty);

								Msg = Command.Success?.Find(this.language);
								if (!string.IsNullOrEmpty(Msg))
									await MainWindow.MessageBox(Msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
							}
							catch (Exception ex)
							{
								Msg = Command.Failure?.Find(this.language);
								MainWindow.ErrorBox(string.IsNullOrEmpty(Msg) ? ex.Message : Msg);
							}
						});
					}
					catch (Exception ex)
					{
						Msg = Command.Failure?.Find(this.language);
						MainWindow.ErrorBox(string.IsNullOrEmpty(Msg) ? ex.Message : Msg);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a note has been added.
		/// </summary>
		public event EventHandler NoteAdded;

		/// <summary>
		/// Gets called when a new state has been reported.
		/// </summary>
		/// <param name="NewState">New state.</param>
		public void StateUpdated(string NewState)
		{
			this.currentState = NewState;
		}

		/// <summary>
		/// Gets called when the variables of a state-machine has been reported.
		/// </summary>
		/// <param name="NewVariables">New set of variables.</param>
		public void VariablesUpdated(Variables NewVariables)
		{
			this.currentVariables = NewVariables;
		}

		/// <summary>
		/// Current state of state-machine (if known).
		/// </summary>
		public string CurrentState => this.currentState;

		/// <summary>
		/// Current variables of state-machine (if known).
		/// </summary>
		public Variables CurrentVariables => this.currentVariables;
	}
}
