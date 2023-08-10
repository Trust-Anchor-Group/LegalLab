using EDaler;
using EDaler.Uris;
using EDaler.Uris.Incomplete;
using LegalLab.Dialogs.BuyEDaler;
using LegalLab.Dialogs.SellEDaler;
using LegalLab.Dialogs.TransferEDaler;
using LegalLab.Models.Network;
using NeuroFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace LegalLab.Models.Wallet
{
	/// <summary>
	/// Wallet Model
	/// </summary>
	[Singleton]
	public class WalletModel : PersistedModel, IDisposable
	{
		private readonly Property<decimal> amount;
		private readonly Property<decimal> reserved;
		private readonly Property<decimal> pending;
		private readonly Property<decimal> available;
		private readonly Property<string> currency;
		private readonly Property<DateTime> timestamp;
		private readonly Property<string> uri;

		private readonly List<AccountEventWrapper> events = new();

		private readonly Command sendUri;
		private readonly Command transferEDaler;
		private readonly Command buyEDaler;
		private readonly Command sellEDaler;

		private readonly ContractsClient contractsClient;
		private readonly EDalerClient eDalerClient;
		private readonly NetworkModel networkModel;
		private Balance balance = null;
		private AccountEventWrapper selectedItem = null;

		/// <summary>
		/// Wallet Model
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Contracts">Contracts Client</param>
		/// <param name="ComponentJid">Component JID</param>
		/// <param name="Network">Network model</param>
		public WalletModel(XmppClient Client, ContractsClient Contracts, string ComponentJid, NetworkModel Network)
			: base()
		{
			this.amount = new Property<decimal>(nameof(this.Amount), 0, this);
			this.pending = new Property<decimal>(nameof(this.Pending), 0, this);
			this.available = new Property<decimal>(nameof(this.Available), 0, this);
			this.reserved = new Property<decimal>(nameof(this.Reserved), 0, this);
			this.currency = new Property<string>(nameof(this.Currency), string.Empty, this);
			this.timestamp = new Property<DateTime>(nameof(this.Timestamp), DateTime.MinValue, this);
			this.uri = new Property<string>(nameof(this.Uri), string.Empty, this);

			this.sendUri = new Command(this.CanExecuteSendUri, this.ExecuteSendUri);
			this.transferEDaler = new Command(this.CanTransferEDaler, this.ExecuteTransferEDaler);
			this.buyEDaler = new Command(this.CanBuyEDaler, this.ExecuteBuyEDaler);
			this.sellEDaler = new Command(this.CanSellEDaler, this.ExecuteSellEDaler);

			this.contractsClient = Contracts;
			this.networkModel = Network;

			this.eDalerClient = new EDalerClient(Client, Contracts, ComponentJid);
			this.eDalerClient.BalanceUpdated += this.EDalerClient_BalanceUpdated;
			this.eDalerClient.BuyEDalerClientUrlReceived += this.EDalerClient_BuyEDalerClientUrlReceived;
			this.eDalerClient.BuyEDalerCompleted += this.EDalerClient_BuyEDalerCompleted;
			this.eDalerClient.BuyEDalerError += this.EDalerClient_BuyEDalerError;
			this.eDalerClient.BuyEDalerOptionsClientUrlReceived += this.EDalerClient_BuyEDalerOptionsClientUrlReceived;
			this.eDalerClient.BuyEDalerOptionsCompleted += this.EDalerClient_BuyEDalerOptionsCompleted;
			this.eDalerClient.BuyEDalerOptionsError += this.EDalerClient_BuyEDalerOptionsError;
			this.eDalerClient.SellEDalerClientUrlReceived += this.EDalerClient_SellEDalerClientUrlReceived;
			this.eDalerClient.SellEDalerCompleted += this.EDalerClient_SellEDalerCompleted;
			this.eDalerClient.SellEDalerError += this.EDalerClient_SellEDalerError;
			this.eDalerClient.SellEDalerOptionsClientUrlReceived += this.EDalerClient_SellEDalerOptionsClientUrlReceived;
			this.eDalerClient.SellEDalerOptionsCompleted += this.EDalerClient_SellEDalerOptionsCompleted;
			this.eDalerClient.SellEDalerOptionsError += this.EDalerClient_SellEDalerOptionsError;
		}

		private async Task EDalerClient_BalanceUpdated(object Sender, BalanceEventArgs e)
		{
			await this.SetBalance(e.Balance);

			AccountEventWrapper Item = new(e.Balance.Event);
			Item.Selected += this.Item_Selected;
			Item.Deselected += this.Item_Deselected;

			lock (this.events)
			{
				this.events.Insert(0, Item);
			}

			this.RaisePropertyChanged(nameof(this.Events));
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
		/// eDaler® client.
		/// </summary>
		public EDalerClient EDaler => this.eDalerClient;

		/// <summary>
		/// Balance amount
		/// </summary>
		public decimal Amount
		{
			get => this.amount.Value;
			set => this.amount.Value = value;
		}

		/// <summary>
		/// Reserved amount
		/// </summary>
		public decimal Reserved
		{
			get => this.reserved.Value;
			set => this.reserved.Value = value;
		}

		/// <summary>
		/// Pending amount
		/// </summary>
		public decimal Pending
		{
			get => this.pending.Value;
			set => this.pending.Value = value;
		}

		/// <summary>
		/// Available amount
		/// </summary>
		public decimal Available
		{
			get => this.available.Value;
			set => this.available.Value = value;
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
		/// eDaler® URI
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
		public Balance Balance => this.balance;

		/// <summary>
		/// Sets the balance of the wallet.
		/// </summary>
		/// <param name="Balance">Balance</param>
		public Task SetBalance(Balance Balance)
		{
			this.balance = Balance;
			return this.UpdateAmounts();
		}

		private async Task UpdateAmounts()
		{ 
			this.Amount = this.balance.Amount;
			this.Reserved = this.balance.Reserved;
			this.Currency = this.balance.Currency;
			this.Timestamp = this.balance.Timestamp;

			(decimal Pending, _, _) = await this.eDalerClient.GetPendingPayments();

			this.Pending = Pending;
			this.Available = this.Amount - this.Reserved - this.Pending;
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
					throw new Exception("Invalid eDaler® URI.");

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
		/// Command for transferring eDaler®.
		/// </summary>
		public ICommand TransferEDaler => this.transferEDaler;

		private bool CanTransferEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteTransferEDaler()
		{
			try
			{
				MainWindow.MouseHourglass();

				CreationAttributesEventArgs DefaultArgs = await this.networkModel.Tokens.NeuroFeaturesClient.GetCreationAttributesAsync();

				MainWindow.MouseDefault();

				TransferEDalerDialog Dialog = new();
				TransferEDalerModel Model = new(Dialog, DefaultArgs.Currency);

				bool? Result = Dialog.ShowDialog();
				if (!Result.HasValue || !Result.Value)
					return;

				this.Uri = await this.eDalerClient.CreateFullPaymentUri(Model.Recipient, Model.Amount,
					Model.AmountExtra > 0 ? Model.AmountExtra : (decimal?)null, Model.Currency, Model.ValidNrDays, Model.Message);

				await this.UpdateAmounts();
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Command for buying eDaler®.
		/// </summary>
		public ICommand BuyEDaler => this.buyEDaler;

		private bool CanBuyEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteBuyEDaler()
		{
			try
			{
				MainWindow.MouseHourglass();

				IBuyEDalerServiceProvider[] Providers = await this.eDalerClient.GetServiceProvidersForBuyingEDalerAsync();
				CreationAttributesEventArgs DefaultArgs = await this.networkModel.Tokens.NeuroFeaturesClient.GetCreationAttributesAsync();

				MainWindow.MouseDefault();

				BuyEDalerDialog Dialog = new();
				BuyEDalerModel Model = new(Dialog, Providers, DefaultArgs.Currency);

				bool? Result = Dialog.ShowDialog();
				if (!Result.HasValue || !Result.Value)
					return;

				if (Dialog.ServiceProvider.SelectedItem is not ServiceProviderModel ServiceProviderModel ||
					ServiceProviderModel.ServiceProvider is not IBuyEDalerServiceProvider ServiceProvider)
				{
					throw new Exception("Cannot buy eDaler® using that service provider.");
				}

				if (string.IsNullOrEmpty(ServiceProvider.BuyEDalerTemplateContractId))
				{
					await this.eDalerClient.InitiateBuyEDalerAsync(ServiceProvider.Id, ServiceProvider.Type,
						Model.Amount, Model.Currency);

					// Server will ask client to open web URL via event.
				}
				else
				{
					MainWindow.MouseHourglass();

					string TemplateName = "Buy eDaler® using " + ServiceProvider.Name;
					string Key = "Contract.Template." + TemplateName;
					string StoredId = await RuntimeSettings.GetAsync(Key, string.Empty);

					if (StoredId != ServiceProvider.BuyEDalerTemplateContractId)
					{
						Contract Contract = await this.contractsClient.GetContractAsync(ServiceProvider.BuyEDalerTemplateContractId);
						if (!Contract.CanActAsTemplate)
							throw new Exception("Contract referenced by service provider is not a template.");

						await RuntimeSettings.SetAsync(Key, Contract.ContractId);
						this.networkModel.Legal.ContractTemplateAdded(TemplateName, Contract);
					}

					Dictionary<CaseInsensitiveString, object> PresetValues = new()
					{
						{ "Amount", Model.Amount },
						{ "Currency", Model.Currency }
					};

					await this.networkModel.Legal.SetContractTemplateName(TemplateName, PresetValues);

					foreach (TabItem Item in MainWindow.currentInstance.TabControl.Items)
					{
						if (Item.Content == MainWindow.currentInstance.ContractsTab)
						{
							MainWindow.currentInstance.TabControl.SelectedItem = Item;
							break;
						}
					}

					MainWindow.MouseDefault();

					string TransactionId = Guid.NewGuid().ToString();

					await this.eDalerClient.InitiateGetOptionsBuyEDalerAsync(ServiceProvider.Id, ServiceProvider.Type, TransactionId, null, null, null);
				}
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private Task EDalerClient_BuyEDalerClientUrlReceived(object Sender, BuyEDalerClientUrlEventArgs e)
		{
			OpenUrl(e.ClientUrl);
			return Task.CompletedTask;
		}

		private static void OpenUrl(string Url)
		{
			ProcessStartInfo StartInfo = new()
			{
				FileName = Url,
				UseShellExecute = true
			};

			Process.Start(StartInfo);
		}

		private Task EDalerClient_BuyEDalerCompleted(object Sender, PaymentCompletedEventArgs e)
		{
			MainWindow.SuccessBox("Successfully bought " + e.Amount.ToString() + " " + e.Currency + " eDaler®.");
			return Task.CompletedTask;
		}

		private Task EDalerClient_BuyEDalerError(object Sender, PaymentErrorEventArgs e)
		{
			MainWindow.ErrorBox("Unable to buy eDaler®: " + e.Message);
			return Task.CompletedTask;
		}

		private Task EDalerClient_BuyEDalerOptionsClientUrlReceived(object Sender, BuyEDalerClientUrlEventArgs e)
		{
			OpenUrl(e.ClientUrl);
			return Task.CompletedTask;
		}

		private Task EDalerClient_BuyEDalerOptionsCompleted(object Sender, PaymentOptionsEventArgs e)
		{
			ContractOptionsReceived(e.Options);
			return Task.CompletedTask;
		}

		private Task EDalerClient_BuyEDalerOptionsError(object Sender, PaymentErrorEventArgs e)
		{
			MainWindow.ErrorBox("Unable to get payment options for buying eDaler®: " + e.Message);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Command for selling eDaler®.
		/// </summary>
		public ICommand SellEDaler => this.sellEDaler;

		private bool CanSellEDaler()
		{
			return this.eDalerClient.Client.State == XmppState.Connected;
		}

		private async Task ExecuteSellEDaler()
		{
			try
			{
				MainWindow.MouseHourglass();

				ISellEDalerServiceProvider[] Providers = await this.eDalerClient.GetServiceProvidersForSellingEDalerAsync();
				CreationAttributesEventArgs DefaultArgs = await this.networkModel.Tokens.NeuroFeaturesClient.GetCreationAttributesAsync();

				MainWindow.MouseDefault();

				SellEDalerDialog Dialog = new();
				SellEDalerModel Model = new(Dialog, Providers, DefaultArgs.Currency);

				bool? Result = Dialog.ShowDialog();
				if (!Result.HasValue || !Result.Value)
					return;

				if (Dialog.ServiceProvider.SelectedItem is not ServiceProviderModel ServiceProviderModel ||
					ServiceProviderModel.ServiceProvider is not ISellEDalerServiceProvider ServiceProvider)
				{
					throw new Exception("Cannot sell eDaler® using that service provider.");
				}

				if (string.IsNullOrEmpty(ServiceProvider.SellEDalerTemplateContractId))
				{
					await this.eDalerClient.InitiateSellEDalerAsync(ServiceProvider.Id, ServiceProvider.Type,
						Model.Amount, Model.Currency);

					// Server will ask client to open web URL via event.
				}
				else
				{
					MainWindow.MouseHourglass();

					string TemplateName = "Sell eDaler® using " + ServiceProvider.Name;
					string Key = "Contract.Template." + TemplateName;
					string StoredId = await RuntimeSettings.GetAsync(Key, string.Empty);

					if (StoredId != ServiceProvider.SellEDalerTemplateContractId)
					{
						Contract Contract = await this.contractsClient.GetContractAsync(ServiceProvider.SellEDalerTemplateContractId);
						if (!Contract.CanActAsTemplate)
							throw new Exception("Contract referenced by service provider is not a template.");

						await RuntimeSettings.SetAsync(Key, Contract.ContractId);
						this.networkModel.Legal.ContractTemplateAdded(TemplateName, Contract);
					}

					Dictionary<CaseInsensitiveString, object> PresetValues = new()
					{
						{ "Amount", Model.Amount },
						{ "Currency", Model.Currency }
					};

					await this.networkModel.Legal.SetContractTemplateName(TemplateName, PresetValues);

					foreach (TabItem Item in MainWindow.currentInstance.TabControl.Items)
					{
						if (Item.Content == MainWindow.currentInstance.ContractsTab)
						{
							MainWindow.currentInstance.TabControl.SelectedItem = Item;
							break;
						}
					}

					MainWindow.MouseDefault();

					string TransactionId = Guid.NewGuid().ToString();

					await this.eDalerClient.InitiateGetOptionsSellEDalerAsync(ServiceProvider.Id, ServiceProvider.Type, TransactionId, null, null, null);
				}
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private Task EDalerClient_SellEDalerClientUrlReceived(object Sender, SellEDalerClientUrlEventArgs e)
		{
			OpenUrl(e.ClientUrl);
			return Task.CompletedTask;
		}

		private Task EDalerClient_SellEDalerCompleted(object Sender, PaymentCompletedEventArgs e)
		{
			MainWindow.SuccessBox("Successfully sold " + e.Amount.ToString() + " " + e.Currency + " eDaler®.");
			return Task.CompletedTask;
		}

		private Task EDalerClient_SellEDalerError(object Sender, PaymentErrorEventArgs e)
		{
			MainWindow.ErrorBox("Unable to sell eDaler®: " + e.Message);
			return Task.CompletedTask;
		}

		private Task EDalerClient_SellEDalerOptionsClientUrlReceived(object Sender, SellEDalerClientUrlEventArgs e)
		{
			OpenUrl(e.ClientUrl);
			return Task.CompletedTask;
		}

		private Task EDalerClient_SellEDalerOptionsCompleted(object Sender, PaymentOptionsEventArgs e)
		{
			ContractOptionsReceived(e.Options);
			return Task.CompletedTask;
		}

		private Task EDalerClient_SellEDalerOptionsError(object Sender, PaymentErrorEventArgs e)
		{
			MainWindow.ErrorBox("Unable to get payment options for selling eDaler®: " + e.Message);
			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.WalletTab.DataContext = this;
				return Task.CompletedTask;
			});

			await this.SetBalance(await this.eDalerClient.GetBalanceAsync());

			(AccountEvent[] Events, bool More) = await this.eDalerClient.GetAccountEventsAsync(50);

			lock (this.events)
			{
				foreach (AccountEvent Event in Events)
				{
					AccountEventWrapper Item = new(Event);
					Item.Selected += this.Item_Selected;
					Item.Deselected += this.Item_Deselected;

					this.events.Add(Item);
				}
			}

			this.RaisePropertyChanged(nameof(this.Events));

			await base.Start();
		}

		private static void ContractOptionsReceived(IDictionary<CaseInsensitiveString, object>[] Options)
		{
			// TODO
		}
	}
}
