using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Items;
using LegalLabMaui.Models.Tokens.Reports;
using NeuroFeatures;
using NeuroFeatures.NoteCommands;
using NeuroFeatures.Tags;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;
using Waher.Script.Model;

namespace LegalLabMaui.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly DesignModel designModel;
		private readonly NeuroFeaturesClient client;
		private readonly Token token;
		private readonly string language;
		private NoteCommand[] noteCommands = [];
		private TokenDetail[] details = [];
		private string? currentState;
		private Variables? currentVariables;

		private readonly Command viewPresentReport;
		private readonly Command viewHistoryReport;
		private readonly Command viewStateDiagramReport;
		private readonly Command viewProfilingReport;
		private readonly Command viewEmbeddedLayout;
		private readonly ParametrizedCommand executeNoteCommand;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Client">Client</param>
		/// <param name="Token">Neuro-Feature token</param>
		/// <param name="Language">Language</param>
		/// <param name="DesignModel">Reference to the design model.</param>
		private TokenModel(NeuroFeaturesClient Client, Token Token, string Language, DesignModel DesignModel)
		{
			this.designModel = DesignModel;
			this.client = Client;
			this.token = Token;
			this.language = Language;

			this.viewPresentReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewPresentReport);
			this.viewHistoryReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewHistoryReport);
			this.viewStateDiagramReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewStateDiagramReport);
			this.viewProfilingReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewProfilingReport);
			this.viewEmbeddedLayout = new Command(this.CanExecuteViewEmbeddedLayout, this.ExecuteViewEmbeddedLayout);
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
		/// Command for showing the embedded layout.
		/// </summary>
		public Command ViewEmbeddedLayout => this.viewEmbeddedLayout;

		/// <summary>
		/// Command for executing a note command.
		/// </summary>
		public ParametrizedCommand NoteCommand => this.executeNoteCommand;

		public static async Task<TokenModel> CreateAsync(NeuroFeaturesClient Client, Token Token, string Language, DesignModel DesignModel)
		{
			TokenModel Result = new(Client, Token, Language, DesignModel);

			Result.noteCommands = await Result.token.GetNoteCommands();

			List<TokenDetail> Details =
			[
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
				new TokenDetail("Description", Result.token.Description, false),
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
				new TokenDetail("DefinitionLocalName", Result.token.DefinitionParsed?.DocumentElement?.LocalName, false),
				new TokenDetail("DefinitionNamespace", Result.token.DefinitionNamespace, false),
				new TokenDetail("CreationContract", Result.token.CreationContract, false),
				new TokenDetail("CreationContractTemplate", Result.token.CreationContractTemplate, false),
				new TokenDetail("OwnershipContract", Result.token.OwnershipContract, false)
			];

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
				Details.Add(new TokenDetail("State-Machine Present State", Result.viewPresentReport, false));
				Details.Add(new TokenDetail("State-Machine History", Result.viewHistoryReport, false));
				Details.Add(new TokenDetail("State-Machine State Diagram", Result.viewStateDiagramReport, false));
				Details.Add(new TokenDetail("State-Machine Profiling", Result.viewProfilingReport, false));

				await Result.AddNoteCommands(Details);
			}

			if (Result.token.HasEmbeddedLayout)
				Details.Add(new TokenDetail("Embedded Layout", Result.viewEmbeddedLayout, false));

			Result.details = [.. Details];

			return Result;
		}

		/// <summary>
		/// Updates visible note commands, based on the current context.
		/// </summary>
		public async Task UpdateNoteCommands()
		{
			List<TokenDetail> Details = [];

			foreach (TokenDetail Detail in this.details)
			{
				if (!Detail.NoteCommand)
					Details.Add(Detail);
			}

			await this.AddNoteCommands(Details);

			await AppService.UpdateGui(() =>
			{
				this.Details = [.. Details];
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
			List<KeyValuePair<NoteCommand, int>> Result = [];

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
						Log.Exception(ex);
						continue;
					}
				}

				Result.Add(new KeyValuePair<NoteCommand, int>(NoteCommand, i));
			}

			return [.. Result];
		}

		private async Task AddNoteCommands(List<TokenDetail> Details)
		{
			KeyValuePair<NoteCommand, int>[] Commands = await this.GetContextSpecificNoteCommands(true);

			foreach (KeyValuePair<NoteCommand, int> P in Commands)
			{
				NoteCommand NoteCommand = P.Key;
				int i = P.Value;

				// Value stores the command index so the UI can bind a button to executeNoteCommand
				// with the index as its CommandParameter.
				Details.Add(new TokenDetail(
					NoteCommand.ToolTip?.Find(this.language),
					new NoteCommandEntry(NoteCommand.Title?.Find(this.language), this.executeNoteCommand, i),
					true));
			}
		}

		/// <summary>
		/// Token glyph as raw bytes (MAUI consumers should decode as needed).
		/// </summary>
		public byte[] GlyphBytes => this.token.Glyph;

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
			AppService.MouseHourglass();
			try
			{
				string Title = await Report.GetTitle();
				string ReportMarkdown = await Report.GetReportMarkdown();

				await Shell.Current.GoToAsync("ReportPage", new Dictionary<string, object>
				{
					["Title"] = Title,
					["ReportMarkdown"] = ReportMarkdown,
					["Report"] = Report
				});
			}
			catch (Exception ex)
			{
				AppService.ErrorBox(ex.Message);
			}
			finally
			{
				AppService.MouseDefault();
			}
		}

		private bool CanExecuteViewEmbeddedLayout()
		{
			return this.token?.HasEmbeddedLayout ?? false;
		}

		private async Task ExecuteViewEmbeddedLayout()
		{
			if (this.token is not null)
				await AddReport(new TokenEmbeddedLayout(this.client, this.token));
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
							// Navigate to parameters page for note command
							await Shell.Current.GoToAsync("AddXmlNotePage", new Dictionary<string, object>
							{
								["NoteCommand"] = Command,
								["Language"] = this.language,
								["DesignModel"] = this.designModel
							});
							return;
						}
						else
							NoteParameters = [];

						Msg = Command.Confirmation?.Find(this.language);
						if (!string.IsNullOrEmpty(Msg))
						{
							bool Confirmed = await AppService.MessageBox(Msg, "Confirmation", true);
							if (!Confirmed)
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

								this.NoteAdded.Raise(this, EventArgs.Empty);

								Msg = Command.Success?.Find(this.language);
								if (!string.IsNullOrEmpty(Msg))
									AppService.SuccessBox(Msg);
							}
							catch (Exception ex)
							{
								Msg = Command.Failure?.Find(this.language);
								AppService.ErrorBox(string.IsNullOrEmpty(Msg) ? ex.Message : Msg);
							}
						});
					}
					catch (Exception ex)
					{
						Msg = Command.Failure?.Find(this.language);
						AppService.ErrorBox(string.IsNullOrEmpty(Msg) ? ex.Message : Msg);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when a note has been added.
		/// </summary>
		public event EventHandler? NoteAdded;

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
		public string? CurrentState => this.currentState;

		/// <summary>
		/// Current variables of state-machine (if known).
		/// </summary>
		public Variables? CurrentVariables => this.currentVariables;
	}

	/// <summary>
	/// Represents a note command entry stored in a <see cref="TokenDetail"/> value,
	/// carrying the display title, command reference, and the integer parameter index.
	/// </summary>
	public sealed record NoteCommandEntry(string? Title, ICommand Command, int CommandParameter);
}
