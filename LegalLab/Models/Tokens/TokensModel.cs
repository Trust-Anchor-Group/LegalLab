using NeuroFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		}

		private Task NeuroFeaturesClient_TokenAdded(object Sender, TokenEventArgs e)
		{
			TokenModel Token = new TokenModel(e.Token);
			Token.Selected += Token_Selected;
			Token.Deselected += Token_Deselected;

			lock (this.tokens)
			{
				this.tokens.Insert(0, Token);
			}

			this.RaisePropertyChanged(nameof(this.Tokens));

			Task _ = this.LoadTotals();

			return Task.CompletedTask;
		}

		private void Token_Deselected(object sender, EventArgs e)
		{
			if (this.selectedItem == sender)
			{
				this.selectedItem = null;
				this.RaisePropertyChanged(nameof(this.SelectedItem));
			}
		}

		private void Token_Selected(object sender, EventArgs e)
		{
			this.selectedItem = sender as TokenModel;
			this.RaisePropertyChanged(nameof(this.SelectedItem));
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
				lock (this.tokens)
				{
					foreach (Token Token in e2.Tokens)
					{
						TokenModel TokenModel = new TokenModel(Token);
						TokenModel.Selected += Token_Selected;
						TokenModel.Deselected += Token_Deselected;

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
