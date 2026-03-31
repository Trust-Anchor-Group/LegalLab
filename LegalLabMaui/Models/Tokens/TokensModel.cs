using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Tokens.Details;
using NeuroFeatures;
using NeuroFeatures.EventArguments;
using NeuroFeatures.NoteCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Inventory;

namespace LegalLabMaui.Models.Tokens
{
	/// <summary>
	/// Tokens Model
	/// </summary>
	[Singleton]
	public class TokensModel : PersistedModel, IDisposable
	{
		private readonly List<TokenModel> tokens = [];
		private readonly List<TokenTotal> totals = [];

		private readonly NeuroFeaturesClient neuroFeaturesClient;

		private TokenModel? selectedItem;
		private TokenEventDetail[]? events;
		private TokenMenuEntry[]? tokenMenuItems;

		private readonly Command addTextNote;
		private readonly Command addXmlNote;

		/// <summary>
		/// Tokens Model
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="ComponentJid">Component JID</param>
		public TokensModel(XmppClient Client, ContractsClient Contracts, string ComponentJid)
			: base()
		{
			this.neuroFeaturesClient = new NeuroFeaturesClient(Client, Contracts, ComponentJid);

			this.neuroFeaturesClient.TokenAdded += this.NeuroFeaturesClient_TokenAdded;
			this.neuroFeaturesClient.TokenRemoved += this.NeuroFeaturesClient_TokenRemoved;

			this.neuroFeaturesClient.StateUpdated += this.NeuroFeaturesClient_StateUpdated;
			this.neuroFeaturesClient.VariablesUpdated += this.NeuroFeaturesClient_VariablesUpdated;

			this.addTextNote = new Command(this.CanExecuteAddNote, this.ExecuteAddTextNote);
			this.addXmlNote = new Command(this.CanExecuteAddNote, this.ExecuteAddXmlNote);
		}

		private Task NeuroFeaturesClient_TokenAdded(object Sender, TokenEventArgs e)
		{
			AppService.UpdateGui(async () =>
			{
				TokenModel Token = await TokenModel.CreateAsync(this.neuroFeaturesClient, e.Token, "en", AppService.DesignModel);
				Token.Selected += this.Token_Selected;
				Token.Deselected += this.Token_Deselected;
				Token.NoteAdded += this.Token_NoteAdded;

				lock (this.tokens)
				{
					this.tokens.Insert(0, Token);
				}

				this.RaisePropertyChanged(nameof(this.Tokens));

				Task _ = this.LoadTotals();
			});

			return Task.CompletedTask;
		}

		private void Token_NoteAdded(object? sender, EventArgs e)
		{
			this.LoadEvents();
		}

		private void Token_Deselected(object? sender, EventArgs e)
		{
			if (this.selectedItem == sender)
			{
				this.selectedItem = null;
				this.RaisePropertyChanged(nameof(this.SelectedItem));

				this.events = null;
				this.RaisePropertyChanged(nameof(this.Events));

				this.tokenMenuItems = null;
				this.RaisePropertyChanged(nameof(this.TokenMenuItems));
			}
		}

