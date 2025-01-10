using EDaler;
using LegalLab.Extensions;
using LegalLab.Models.Legal;
using LegalLab.Models.Network.Sniffer;
using LegalLab.Models.Tokens;
using LegalLab.Models.Wallet;
using NeuroFeatures;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Waher.Events;
using Waher.Networking.DNS;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace LegalLab.Models.Network
{
	/// <summary>
	/// Network Model
	/// </summary>
	[Singleton]
	public class NetworkModel : PersistedModel
	{
		private readonly PersistedProperty<string> savedAccounts;
		private readonly PersistedProperty<string> selectedAccount;
		private readonly PersistedProperty<string> xmppServer;
		private readonly PersistedProperty<string> account;
		private readonly PersistedProperty<string> password;
		private readonly PersistedProperty<string> passwordMethod;
		private readonly PersistedProperty<string> apiKey;
		private readonly PersistedProperty<string> apiKeySecret;
		private readonly PersistedProperty<string> legalComponentJid;
		private readonly PersistedProperty<string> eDalerComponentJid;
		private readonly PersistedProperty<string> neuroFeaturesComponentJid;
		private readonly PersistedProperty<string> httpFileUploadComponentJid;
		private readonly PersistedProperty<long> httpFileUploadMaxSize;
		private readonly PersistedProperty<bool> createAccount;
		private readonly PersistedProperty<bool> trustServerCertificate;
		private readonly PersistedProperty<bool> allowInsecureAlgorithms;
		private readonly PersistedProperty<bool> storePasswordInsteadOfDigest;
		private readonly PersistedProperty<bool> connectOnStartup;
		private readonly Property<string> password2;
		private readonly Property<XmppState> state;
		private readonly Property<bool> connected;

		private readonly Command connect;
		private readonly Command disconnect;
		private readonly Command randomizePassword;
		private readonly ParametrizedCommand copySnifferItem;
		private readonly ParametrizedCommand removeSnifferItem;
		private readonly Command clearSniffer;
		private readonly Command saveCredentials;
		private readonly Command deleteCredentials;
		private readonly Command newAccount;

		private XmppClient client;
		private LegalModel legalModel;
		private WalletModel walletModel;
		private TokensModel tokensModel;

		private bool loading = false;


		public NetworkModel()
			: base()
		{
			this.loading = true;
			try
			{
				this.Add(this.savedAccounts = new PersistedProperty<string>("Accounts", nameof(this.SavedAccounts), true, string.Empty, this));
				this.Add(this.selectedAccount = new PersistedProperty<string>("Accounts", nameof(this.SelectedAccount), true, string.Empty, this));
				this.Add(this.xmppServer = new PersistedProperty<string>("XMPP", nameof(this.XmppServer), false, string.Empty, this));
				this.Add(this.account = new PersistedProperty<string>("XMPP", nameof(this.Account), false, string.Empty, this));
				this.Add(this.password = new PersistedProperty<string>("XMPP", nameof(this.Password), false, string.Empty, this));
				this.Add(this.passwordMethod = new PersistedProperty<string>("XMPP", nameof(this.PasswordMethod), false, string.Empty, this));
				this.Add(this.apiKey = new PersistedProperty<string>("XMPP", nameof(this.ApiKey), false, string.Empty, this));
				this.Add(this.apiKeySecret = new PersistedProperty<string>("XMPP", nameof(this.ApiKeySecret), false, string.Empty, this));
				this.Add(this.createAccount = new PersistedProperty<bool>("XMPP", nameof(this.CreateAccount), false, false, this));
				this.Add(this.trustServerCertificate = new PersistedProperty<bool>("XMPP", nameof(this.TrustServerCertificate), false, false, this));
				this.Add(this.allowInsecureAlgorithms = new PersistedProperty<bool>("XMPP", nameof(this.AllowInsecureAlgorithms), false, false, this));
				this.Add(this.storePasswordInsteadOfDigest = new PersistedProperty<bool>("XMPP", nameof(this.StorePasswordInsteadOfDigest), false, false, this));
				this.Add(this.connectOnStartup = new PersistedProperty<bool>("XMPP", nameof(this.ConnectOnStartup), false, false, this));

				this.Add(this.legalComponentJid = new PersistedProperty<string>("XMPP", nameof(this.LegalComponentJid), true, string.Empty, this));
				this.Add(this.eDalerComponentJid = new PersistedProperty<string>("XMPP", nameof(this.EDalerComponentJid), true, string.Empty, this));
				this.Add(this.neuroFeaturesComponentJid = new PersistedProperty<string>("XMPP", nameof(this.NeuroFeaturesComponentJid), true, string.Empty, this));
				this.Add(this.httpFileUploadComponentJid = new PersistedProperty<string>("XMPP", nameof(this.HttpFileUploadComponentJid), true, string.Empty, this));
				this.Add(this.httpFileUploadMaxSize = new PersistedProperty<long>("XMPP", nameof(this.HttpFileUploadMaxSize), true, 0L, this));

				this.password2 = new Property<string>(nameof(this.Password2), string.Empty, this);
				this.state = new Property<XmppState>(nameof(this.State), XmppState.Offline, this);
				this.connected = new Property<bool>(nameof(this.Connected), false, this);

				this.connect = new Command(this.CanExecuteConnect, this.ExecuteConnect);
				this.disconnect = new Command(this.CanExecuteDisconnect, this.ExecuteDisconnect);
				this.randomizePassword = new Command(this.CanExecuteRandomizePassword, ExecuteRandomizePassword);
				this.copySnifferItem = new ParametrizedCommand(this.CanExecuteCopy, this.ExecuteCopy);
				this.removeSnifferItem = new ParametrizedCommand(this.CanExecuteRemove, this.ExecuteRemove);
				this.clearSniffer = new Command(this.CanExecuteClearAll, this.ExecuteClearAll);
				this.saveCredentials = new Command(this.CanExecuteSaveCredentials, this.ExecuteSaveCredentials);
				this.deleteCredentials = new Command(this.CanExecuteDeleteCredentials, this.ExecuteDeleteCredentials);
				this.newAccount = new Command(this.ExecuteNewAccount);
			}
			finally
			{
				this.loading = false;
			}
		}

		/// <summary>
		/// Saved accounts
		/// </summary>
		public string[] SavedAccounts
		{
			get
			{
				string s = this.savedAccounts.Value;
				if (string.IsNullOrEmpty(s))
					return [];
				else
					return s.Split('|');
			}

			set
			{
				StringBuilder sb = new();
				bool First = true;

				foreach (string Item in value)
				{
					if (First)
						First = false;
					else
						sb.Append('|');

					sb.Append(Item);
				}

				this.savedAccounts.Value = sb.ToString();
			}
		}

		/// <summary>
		/// Selected account
		/// </summary>
		public string SelectedAccount
		{
			get => this.selectedAccount.Value;
			set
			{
				this.selectedAccount.Value = value;

				this.deleteCredentials.RaiseCanExecuteChanged();

				if (!this.loading && !string.IsNullOrEmpty(value))
					Task.Run(() => MainWindow.UpdateGui(async () => await this.LoadCredentials(value)));
			}
		}

		private async Task LoadCredentials(string Account)
		{
			int i = Account.IndexOf('@');
			if (i < 0)
				return;

			string Prefix = "Credentials." + Account + ".";
			bool Connect = this.Connected;

			this.Account = Account[..i];
			this.XmppServer = Account[(i + 1)..];
			this.Password = await RuntimeSettings.GetAsync(Prefix + "Password", string.Empty);
			this.PasswordMethod = await RuntimeSettings.GetAsync(Prefix + "PasswordMethod", string.Empty);
			this.ApiKey = await RuntimeSettings.GetAsync(Prefix + "ApiKey", string.Empty);
			this.ApiKeySecret = await RuntimeSettings.GetAsync(Prefix + "ApiKeySecret", string.Empty);
			this.LegalComponentJid = await RuntimeSettings.GetAsync(Prefix + "LegalComponentJid", string.Empty);
			this.EDalerComponentJid = await RuntimeSettings.GetAsync(Prefix + "EDalerComponentJid", string.Empty);
			this.NeuroFeaturesComponentJid = await RuntimeSettings.GetAsync(Prefix + "NeuroFeaturesComponentJid", string.Empty);
			this.HttpFileUploadComponentJid = await RuntimeSettings.GetAsync(Prefix + "HttpFileUploadComponentJid", string.Empty);
			this.HttpFileUploadMaxSize = await RuntimeSettings.GetAsync(Prefix + "HttpFileUploadMaxSize", 0L);
			this.TrustServerCertificate = await RuntimeSettings.GetAsync(Prefix + "TrustServerCertificate", false);
			this.AllowInsecureAlgorithms = await RuntimeSettings.GetAsync(Prefix + "AllowInsecureAlgorithms", false);
			this.StorePasswordInsteadOfDigest = await RuntimeSettings.GetAsync(Prefix + "StorePasswordInsteadOfDigest", false);
			this.ConnectOnStartup = await RuntimeSettings.GetAsync(Prefix + "ConnectOnStartup", false);
			this.CreateAccount = false;
			this.Password2 = string.Empty;

			await this.ExecuteDisconnect();
			await this.ExecuteClearAll();

			if (Connect)
				await this.ExecuteConnect();
		}

		/// <summary>
		/// Domain name of XMPP Server
		/// </summary>
		public string XmppServer
		{
			get => this.xmppServer.Value;
			set => this.xmppServer.Value = value;
		}

		/// <summary>
		/// Account name of XMPP Server
		/// </summary>
		public string Account
		{
			get => this.account.Value;
			set => this.account.Value = value;
		}

		/// <summary>
		/// Password of account (or digest of password).
		/// </summary>
		public string Password
		{
			get => this.password.Value;
			set
			{
				if (this.password.Value != value)
				{
					this.password.Value = value;
					this.PasswordMethod = string.Empty;
				}
			}
		}

		/// <summary>
		/// Method of digest, if password digest is used.
		/// </summary>
		public string PasswordMethod
		{
			get => this.passwordMethod.Value;
			set => this.passwordMethod.Value = value;
		}

		/// <summary>
		/// API Key, if used to create an account.
		/// </summary>
		public string ApiKey
		{
			get => this.apiKey.Value;
			set => this.apiKey.Value = value;
		}

		/// <summary>
		/// API Key Secret, if used to create an account.
		/// </summary>
		public string ApiKeySecret
		{
			get => this.apiKeySecret.Value;
			set => this.apiKeySecret.Value = value;
		}

		/// <summary>
		/// If an account can be created
		/// </summary>
		public bool CreateAccount
		{
			get => this.createAccount.Value;
			set
			{
				this.createAccount.Value = value;
				this.connect.RaiseCanExecuteChanged();
				this.randomizePassword.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// If server certificates should be trusted (even if they don't validate).
		/// </summary>
		public bool TrustServerCertificate
		{
			get => this.trustServerCertificate.Value;
			set => this.trustServerCertificate.Value = value;
		}

		/// <summary>
		/// If insecure algorithms should be allowed or not.
		/// </summary>
		public bool AllowInsecureAlgorithms
		{
			get => this.allowInsecureAlgorithms.Value;
			set => this.allowInsecureAlgorithms.Value = value;
		}

		/// <summary>
		/// If passwords are to be stored, insteaed of digests
		/// </summary>
		public bool StorePasswordInsteadOfDigest
		{
			get => this.storePasswordInsteadOfDigest.Value;
			set => this.storePasswordInsteadOfDigest.Value = value;
		}

		/// <summary>
		/// If the application should connect to the XMPP network on startup.
		/// </summary>
		public bool ConnectOnStartup
		{
			get => this.connectOnStartup.Value;
			set => this.connectOnStartup.Value = value;
		}

		/// <summary>
		/// Legal Component JID
		/// </summary>
		public string LegalComponentJid
		{
			get => this.legalComponentJid.Value;
			set => this.legalComponentJid.Value = value;
		}

		/// <summary>
		/// eDaler® Component JID
		/// </summary>
		public string EDalerComponentJid
		{
			get => this.eDalerComponentJid.Value;
			set => this.eDalerComponentJid.Value = value;
		}

		/// <summary>
		/// Neuro-Features Component JID
		/// </summary>
		public string NeuroFeaturesComponentJid
		{
			get => this.neuroFeaturesComponentJid.Value;
			set => this.neuroFeaturesComponentJid.Value = value;
		}

		/// <summary>
		/// HTTP File Upload Component JID
		/// </summary>
		public string HttpFileUploadComponentJid
		{
			get => this.httpFileUploadComponentJid.Value;
			set => this.httpFileUploadComponentJid.Value = value;
		}

		/// <summary>
		/// HTTP File Upload Maximum size
		/// </summary>
		public long HttpFileUploadMaxSize
		{
			get => this.httpFileUploadMaxSize.Value;
			set => this.httpFileUploadMaxSize.Value = value;
		}

		/// <summary>
		/// Connection command
		/// </summary>
		public ICommand Connect => this.connect;

		/// <summary>
		/// Disconnection command
		/// </summary>
		public ICommand Disconnect => this.disconnect;

		/// <summary>
		/// Password randomization command
		/// </summary>
		public ICommand RandomizePassword => this.randomizePassword;

		/// <summary>
		/// Second entry of password
		/// </summary>
		public string Password2
		{
			get => this.password2.Value;
			set
			{
				this.password2.Value = value;
				this.connect.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Connection state.
		/// </summary>
		public XmppState State
		{
			get => this.state.Value;
			set
			{
				this.state.Value = value;
				this.connect.RaiseCanExecuteChanged();
				this.disconnect.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// If the client is connected.
		/// </summary>
		public bool Connected
		{
			get => this.connected.Value;
			set
			{
				this.connected.Value = value;
				this.connect.RaiseCanExecuteChanged();
				this.disconnect.RaiseCanExecuteChanged();
				this.randomizePassword.RaiseCanExecuteChanged();
				this.saveCredentials.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Legal ID model
		/// </summary>
		public LegalModel Legal => this.legalModel;

		/// <summary>
		/// Wallet model
		/// </summary>
		public WalletModel Wallet => this.walletModel;

		/// <summary>
		/// Tokens model
		/// </summary>
		public TokensModel Tokens => this.tokensModel;

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override async Task Start()
		{
			await MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.DataContext = this;
				MainWindow.currentInstance.XmppState.DataContext = this;

				MainWindow.currentInstance.NetworkTab.XmppPassword.Password = this.Password;
				MainWindow.currentInstance.NetworkTab.ApiKeySecret.Password = this.ApiKeySecret;

				MainWindow.currentInstance.NetworkTab.XmppPassword.PasswordChanged += this.PasswordChanged;
				MainWindow.currentInstance.NetworkTab.XmppPassword2.PasswordChanged += this.Password2Changed;
				MainWindow.currentInstance.NetworkTab.ApiKeySecret.PasswordChanged += this.ApiKeySecretChanged;

				return Task.CompletedTask;
			});

			if (this.ConnectOnStartup)
				await this.ExecuteConnect();

			await base.Start();
		}

		/// <summary>
		/// Stops the model.
		/// </summary>
		public override async Task Stop()
		{
			MainWindow.currentInstance.NetworkTab.XmppPassword.PasswordChanged -= this.PasswordChanged;
			MainWindow.currentInstance.NetworkTab.XmppPassword2.PasswordChanged -= this.Password2Changed;
			MainWindow.currentInstance.NetworkTab.ApiKeySecret.PasswordChanged -= this.ApiKeySecretChanged;

			if (this.legalModel is not null)
			{
				await this.legalModel.Stop();
				this.legalModel.Dispose();
				this.legalModel = null;
			}

			if (this.walletModel is not null)
			{
				await this.walletModel.Stop();
				this.walletModel.Dispose();
				this.walletModel = null;
			}

			if (this.client is not null)
			{
				await this.client.DisposeAsync();
				this.client = null;
			}

			await base.Stop();
		}

		private void PasswordChanged(object sender, RoutedEventArgs e)
		{
			this.Password = MainWindow.currentInstance.NetworkTab.XmppPassword.Password;
		}

		private void Password2Changed(object sender, RoutedEventArgs e)
		{
			this.Password2 = MainWindow.currentInstance.NetworkTab.XmppPassword.Password;
		}

		private void ApiKeySecretChanged(object sender, RoutedEventArgs e)
		{
			this.ApiKeySecret = MainWindow.currentInstance.NetworkTab.ApiKeySecret.Password;
		}

		private bool CanExecuteRandomizePassword()
		{
			return this.client is null && this.CreateAccount;
		}

		public static Task ExecuteRandomizePassword()
		{
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			byte[] Bin = new byte[28];
			Rnd.GetBytes(Bin);
			string Password = Convert.ToBase64String(Bin);

			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.XmppPassword.Password = Password;
				MainWindow.currentInstance.NetworkTab.XmppPassword2.Password = Password;

				return Task.CompletedTask;
			});

			return Task.CompletedTask;
		}

		private bool CanExecuteConnect()
		{
			return this.client is null && (!this.CreateAccount || this.Password == this.Password2);
		}

		/// <summary>
		/// Connects to the network
		/// </summary>
		public async Task ExecuteConnect()
		{
			try
			{
				string Host;
				int Port;

				MainWindow.MouseHourglass();

				try
				{
					SRV Record = await DnsResolver.LookupServiceEndpoint(this.XmppServer, "xmpp-client", "tcp");

					Host = Record.TargetHost;
					Port = Record.Port;
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					Host = this.XmppServer;
					Port = 5222;    // Default XMPP Client-to-Server port.
				}

				if (this.legalModel is not null)
				{
					await this.legalModel.Stop();
					this.legalModel.Dispose();
					this.legalModel = null;
				}

				if (this.walletModel is not null)
				{
					await this.walletModel.Stop();
					this.walletModel.Dispose();
					this.walletModel = null;
				}

				ListViewSniffer Sniffer = new(MainWindow.currentInstance.NetworkTab.SnifferListView, 1000);
				Sniffer.SelectionChanged += this.Sniffer_SelectionChanged;

				if (string.IsNullOrEmpty(this.PasswordMethod))
					this.client = new XmppClient(Host, Port, this.Account, this.Password, "en", typeof(MainWindow).Assembly, Sniffer);
				else
					this.client = new XmppClient(Host, Port, this.Account, this.Password, this.PasswordMethod, "en", typeof(MainWindow).Assembly, Sniffer);

				if (this.CreateAccount)
					this.client.AllowRegistration(this.ApiKey, this.ApiKeySecret);

				this.client.TrustServer = this.TrustServerCertificate;
				this.client.AllowEncryption = true;

				if (!this.AllowInsecureAlgorithms)
				{
					this.client.AllowCramMD5 = false;
					this.client.AllowDigestMD5 = false;
					this.client.AllowPlain = false;
					this.client.AllowScramSHA1 = false;
				}

				this.client.OnStateChanged += this.Client_OnStateChanged;
				this.client.OnConnectionError += this.Client_OnConnectionError;
				this.client.OnChatMessage += this.Client_OnChatMessage;

				await this.client.Connect(this.XmppServer);
			}
			catch (Exception ex)
			{
				MainWindow.MouseDefault();
				MainWindow.ErrorBox("Unable to connect to the XMPP network. Error reported: " + ex.Message);
			}
		}

		private Task Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			MainWindow.MessageBox(e.Body, "Message from " + e.From, MessageBoxButton.OK, MessageBoxImage.Information);
			return Task.CompletedTask;
		}

		private async Task Client_OnConnectionError(object Sender, Exception Exception)
		{
			if (this.CreateAccount || !this.ConnectOnStartup)
			{
				if (this.client is not null)
				{
					await this.client.DisposeAsync();
					this.client = null;
				}
			}

			this.connect.RaiseCanExecuteChanged();

			MainWindow.ErrorBox(Exception.Message);
		}

		private async Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			try
			{
				this.State = NewState;

				switch (NewState)
				{
					case XmppState.Connected:
						this.Connected = true;

						MainWindow.MouseDefault();
						this.client.OnConnectionError -= this.Client_OnConnectionError;

						this.ConnectOnStartup = true;
						this.CreateAccount = false;
						this.ApiKey = string.Empty;
						this.ApiKeySecret = string.Empty;

						if (string.IsNullOrEmpty(this.PasswordMethod) && !this.StorePasswordInsteadOfDigest)
						{
							this.Password = this.client.PasswordHash;
							this.PasswordMethod = this.client.PasswordHashMethod;
						}

						await this.Save();

						if (this.legalModel is null || this.legalModel.Contracts.ComponentAddress != this.LegalComponentJid || 
							this.walletModel is null || this.walletModel.EDaler.ComponentAddress != this.EDalerComponentJid ||
							this.tokensModel is null || this.tokensModel.NeuroFeaturesClient.ComponentAddress != this.NeuroFeaturesComponentJid ||
							this.legalModel.FileUpload.FileUploadJid != this.HttpFileUploadComponentJid)
						{
							if (string.IsNullOrEmpty(this.LegalComponentJid) ||
								string.IsNullOrEmpty(this.EDalerComponentJid) ||
								string.IsNullOrEmpty(this.NeuroFeaturesComponentJid) ||
								string.IsNullOrEmpty(this.HttpFileUploadComponentJid))
							{
								ServiceItemsDiscoveryEventArgs e = await this.client.ServiceItemsDiscoveryAsync(string.Empty);
								if (e.Ok)
								{
									foreach (Item Component in e.Items)
									{
										ServiceDiscoveryEventArgs e2 = await this.client.ServiceDiscoveryAsync(Component.JID);

										if (e2.HasAnyFeature(ContractsClient.NamespacesLegalIdentities) &&
											e2.HasAnyFeature(ContractsClient.NamespacesSmartContracts))
										{
											this.LegalComponentJid = Component.JID;
										}

										if (e2.HasFeature(EDalerClient.NamespaceEDaler))
											this.EDalerComponentJid = Component.JID;

										if (e2.HasFeature(NeuroFeaturesClient.NamespaceNeuroFeatures))
											this.NeuroFeaturesComponentJid = Component.JID;

										if (e2.HasFeature(HttpFileUploadClient.Namespace))
										{
											this.HttpFileUploadComponentJid = Component.JID;
											this.HttpFileUploadMaxSize = HttpFileUploadClient.FindMaxFileSize(this.client, e2) ?? 0;
										}
									}
								}
							}

							if (!string.IsNullOrEmpty(this.LegalComponentJid))
							{
								if (this.legalModel is null || this.legalModel.Contracts.ComponentAddress != this.LegalComponentJid)
								{
									this.legalModel?.Dispose();
									this.legalModel = null;

									this.legalModel = new LegalModel(this.client, this.LegalComponentJid, this.HttpFileUploadComponentJid,
										this.HttpFileUploadMaxSize <= 0 ? null : this.HttpFileUploadMaxSize);

									await this.legalModel.Load();
									await this.legalModel.Start();
								}

								if (!string.IsNullOrEmpty(this.EDalerComponentJid) && 
									(this.walletModel is null || this.walletModel.EDaler.ComponentAddress != this.EDalerComponentJid))
								{
									this.walletModel?.Dispose();
									this.walletModel = null;

									this.walletModel = new WalletModel(this.client, this.legalModel.Contracts, this.EDalerComponentJid, this);
									await this.walletModel.Load();
									await this.walletModel.Start();
								}

								if (!string.IsNullOrEmpty(this.NeuroFeaturesComponentJid) && 
									(this.tokensModel is null || this.tokensModel.NeuroFeaturesClient.ComponentAddress != this.NeuroFeaturesComponentJid))
								{
									this.tokensModel?.Dispose();
									this.tokensModel = null;

									this.tokensModel = new TokensModel(this.client, this.legalModel.Contracts, this.NeuroFeaturesComponentJid);
									await this.tokensModel.Load();
									await this.tokensModel.Start();
								}
							}
						}
						break;

					case XmppState.Error:
					case XmppState.Offline:
						this.Connected = false;
						break;
				}

				await this.OnStateChanged.Raise(this, NewState);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Event raised when connection state changes.
		/// </summary>
		public event EventHandlerAsync<XmppState> OnStateChanged;

		private bool CanExecuteDisconnect()
		{
			return this.client is not null;
		}

		/// <summary>
		/// Disconnects from the network
		/// </summary>
		public async Task ExecuteDisconnect()
		{
			this.LegalComponentJid = string.Empty;
			this.EDalerComponentJid = string.Empty;
			this.NeuroFeaturesComponentJid = string.Empty;

			this.legalModel?.Dispose();
			this.legalModel = null;

			if (this.client is not null)
			{
				await this.client.DisposeAsync();
				this.client = null;
			}

			this.State = XmppState.Offline;
			this.Connected = false;
			this.ConnectOnStartup = false;
		}

		/// <summary>
		/// Copy Sniffer Item Command
		/// </summary>
		public ICommand CopySnifferItem => this.copySnifferItem;

		private bool CanExecuteCopy(object Item) => SelectedItem(Item) is SniffItem SniffItem && SniffItem.IsSelected;

		private static object SelectedItem(object Item)
		{
			return Item ?? MainWindow.currentInstance.NetworkTab.SnifferListView.SelectedItem;
		}

		private void ExecuteCopy(object Item)
		{
			if (SelectedItem(Item) is SniffItem SniffItem)
			{
				StringBuilder Output = new();

				Output.Append("Date:\t");
				Output.AppendLine(SniffItem.Timestamp.Date.ToShortDateString());

				Output.Append("Time:\t");
				Output.AppendLine(SniffItem.Timestamp.ToLongTimeString());

				Output.Append("Type:\t");
				Output.AppendLine(SniffItem.Type.ToString());

				Output.AppendLine();

				try
				{
					(string PrettyXml, XmlElement _) = SniffItem.Message.ToPrettyXml();
					Output.AppendLine(PrettyXml);
				}
				catch (Exception)
				{
					Output.AppendLine(SniffItem.Message);
				}

				Clipboard.SetText(Output.ToString());
			}
		}

		/// <summary>
		/// Remove Sniffer Item Command
		/// </summary>
		public ICommand RemoveSnifferItem => this.removeSnifferItem;

		private bool CanExecuteRemove(object Item) => SelectedItem(Item) is SniffItem SniffItem && SniffItem.IsSelected;

		private void ExecuteRemove(object Item)
		{
			if (SelectedItem(Item) is SniffItem SniffItem)
				MainWindow.currentInstance.NetworkTab.SnifferListView.Items.Remove(SniffItem);
		}

		private void Sniffer_SelectionChanged(object sender, EventArgs e)
		{
			this.copySnifferItem?.RaiseCanExecuteChanged();
			this.removeSnifferItem?.RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Clear Sniffer Command
		/// </summary>
		public ICommand ClearSniffer => this.clearSniffer;

		private bool CanExecuteClearAll() => true;

		private Task ExecuteClearAll()
		{
			MainWindow.currentInstance.NetworkTab.SnifferListView.Items.Clear();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Save Credentials command
		/// </summary>
		public ICommand SaveCredentials => this.saveCredentials;

		private bool CanExecuteSaveCredentials() => this.Connected;

		private async Task ExecuteSaveCredentials()
		{
			SortedDictionary<string, bool> Sorted = [];

			foreach (string Account in this.SavedAccounts)
				Sorted[Account] = true;

			string Jid = this.Account + "@" + this.XmppServer;
			Sorted[Jid] = true;

			string Prefix = "Credentials." + Jid + ".";

			await RuntimeSettings.SetAsync(Prefix + "Password", this.Password);
			await RuntimeSettings.SetAsync(Prefix + "PasswordMethod", this.PasswordMethod);
			await RuntimeSettings.SetAsync(Prefix + "ApiKey", this.ApiKey);
			await RuntimeSettings.SetAsync(Prefix + "ApiKeySecret", this.ApiKeySecret);
			await RuntimeSettings.SetAsync(Prefix + "LegalComponentJid", this.LegalComponentJid);
			await RuntimeSettings.SetAsync(Prefix + "EDalerComponentJid", this.EDalerComponentJid);
			await RuntimeSettings.SetAsync(Prefix + "NeuroFeaturesComponentJid", this.NeuroFeaturesComponentJid);
			await RuntimeSettings.SetAsync(Prefix + "HttpFileUploadComponentJid", this.HttpFileUploadComponentJid);
			await RuntimeSettings.SetAsync(Prefix + "HttpFileUploadMaxSize", this.HttpFileUploadMaxSize);
			await RuntimeSettings.SetAsync(Prefix + "TrustServerCertificate", this.TrustServerCertificate);
			await RuntimeSettings.SetAsync(Prefix + "AllowInsecureAlgorithms", this.AllowInsecureAlgorithms);
			await RuntimeSettings.SetAsync(Prefix + "StorePasswordInsteadOfDigest", this.StorePasswordInsteadOfDigest);
			await RuntimeSettings.SetAsync(Prefix + "ConnectOnStartup", this.ConnectOnStartup);

			string[] Accounts = new string[Sorted.Count];
			Sorted.Keys.CopyTo(Accounts, 0);

			this.loading = true;
			try
			{
				this.SavedAccounts = Accounts;
				this.SelectedAccount = Jid;
			}
			finally
			{
				this.loading = false;
			}

			await MainWindow.MessageBox("Credentials saved.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// New ACcount command
		/// </summary>
		public ICommand NewAccount => this.newAccount;

		private async Task ExecuteNewAccount()
		{
			await this.ExecuteDisconnect();

			this.SelectedAccount = string.Empty;
			this.XmppServer = string.Empty;
			this.Account = string.Empty;
			this.Password = string.Empty;
			this.PasswordMethod = string.Empty;
			this.ApiKey = string.Empty;
			this.ApiKeySecret = string.Empty;
			this.LegalComponentJid = string.Empty;
			this.EDalerComponentJid = string.Empty;
			this.NeuroFeaturesComponentJid = string.Empty;
			this.EDalerComponentJid = string.Empty;
			this.TrustServerCertificate = false;
			this.AllowInsecureAlgorithms = false;
			this.StorePasswordInsteadOfDigest = false;
			this.ConnectOnStartup = false;
			this.CreateAccount = false;
			this.Password2 = string.Empty;

			await this.ExecuteClearAll();
		}

		/// <summary>
		/// Delete Credentials command
		/// </summary>
		public ICommand DeleteCredentials => this.deleteCredentials;

		private bool CanExecuteDeleteCredentials() => !string.IsNullOrEmpty(this.SelectedAccount);

		private async Task ExecuteDeleteCredentials()
		{
			SortedDictionary<string, bool> Sorted = [];

			foreach (string Account in this.SavedAccounts)
			{
				if (Account != this.SelectedAccount)
					Sorted[Account] = true;
			}

			string Jid = this.Account + "@" + this.XmppServer;
			string Prefix = "Credentials." + Jid + ".";

			await RuntimeSettings.DeleteAsync(Prefix + "Password");
			await RuntimeSettings.DeleteAsync(Prefix + "PasswordMethod");
			await RuntimeSettings.DeleteAsync(Prefix + "ApiKey");
			await RuntimeSettings.DeleteAsync(Prefix + "ApiKeySecret");
			await RuntimeSettings.DeleteAsync(Prefix + "LegalComponentJid");
			await RuntimeSettings.DeleteAsync(Prefix + "EDalerComponentJid");
			await RuntimeSettings.DeleteAsync(Prefix + "NeuroFeaturesComponentJid");
			await RuntimeSettings.DeleteAsync(Prefix + "EDalerComponentJid");
			await RuntimeSettings.DeleteAsync(Prefix + "TrustServerCertificate");
			await RuntimeSettings.DeleteAsync(Prefix + "AllowInsecureAlgorithms");
			await RuntimeSettings.DeleteAsync(Prefix + "StorePasswordInsteadOfDigest");
			await RuntimeSettings.DeleteAsync(Prefix + "ConnectOnStartup");

			string[] Accounts = new string[Sorted.Count];
			Sorted.Keys.CopyTo(Accounts, 0);

			this.loading = true;
			try
			{
				this.SelectedAccount = string.Empty;
				this.SavedAccounts = Accounts;
			}
			finally
			{
				this.loading = false;
			}

			await MainWindow.MessageBox("Credentials deleted.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
		}

	}
}
