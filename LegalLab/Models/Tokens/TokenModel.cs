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
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly NeuroFeaturesClient client;
		private readonly NoteCommand[] noteCommands;
		private readonly Token token;
		private readonly string language;
		private TokenDetail[] details;
		private BitmapImage glyph;

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
		private TokenModel(NeuroFeaturesClient Client, Token Token)
		{
			this.client = Client;
			this.token = Token;
			this.noteCommands = this.token.GetNoteCommands();
			this.language = "en";

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
			TokenModel Result = new(Client, Token);

			List<TokenDetail> Details = new()
			{
				new TokenDetail("Token ID", Result.token.TokenId),
				new TokenDetail("Token ID Method", Result.token.TokenIdMethod),
				new TokenDetail("Short ID", Result.token.ShortId),
				new TokenDetail("Ordinal", Result.token.Ordinal),
				new TokenDetail("Batch Size", Result.token.BatchSize),
				new TokenDetail("Created", Result.token.Created),
				new TokenDetail("Updated", Result.token.Updated),
				new TokenDetail("Value", Result.token.Value),
				new TokenDetail("Currency", Result.token.Currency),
				new TokenDetail("Expires", Result.token.Expires),
				new TokenDetail("Archivig time (Required)", Result.token.ArchiveRequired),
				new TokenDetail("Archivig time (Optional)", Result.token.ArchiveOptional),
				new TokenDetail("Signature Timestamp", Result.token.SignatureTimestamp),
				new TokenDetail("Signature", Convert.ToBase64String(Result.token.Signature)),
				new TokenDetail("Definition Schema Digest", Convert.ToBase64String(Result.token.DefinitionSchemaDigest)),
				new TokenDetail("Definition Schema Hash Function", Result.token.DefinitionSchemaHashFunction),
				new TokenDetail("Creator Can Destroy", Result.token.CreatorCanDestroy),
				new TokenDetail("Owner Can Destroy Batch", Result.token.OwnerCanDestroyBatch),
				new TokenDetail("Owner Can Destroy Individual", Result.token.OwnerCanDestroyIndividual),
				new TokenDetail("Certifier Can Destroy", Result.token.CertifierCanDestroy),
				new TokenDetail("Friendly Name", Result.token.FriendlyName),
				new TokenDetail("Category", Result.token.Category),
				new TokenDetail("Description", await MarkdownToXaml(Result.token.Description)),
				new TokenDetail("Glyph", Convert.ToBase64String(Result.token.Glyph)),
				new TokenDetail("Glyph Content Type", Result.token.GlyphContentType),
				new TokenDetail("Glyph Width", Result.token.GlyphWidth),
				new TokenDetail("Glyph Height", Result.token.GlyphHeight),
				new TokenDetail("Visibility", Result.token.Visibility),
				new TokenDetail("Creator", Result.token.Creator),
				new TokenDetail("CreatorJid", Result.token.CreatorJid),
				new TokenDetail("Owner", Result.token.Owner),
				new TokenDetail("OwnerJid", Result.token.OwnerJid),
				new TokenDetail("TrustProvider", Result.token.TrustProvider),
				new TokenDetail("TrustProviderJid", Result.token.TrustProviderJid),
				new TokenDetail("Reference", Result.token.Reference),
				new TokenDetail("Definition", Result.token.Definition),
				new TokenDetail("DefinitionNamespace", Result.token.DefinitionNamespace),
				new TokenDetail("CreationContract", Result.token.CreationContract),
				new TokenDetail("OwnershipContract", Result.token.OwnershipContract)
			};

			foreach (string s in Result.token.Witness)
				Details.Add(new TokenDetail("Witness", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.Certifier)
				Details.Add(new TokenDetail("Certifier", s));

			foreach (string s in Result.token.Valuator)
				Details.Add(new TokenDetail("Valuator", s));

			foreach (string s in Result.token.Assessor)
				Details.Add(new TokenDetail("Assessor", s));

			foreach (TokenTag Tag in Result.token.Tags)
				Details.Add(new TokenDetail(Tag.Name, Tag.Value));

			if (Result.token.HasStateMachine)
			{
				Details.Add(new TokenDetail("State-Machine Present State", new Button()
				{
					Command = Result.viewPresentReport,
					Content = "View Report",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}));

				Details.Add(new TokenDetail("State-Machine History", new Button()
				{
					Command = Result.viewHistoryReport,
					Content = "View Report",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}));

				Details.Add(new TokenDetail("State-Machine State Diagram", new Button()
				{
					Command = Result.viewStateDiagramReport,
					Content = "View Report",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}));

				Details.Add(new TokenDetail("State-Machine Profiling", new Button()
				{
					Command = Result.viewProfilingReport,
					Content = "View Report",
					Margin = new Thickness(10, 2, 10, 2),
					MinWidth = 150
				}));

				int i = 0;

				foreach (NoteCommand NoteCommand in Result.noteCommands)
				{
					Details.Add(new TokenDetail(NoteCommand.ToolTip?.Find(Language), new Button()
					{
						Command = Result.executeNoteCommand,
						CommandParameter = i++,
						Content = NoteCommand.Title?.Find(Language),
						Margin = new Thickness(10, 2, 10, 2),
						MinWidth = 150
					}));
				}
			}

			Result.details = Details.ToArray();

			return Result;
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
		public TokenDetail[] Details => this.details;

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
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

	}
}
