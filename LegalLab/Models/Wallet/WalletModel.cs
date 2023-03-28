using EDaler;
using EDaler.Uris;
using EDaler.Uris.Incomplete;
using LegalLab.Dialogs.BuyEDaler;
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
			this.amount = new Property<double>(nameof(this.Amount), 0, this);
			this.reserved = new Property<double>(nameof(this.Reserved), 0, this);
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
			this.eDalerClient.SellEDalerClientUrlReceived += this.EDalerClient_SellEDalerClientUrlReceived;
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
			try
			{
				MainWindow.MouseHourglass();

				CreationAttributesEventArgs DefaultArgs = await this.networkModel.Tokens.NeuroFeaturesClient.GetCreationAttributesAsync();

				MainWindow.MouseDefault();

				TransferEDalerDialog Dialog = new TransferEDalerDialog();
				TransferEDalerModel Model = new TransferEDalerModel(Dialog, DefaultArgs.Currency);

				bool? Result = Dialog.ShowDialog();
				if (!Result.HasValue || !Result.Value)
					return;

				this.Uri = await this.eDalerClient.CreateFullPaymentUri(Model.Recipient, Model.Amount,
					Model.AmountExtra > 0 ? Model.AmountExtra : (decimal?)null, Model.Currency, 3, Model.Message);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
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
			try
			{
				MainWindow.MouseHourglass();

				IBuyEDalerServiceProvider[] Providers = await this.eDalerClient.GetServiceProvidersForBuyingEDalerAsync();
				CreationAttributesEventArgs DefaultArgs = await this.networkModel.Tokens.NeuroFeaturesClient.GetCreationAttributesAsync();

				MainWindow.MouseDefault();

				BuyEDalerDialog Dialog = new BuyEDalerDialog();
				BuyEDalerModel Model = new BuyEDalerModel(Dialog, Providers, DefaultArgs.Currency);

				bool? Result = Dialog.ShowDialog();
				if (!Result.HasValue || !Result.Value)
					return;

				if (!(Dialog.ServiceProvider.SelectedItem is ServiceProviderModel ServiceProviderModel) ||
					!(ServiceProviderModel.ServiceProvider is IBuyEDalerServiceProvider ServiceProvider))
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

					string TemplateName = "Buy eDaler from " + ServiceProvider.Name;
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

					Dictionary<CaseInsensitiveString, object> PresetValues = new Dictionary<CaseInsensitiveString, object>()
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
				}
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private Task EDalerClient_BuyEDalerClientUrlReceived(object Sender, BuyEDalerClientUrlEventArgs e)
		{
			ProcessStartInfo StartInfo = new ProcessStartInfo
			{
				FileName = e.ClientUrl,
				UseShellExecute = true
			};

			Process.Start(StartInfo);

			return Task.CompletedTask;
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

		private Task EDalerClient_SellEDalerClientUrlReceived(object Sender, SellEDalerClientUrlEventArgs e)
		{
			ProcessStartInfo StartInfo = new ProcessStartInfo
			{
				FileName = e.ClientUrl,
				UseShellExecute = true
			};

			Process.Start(StartInfo);

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
