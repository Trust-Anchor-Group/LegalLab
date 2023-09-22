using LegalLab.Extensions;
using LegalLab.Models.Tokens.Events;
using LegalLab.Tabs;
using NeuroFeatures;
using NeuroFeatures.NoteCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Tokens Model
	/// </summary>
	[Singleton]
	public class TokensModel : PersistedModel, IDisposable
	{
		private readonly List<TokenModel> tokens = new();
		private readonly List<TokenTotal> totals = new();

		private readonly NeuroFeaturesClient neuroFeaturesClient;

		private TokenModel selectedItem = null;
		private TokenEventDetail[] events = null;
		private Control[] tokenMenuItems = null;

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
			MainWindow.UpdateGui(async () =>
			{
				TokenModel Token = await TokenModel.CreateAsync(this.neuroFeaturesClient, e.Token, "en");
				Token.Selected += this.Token_Selected;
				Token.Deselected += this.Token_Deselected;

				lock (this.tokens)
				{
					this.tokens.Insert(0, Token);
				}

				this.RaisePropertyChanged(nameof(this.Tokens));

				Task _ = this.LoadTotals();
			});

			return Task.CompletedTask;
		}

		private void Token_Deselected(object sender, EventArgs e)
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

		private void Token_Selected(object sender, EventArgs e)
		{
			this.selectedItem = sender as TokenModel;
			this.RaisePropertyChanged(nameof(this.SelectedItem));

			this.LoadEvents();
			this.PrepareTokenMenuItems();
		}

		private void LoadEvents()
		{
			this.events = Array.Empty<TokenEventDetail>();
			this.RaisePropertyChanged(nameof(this.Events));

			this.neuroFeaturesClient.GetEvents(this.selectedItem.TokenId, 0, 100, (sender, e) =>
			{
				if (e.Ok && e.Events.Length > 0 && e.Events[0].TokenId == (string)e.State)
				{
					int i, c = e.Events.Length;
					TokenEventDetail[] Events = new TokenEventDetail[c];

					for (i = 0; i < c; i++)
						Events[i] = TokenEventDetail.Create(e.Events[i]);

					this.events = Events;
					this.RaisePropertyChanged(nameof(this.Events));
				}

				return Task.CompletedTask;
			}, this.selectedItem.TokenId);
		}

		private void PrepareTokenMenuItems()
		{
			List<Control> Items = new();
			TokenModel Model = this.selectedItem;

			if (Model is not null)
			{
				Items.Add(new MenuItem()
				{
					Header = "Add Text Note...",
					Command = this.addTextNote
				});

				Items.Add(new MenuItem()
				{
					Header = "Add XML Note...",
					Command = this.addXmlNote
				});
				
				Items.Add(new Separator());

				Items.Add(new MenuItem()
				{
					Header = "Present State...",
					Command = Model.ViewPresentReport
				});

				Items.Add(new MenuItem()
				{
					Header = "History...",
					Command = Model.ViewHistoryReport
				});

				Items.Add(new MenuItem()
				{
					Header = "State Diagram...",
					Command = Model.ViewStateDiagramReport
				});

				Items.Add(new MenuItem()
				{
					Header = "Profiling...",
					Command = Model.ViewProfilingReport
				});

				if ((Model.NoteCommands?.Length ?? 0) > 0)
				{
					Items.Add(new Separator());

					int i = 0;

					foreach (NoteCommand Command in Model.NoteCommands)
					{
						Items.Add(new MenuItem()
						{
							Header = Command.Title?.Find("en"),
							ToolTip = Command.ToolTip?.Find("en"),
							Command = Model.NoteCommand,
							CommandParameter = ++i
						});
					}
				}
			}

			this.tokenMenuItems = Items.ToArray();
			this.RaisePropertyChanged(nameof(this.TokenMenuItems));
		}

		private Task NeuroFeaturesClient_TokenRemoved(object Sender, TokenEventArgs e)
		{
			MainWindow.UpdateGui(() =>
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
					return this.tokens.ToArray();
				}
			}
		}

		/// <summary>
		/// Selected item
		/// </summary>
		public TokenModel SelectedItem
		{
			get => this.selectedItem;
			set => this.selectedItem = value;
		}

		/// <summary>
		/// Events of selected item.
		/// </summary>
		public TokenEventDetail[] Events => this.events;

		/// <summary>
		/// Context-menu items for selected token.
		/// </summary>
		public Control[] TokenMenuItems => this.tokenMenuItems;

		public IEnumerable<TokenTotal> Totals
		{
			get
			{
				lock (this.totals)
				{
					return this.totals.ToArray();
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
				Log.Critical(ex);
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
			string s = MainWindow.PromptUser("Add Text Note", "Enter text note to add to token:");

			if (!string.IsNullOrEmpty(s))
			{
				if (this.selectedItem is null)
					MainWindow.ErrorBox("No token selected.");
				else
				{
					try
					{
						await this.neuroFeaturesClient.AddTextNoteAsync(this.selectedItem.TokenId, s);
						this.LoadEvents();
					}
					catch (Exception ex)
					{
						MainWindow.ErrorBox(ex.Message);
					}
				}
			}
		}

		private async Task ExecuteAddXmlNote()
		{
			string s = string.Empty;

			do
			{
				s = MainWindow.PromptUser("Add XML Note", "Enter XML note to add to token:", s);
				if (string.IsNullOrEmpty(s))
					return;
			}
			while (!XML.IsValidXml(s));

			if (this.selectedItem is null)
				MainWindow.ErrorBox("No token selected.");
			else
			{
				try
				{
					await this.neuroFeaturesClient.AddXmlNoteAsync(this.selectedItem.TokenId, s);
					this.LoadEvents();
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
			}
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.TokensTab.DataContext = this;
				return Task.CompletedTask;
			});

			await this.LoadTotals();

			int Offset = 0;
			int c = 20;

			TokensEventArgs e2 = await this.neuroFeaturesClient.GetTokensAsync(Offset, c);
			while (e2.Ok)
			{
				foreach (Token Token in e2.Tokens)
				{
					TokenModel TokenModel = await TokenModel.CreateAsync(this.neuroFeaturesClient, Token, "en");
					TokenModel.Selected += this.Token_Selected;
					TokenModel.Deselected += this.Token_Deselected;

					lock (this.tokens)
					{
						this.tokens.Add(TokenModel);
					}
				}

				this.RaisePropertyChanged(nameof(this.Tokens));

				Offset += e2.Tokens.Length;
				if (e2.Tokens.Length == 0)
					break;

				e2 = await this.neuroFeaturesClient.GetTokensAsync(Offset, c);
			}

			await base.Start();
		}

		private Task NeuroFeaturesClient_VariablesUpdated(object Sender, VariablesUpdatedEventArgs e)
		{
			MainWindow.UpdateGui(async () =>
			{
				try
				{
					foreach (object Control in MainWindow.currentInstance.TabControl.Items)
					{
						if (Control is not TabItem Tab)
							continue;

						if (Tab.Content is not ReportTab ReportTab)
							continue;

						if (ReportTab.Report.TokenId != e.TokenId)
							continue;

						await ReportTab.Report.OnVariablesUpdated(Sender, e);
					}
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
			});

			return Task.CompletedTask;
		}

		private Task NeuroFeaturesClient_StateUpdated(object Sender, NewStateEventArgs e)
		{
			MainWindow.UpdateGui(async () =>
			{
				try
				{
					foreach (object Control in MainWindow.currentInstance.TabControl.Items)
					{
						if (Control is not TabItem Tab)
							continue;

						if (Tab.Content is not ReportTab ReportTab)
							continue;

						if (ReportTab.Report.TokenId != e.TokenId)
							continue;

						await ReportTab.Report.OnNewState(Sender, e);
					}
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
			});

			return Task.CompletedTask;
		}

	}
}
