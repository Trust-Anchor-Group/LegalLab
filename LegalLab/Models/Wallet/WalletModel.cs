using EDaler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Wallet
{
	/// <summary>
	/// Wallet Model
	/// </summary>
	[Singleton]
	public class WalletModel : PersistedModel, IDisposable
	{
		private readonly Property<double> amount;
		private readonly Property<string> currency;
		private readonly Property<DateTime> timestamp;
		private readonly Property<string> uri;

		private readonly List<AccountEvent> events = new List<AccountEvent>();

		private readonly Command send;

		private readonly EDalerClient eDalerClient;
		private Balance balance = null;

		/// <summary>
		/// Wallet Model
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="ComponentJid">Component JID</param>
		public WalletModel(XmppClient Client, ContractsClient Contracts, string ComponentJid)
			: base()
		{
			this.amount = new Property<double>(nameof(this.Amount), 0, this);
			this.currency = new Property<string>(nameof(this.Currency), string.Empty, this);
			this.timestamp = new Property<DateTime>(nameof(this.Timestamp), DateTime.MinValue, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);

			this.send = new Command(this.CanExecuteSend, this.ExecuteSend);

			this.eDalerClient = new EDalerClient(Client, Contracts, ComponentJid);
			this.eDalerClient.BalanceUpdated += EDalerClient_BalanceUpdated;
		}

		private Task EDalerClient_BalanceUpdated(object Sender, BalanceEventArgs e)
		{
			this.Balance = e.Balance;

			lock (this.events)
			{
				this.events.Insert(0, e.Balance.Event);

			}

			this.RaisePropertyChanged(nameof(this.Events));

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.eDalerClient.Dispose();
		}

		/// <summary>
		/// Balance amount
		/// </summary>
		public double Amount
		{
			get => this.amount.Value;
			set => this.amount.Value = value;
		}

		/// <summary>
		/// Balance currency
		/// </summary>
		public string Currency
		{
			get => this.currency.Value;
			set => this.currency.Value = value;
		}

		/// <summary>
		/// Balance timestamp
		/// </summary>
		public DateTime Timestamp
		{
			get => this.timestamp.Value;
			set => this.timestamp.Value = value;
		}

		/// <summary>
		/// e-Daler URI
		/// </summary>
		public string Uri
		{
			get => this.uri.Value;
			set
			{
				this.uri.Value = value;
				this.send.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Latest balance
		/// </summary>
		public Balance Balance
		{
			get => this.balance;
			set
			{
				this.balance = value;

				this.Amount = (double)this.balance.Amount;
				this.Currency = this.balance.Currency;
				this.Timestamp = this.balance.Timestamp;
			}
		}

		/// <summary>
		/// Account events
		/// </summary>
		public IEnumerable<AccountEvent> Events
		{
			get
			{
				lock (this.events)
				{
					return this.events.ToArray();
				}
			}
		}

		/// <summary>
		/// Apply command
		/// </summary>
		public ICommand Send => this.send;

		private bool CanExecuteSend()
		{
			return this.eDalerClient.Client.State == XmppState.Connected && !string.IsNullOrEmpty(this.Uri);
		}

		private async void ExecuteSend()
		{
			try
			{
				await this.eDalerClient.SendEDalerUriAsync(this.Uri);
				this.Uri = string.Empty;
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.WalletTab.DataContext = this;
			});

			this.Balance = await this.eDalerClient.GetBalanceAsync();

			(AccountEvent[] Events, bool More) = await this.eDalerClient.GetAccountEventsAsync(50);

			lock (this.events)
			{
				this.events.AddRange(Events);
			}

			this.RaisePropertyChanged(nameof(this.Events));

			await base.Start();
		}

	}
}
