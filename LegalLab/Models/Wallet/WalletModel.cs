using EDaler;
using EDaler.Uris;
using EDaler.Uris.Incomplete;
using LegalLab.Dialogs.AddLanguage;
using LegalLab.Dialogs.TransferEDaler;
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
		private readonly Property<double> reserved;
		private readonly Property<string> currency;
		private readonly Property<DateTime> timestamp;
		private readonly Property<string> uri;

		private readonly List<AccountEventWrapper> events = new List<AccountEventWrapper>();

		private readonly Command sendUri;
		private readonly Command transferEDaler;
		private readonly Command buyEDaler;
		private readonly Command sellEDaler;

		private readonly EDalerClient eDalerClient;
		private Balance balance = null;
		private AccountEventWrapper selectedItem = null;

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
			this.reserved = new Property<double>(nameof(this.Reserved), 0, this);
			this.currency = new Property<string>(nameof(this.Currency), string.Empty, this);
			this.timestamp = new Property<DateTime>(nameof(this.Timestamp), DateTime.MinValue, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);

			this.sendUri = new Command(this.CanExecuteSendUri, this.ExecuteSendUri);
			this.transferEDaler = new Command(this.CanTransferEDaler, this.ExecuteTransferEDaler);
			this.buyEDaler = new Command(this.CanBuyEDaler, this.ExecuteBuyEDaler);
			this.sellEDaler = new Command(this.CanSellEDaler, this.ExecuteSellEDaler);

			this.eDalerClient = new EDalerClient(Client, Contracts, ComponentJid);
			this.eDalerClient.BalanceUpdated += this.EDalerClient_BalanceUpdated;
		}

		private Task EDalerClient_BalanceUpdated(object Sender, BalanceEventArgs e)
		{
			this.Balance = e.Balance;

			AccountEventWrapper Item = new AccountEventWrapper(e.Balance.Event);
			Item.Selected += this.Item_Selected;
			Item.Deselected += this.Item_Deselected;

			lock (this.events)
			{
				this.events.Insert(0, Item);
			}

			this.RaisePropertyChanged(nameof(this.Events));

			return Task.CompletedTask;
		}

		private void Item_Deselected(object sender, EventArgs e)
		{
			if (this.selectedItem == sender)
			{
				this.selectedItem = null;
				this.RaisePropertyChanged(nameof(this.SelectedItem));
			}
		}

		private void Item_Selected(object sender, EventArgs e)
		{
			this.selectedItem = sender as AccountEventWrapper;
			this.RaisePropertyChanged(nameof(this.SelectedItem));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.eDalerClient.Dispose();
		}

		/// <summary>
		/// eDaler client.
		/// </summary>
		public EDalerClient EDaler => this.eDalerClient;

		/// <summary>
		/// Balance amount
		/// </summary>
		public double Amount
		{
			get => this.amount.Value;
			set => this.amount.Value = value;
		}

		/// <summary>
		/// Reserved amount
		/// </summary>
		public double Reserved
		{
			get => this.reserved.Value;
			set => this.reserved.Value = value;
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
				this.sendUri.RaiseCanExecuteChanged();
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
				this.Reserved = (double)this.balance.Reserved;
				this.Currency = this.balance.Currency;
				this.Timestamp = this.balance.Timestamp;
			}
		}

		/// <summary>
		/// Account events
		/// </summary>
		public IEnumerable<AccountEventWrapper> Events
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
		/// Selected item
		/// </summary>
		public AccountEventWrapper SelectedItem
		{
			get => this.selectedItem;
			set => this.selectedItem = value;
		}

		/// <summary>
		/// Command for sending URI to server.
		/// </summary>
		public ICommand SendUri => this.sendUri;

		private bool CanExecuteSendUri()
		{
			return this.eDalerClient.Client.State == XmppState.Connected && !string.IsNullOrEmpty(this.Uri);
		}

		private async Task ExecuteSendUri()
		{
			try
			{
				if (!EDalerUri.TryParse(this.Uri, out EDalerUri Uri))
					throw new Exception("Invalid eDaler URI.");

				if (Uri is EDalerIncompletePaymentUri IncompleteUri)
				{
					this.Uri = await this.eDalerClient.CreateFullPaymentUri(
						IncompleteUri.To,
						IncompleteUri.Amount,
						IncompleteUri.AmountExtra,
						IncompleteUri.Currency,
						(int)IncompleteUri.Expires.Subtract(DateTime.Today.AddDays(-1)).TotalDays);
				}

				await this.eDalerClient.SendEDalerUriAsync(this.Uri);
				this.Uri = string.Empty;
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Command for transferring eDaler.
		/// </summary>
		public ICommand TransferEDaler => this.transferEDaler;

		private bool CanTransferEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteTransferEDaler()
		{
			TransferEDalerDialog Dialog = new TransferEDalerDialog();
			TransferEDalerModel Model = new TransferEDalerModel(Dialog);

			bool? Result = Dialog.ShowDialog();
			if (!Result.HasValue || !Result.Value)
				return;

			this.Uri = await this.eDalerClient.CreateFullPaymentUri(Model.Recipient, Model.Amount,
				Model.AmountExtra > 0 ? Model.AmountExtra : (decimal?)null, Model.Currency, 3, Model.Message);
		}

		/// <summary>
		/// Command for buying eDaler.
		/// </summary>
		public ICommand BuyEDaler => this.buyEDaler;

		private bool CanBuyEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteBuyEDaler()
		{
			// TODO
		}

		/// <summary>
		/// Command for selling eDaler.
		/// </summary>
		public ICommand SellEDaler => this.sellEDaler;

		private bool CanSellEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteSellEDaler()
		{
			// TODO
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.WalletTab.DataContext = this;
				return Task.CompletedTask;
			});

			this.Balance = await this.eDalerClient.GetBalanceAsync();

			(AccountEvent[] Events, bool More) = await this.eDalerClient.GetAccountEventsAsync(50);

			lock (this.events)
			{
				foreach (AccountEvent Event in Events)
				{
					AccountEventWrapper Item = new AccountEventWrapper(Event);
					Item.Selected += this.Item_Selected;
					Item.Deselected += this.Item_Deselected;

					this.events.Add(Item);
				}
			}

			this.RaisePropertyChanged(nameof(this.Events));

			await base.Start();
		}

	}
}