		private async void Token_Selected(object? sender, EventArgs e)
		{
			try
			{
				this.selectedItem = sender as TokenModel;
				this.RaisePropertyChanged(nameof(this.SelectedItem));

				bool PrepareMenuItems = true;

				if (this.selectedItem is not null)
				{
					string TokenId = this.selectedItem.TokenId;

					if (this.selectedItem.CurrentState is null || this.selectedItem.CurrentVariables is null)
					{
						await this.neuroFeaturesClient.GetCurrentState(TokenId, async (sender2, e2) =>
						{
							if (e2.Ok && this.selectedItem is not null && this.selectedItem.TokenId == TokenId)
							{
								PrepareMenuItems = false;

								this.selectedItem.StateUpdated(e2.CurrentState);
								this.selectedItem.VariablesUpdated(e2.Variables);
								await this.selectedItem.UpdateNoteCommands();

								await AppService.UpdateGui(async () =>
								{
									await this.PrepareTokenMenuItems();
								});
							}
						}, null);
					}
				}

				this.LoadEvents();

				if (PrepareMenuItems)
					await this.PrepareTokenMenuItems();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void LoadEvents()
		{
			this.events = [];
			this.RaisePropertyChanged(nameof(this.Events));

			if (this.selectedItem is null)
				return;

			this.neuroFeaturesClient.GetEvents(this.selectedItem.TokenId, 0, 100, (sender, e) =>
			{
				if (e.Ok && e.Events.Length > 0 && e.Events[0].TokenId == (string)e.State)
				{
					int i, c = e.Events.Length;
					TokenEventDetail[] Events = new TokenEventDetail[c];

					for (i = 0; i < c; i++)
						Events[i] = TokenEventDetail.Create(e.Events[i]);

					this.events = Events;

					AppService.UpdateGui(() =>
					{
						this.RaisePropertyChanged(nameof(this.Events));
						return Task.CompletedTask;
					});
				}

				return Task.CompletedTask;
			}, this.selectedItem.TokenId);
		}

		private async Task PrepareTokenMenuItems()
		{
			List<TokenMenuEntry> Items = [];
			TokenModel? Model = this.selectedItem;

			if (Model is not null)
			{
				Items.Add(new TokenMenuEntry("Add Text Note...", this.addTextNote, null));
				Items.Add(new TokenMenuEntry("Add XML Note...", this.addXmlNote, null));

				if (Model.Token.HasStateMachine)
				{
					Items.Add(TokenMenuEntry.Separator);

					Items.Add(new TokenMenuEntry("Present State...", Model.ViewPresentReport, null));
					Items.Add(new TokenMenuEntry("History...", Model.ViewHistoryReport, null));
					Items.Add(new TokenMenuEntry("State Diagram...", Model.ViewStateDiagramReport, null));
					Items.Add(new TokenMenuEntry("Profiling...", Model.ViewProfilingReport, null));
				}

				if (Model.Token.HasEmbeddedLayout)
				{
					Items.Add(TokenMenuEntry.Separator);
					Items.Add(new TokenMenuEntry("Layout...", Model.ViewEmbeddedLayout, null));
				}

				KeyValuePair<NoteCommand, int>[] Commands = await Model.GetContextSpecificNoteCommands(true);

				if (Commands.Length > 0)
				{
					Items.Add(TokenMenuEntry.Separator);

					foreach (KeyValuePair<NoteCommand, int> P in Commands)
					{
						NoteCommand Command = P.Key;
						int i = P.Value;

						Items.Add(new TokenMenuEntry(
							Command.Title?.Find("en"),
							Model.NoteCommand,
							i,
							Command.ToolTip?.Find("en")));
					}
				}
			}

			this.tokenMenuItems = [.. Items];
			this.RaisePropertyChanged(nameof(this.TokenMenuItems));
		}

		private Task NeuroFeaturesClient_TokenRemoved(object Sender, TokenEventArgs e)
		{
			AppService.UpdateGui(() =>
			{
				lock (this.tokens)
				{
					int i, c = this.tokens.Count;

					for (i = 0; i < c; i++)
					{
						if (this.tokens[i].TokenId == e.Token.TokenId)
						{
							this.tokens.RemoveAt(i);
							break;
						}
					}
				}

				this.RaisePropertyChanged(nameof(this.Tokens));

				Task _ = this.LoadTotals();

				return Task.CompletedTask;
			});

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.neuroFeaturesClient.Dispose();
		}

		/// <summary>
		/// Neuro-Features client
		/// </summary>
		public NeuroFeaturesClient NeuroFeaturesClient => this.neuroFeaturesClient;

		/// <summary>
		/// Account events
		/// </summary>
		public IEnumerable<TokenModel> Tokens
		{
			get
			{
				lock (this.tokens)
				{
					return [.. this.tokens];
				}
			}
		}

		/// <summary>
		/// Selected item
		/// </summary>
		public TokenModel? SelectedItem
		{
			get => this.selectedItem;
			set => this.selectedItem = value;
		}

		/// <summary>
		/// Events of selected item.
		/// </summary>
		public TokenEventDetail[]? Events => this.events;

		/// <summary>
		/// Context-menu items for selected token.
		/// </summary>
		public TokenMenuEntry[]? TokenMenuItems => this.tokenMenuItems;

		public IEnumerable<TokenTotal> Totals
		{
			get
			{
				lock (this.totals)
				{
					return [.. this.totals];
				}
			}
		}

		private async Task LoadTotals()
		{
			try
			{
				TokenTotalsEventArgs e = await this.neuroFeaturesClient.GetTotalsAsync();
				if (e.Ok)
				{
					lock (this.totals)
					{
						this.totals.Clear();
						this.totals.AddRange(e.Totals);
					}

					this.RaisePropertyChanged(nameof(this.Totals));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Command for adding a text note to a token.
		/// </summary>
		public ICommand AddTextNote => this.addTextNote;

		/// <summary>
		/// Command for adding an XML note to a token.
		/// </summary>
		public ICommand AddXmlNote => this.addXmlNote;

		private bool CanExecuteAddNote()
		{
			return this.selectedItem is not null;
		}

		private async Task ExecuteAddTextNote()
		{
			string s = await AppService.PromptUser("Add Text Note", "Enter text note to add to token:");

			if (!string.IsNullOrEmpty(s))
			{
				if (this.selectedItem is null)
					AppService.ErrorBox("No token selected.");
				else
				{
					try
					{
						await this.neuroFeaturesClient.AddTextNoteAsync(this.selectedItem.TokenId, s);
						this.LoadEvents();
					}
					catch (Exception ex)
					{
						AppService.ErrorBox(ex.Message);
					}
				}
			}
		}

		private async Task ExecuteAddXmlNote()
		{
			await Shell.Current.GoToAsync("AddXmlNotePage", new Dictionary<string, object>
			{
				["TokenId"] = this.selectedItem?.TokenId ?? string.Empty,
				["TokensModel"] = this
			});
		}

		/// <summary>
		/// Called by the AddXmlNote page after the user submits valid XML.
		/// </summary>
		public async Task SubmitXmlNote(string XmlInput)
		{
			if (string.IsNullOrEmpty(XmlInput) || !XML.IsValidXml(XmlInput))
				return;

			if (this.selectedItem is null)
				AppService.ErrorBox("No token selected.");
			else
			{
				try
				{
					await this.neuroFeaturesClient.AddXmlNoteAsync(this.selectedItem.TokenId, XmlInput);
					this.LoadEvents();
				}
				catch (Exception ex)
				{
					AppService.ErrorBox(ex.Message);
				}
			}
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			await AppService.UpdateGui(() =>
			{
				// DataContext binding is handled by the MAUI page/view using DI or navigation.
				return Task.CompletedTask;
			});

			await this.LoadTotals();

			int Offset = 0;
			int c = 20;

			TokensEventArgs e2 = await this.neuroFeaturesClient.GetTokensAsync(Offset, c);
			while (e2.Ok)
			{
				await AppService.UpdateGui(async () =>
				{
					foreach (Token Token in e2.Tokens)
					{
						TokenModel TokenModel = await TokenModel.CreateAsync(this.neuroFeaturesClient, Token, "en", AppService.DesignModel);
						TokenModel.Selected += this.Token_Selected;
						TokenModel.Deselected += this.Token_Deselected;
						TokenModel.NoteAdded += this.Token_NoteAdded;

						lock (this.tokens)
						{
							this.tokens.Add(TokenModel);
						}
					}

					this.RaisePropertyChanged(nameof(this.Tokens));
				});

				Offset += e2.Tokens.Length;
				if (e2.Tokens.Length == 0)
					break;

				e2 = await this.neuroFeaturesClient.GetTokensAsync(Offset, c);
			}

			await base.Start();
		}

		private Task NeuroFeaturesClient_VariablesUpdated(object Sender, VariablesUpdatedEventArgs e)
		{
			AppService.UpdateGui(async () =>
			{
				try
				{
					foreach (TokenModel Token in this.tokens)
					{
						if (Token.TokenId == e.TokenId)
						{
							Token.VariablesUpdated(e.Variables);
							await Token.UpdateNoteCommands();

							if (this.selectedItem?.Token.TokenId == e.TokenId)
								await this.PrepareTokenMenuItems();
						}
					}
				}
				catch (Exception ex)
				{
					AppService.ErrorBox(ex.Message);
				}
			});

			return Task.CompletedTask;
		}

		private Task NeuroFeaturesClient_StateUpdated(object Sender, NewStateEventArgs e)
		{
			AppService.UpdateGui(async () =>
			{
				try
				{
					foreach (TokenModel Token in this.tokens)
					{
						if (Token.TokenId == e.TokenId)
						{
							Token.StateUpdated(e.NewState);
							await Token.UpdateNoteCommands();

							if (this.selectedItem?.Token.TokenId == e.TokenId)
								await this.PrepareTokenMenuItems();
						}
					}
				}
				catch (Exception ex)
				{
					AppService.ErrorBox(ex.Message);
				}
			});

			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// Represents a menu entry for token context actions (replaces WPF MenuItem/Separator).
	/// </summary>
	public sealed record TokenMenuEntry(string? Header, ICommand? Command, object? CommandParameter, string? ToolTip = null)
	{
		/// <summary>A sentinel value representing a menu separator.</summary>
		public static readonly TokenMenuEntry Separator = new(null, null, null);

		/// <summary>True when this entry is a separator.</summary>
		public bool IsSeparator => this.Header is null && this.Command is null;
	}
}
