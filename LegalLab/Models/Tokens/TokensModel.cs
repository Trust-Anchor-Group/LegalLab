using LegalLab.Models.Tokens.Events;
using NeuroFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		private readonly List<TokenModel> tokens = new List<TokenModel>();
		private readonly List<TokenTotal> totals = new List<TokenTotal>();

		private readonly NeuroFeaturesClient neuroFeaturesClient;

		private TokenModel selectedItem = null;
		private TokenEventDetail[] events = null;

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

			this.neuroFeaturesClient.TokenAdded += NeuroFeaturesClient_TokenAdded;
			this.neuroFeaturesClient.TokenRemoved += NeuroFeaturesClient_TokenRemoved;

			this.addTextNote = new Command(this.CanExecuteAddNote, this.ExecuteAddTextNote);
			this.addXmlNote = new Command(this.CanExecuteAddNote, this.ExecuteAddXmlNote);
		}

		private async Task NeuroFeaturesClient_TokenAdded(object Sender, TokenEventArgs e)
		{
			TokenModel Token = await TokenModel.CreateAsync(this.neuroFeaturesClient, e.Token);
			Token.Selected += Token_Selected;
			Token.Deselected += Token_Deselected;

			lock (this.tokens)
			{
				this.tokens.Insert(0, Token);
			}

			this.RaisePropertyChanged(nameof(this.Tokens));

			Task _ = this.LoadTotals();
		}

		private void Token_Deselected(object sender, EventArgs e)
		{
			if (this.selectedItem == sender)
			{
				this.selectedItem = null;
				this.RaisePropertyChanged(nameof(this.SelectedItem));

				this.events = null;
				this.RaisePropertyChanged(nameof(this.Events));
			}
		}

		private void Token_Selected(object sender, EventArgs e)
		{
			this.selectedItem = sender as TokenModel;
			this.RaisePropertyChanged(nameof(this.SelectedItem));

			this.LoadEvents();
		}

		private void LoadEvents()
		{
			this.events = new TokenEventDetail[0];
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

		private Task NeuroFeaturesClient_TokenRemoved(object Sender, TokenEventArgs e)
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
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.neuroFeaturesClient.Dispose();
		}

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
			return !(this.selectedItem is null);
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
				s = MainWindow.PromptUser("Add Text Note", "Enter text note to add to token:", s);
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
					TokenModel TokenModel = await TokenModel.CreateAsync(this.neuroFeaturesClient, Token);
					TokenModel.Selected += Token_Selected;
					TokenModel.Deselected += Token_Deselected;

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

	}
}
