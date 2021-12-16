using EDaler;
using LegalLab.Models.Legal;
using LegalLab.Models.Network.Sniffer;
using LegalLab.Models.Wallet;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Events;
using Waher.Networking.DNS;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Network
{
	/// <summary>
	/// Network Model
	/// </summary>
	[Singleton]
	public class NetworkModel : PersistedModel
	{
		private readonly PersistedProperty<string> xmppServer;
		private readonly PersistedProperty<string> account;
		private readonly PersistedProperty<string> password;
		private readonly PersistedProperty<string> passwordMethod;
		private readonly PersistedProperty<string> apiKey;
		private readonly PersistedProperty<string> apiKeySecret;
		private readonly PersistedProperty<string> legalComponentJid;
		private readonly PersistedProperty<string> eDalerComponentJid;
		private readonly PersistedProperty<bool> createAccount;
		private readonly PersistedProperty<bool> trustServerCertificate;
		private readonly PersistedProperty<bool> allowInsecureAlgorithms;
		private readonly PersistedProperty<bool> storePasswordInsteadOfDigest;
		private readonly PersistedProperty<bool> connectOnStartup;
		private readonly Property<string> password2;
		private readonly Property<XmppState> state;

		private readonly Command connect;
		private readonly Command disconnect;
		private readonly Command randomizePassword;

		private XmppClient client;
		private LegalModel legalModel;
		private WalletModel walletModel;

		public NetworkModel()
			: base()
		{
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

			this.password2 = new Property<string>(nameof(this.Password2), string.Empty, this);
			this.state = new Property<XmppState>(nameof(this.State), XmppState.Offline, this);

			this.connect = new Command(this.CanExecuteConnect, this.ExecuteConnect);
			this.disconnect = new Command(this.CanExecuteDisconnect, this.ExecuteDisconnect);
			this.randomizePassword = new Command(this.ExecuteRandomizePassword);
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
		/// e-Daler Component JID
		/// </summary>
		public string EDalerComponentJid
		{
			get => this.eDalerComponentJid.Value;
			set => this.eDalerComponentJid.Value = value;
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
		/// Legal ID model
		/// </summary>
		public LegalModel Legal => this.legalModel;

		/// <summary>
		/// Wallet model
		/// </summary>
		public WalletModel Wallet => this.Wallet;

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.DataContext = this;
				MainWindow.currentInstance.XmppState.DataContext = this;

				MainWindow.currentInstance.NetworkTab.XmppPassword.Password = this.Password;
				MainWindow.currentInstance.NetworkTab.ApiKeySecret.Password = this.ApiKeySecret;

				MainWindow.currentInstance.NetworkTab.XmppPassword.PasswordChanged += this.PasswordChanged;
				MainWindow.currentInstance.NetworkTab.XmppPassword2.PasswordChanged += this.Password2Changed;
				MainWindow.currentInstance.NetworkTab.ApiKeySecret.PasswordChanged += this.ApiKeySecretChanged;
			});

			if (this.ConnectOnStartup)
				this.ExecuteConnect();

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

			if (!(this.legalModel is null))
			{
				await this.legalModel.Stop();
				this.legalModel.Dispose();
				this.legalModel = null;
			}

			if (!(this.walletModel is null))
			{
				await this.walletModel.Stop();
				this.walletModel.Dispose();
				this.walletModel = null;
			}

			this.client?.Dispose();
			this.client = null;

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

		public void ExecuteRandomizePassword()
		{
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			byte[] Bin = new byte[28];
			Rnd.GetBytes(Bin);
			string Password = Convert.ToBase64String(Bin);

			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.XmppPassword.Password = Password;
				MainWindow.currentInstance.NetworkTab.XmppPassword2.Password = Password;
			});
		}

		private bool CanExecuteConnect()
		{
			return this.client is null && (!this.CreateAccount || this.Password == this.Password2);
		}

		/// <summary>
		/// Connects to the network
		/// </summary>
		public async void ExecuteConnect()
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
					Log.Critical(ex);

					Host = this.XmppServer;
					Port = 5222;    // Default XMPP Client-to-Server port.
				}

				if (!(this.legalModel is null))
				{
					await this.legalModel.Stop();
					this.legalModel.Dispose();
					this.legalModel = null;
				}

				if (!(this.walletModel is null))
				{
					await this.walletModel.Stop();
					this.walletModel.Dispose();
					this.walletModel = null;
				}

				ListViewSniffer Sniffer = new ListViewSniffer(MainWindow.currentInstance.NetworkTab.SnifferListView, 1000);

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
					this.client.AllowDigestMD5 = false;
					this.client.AllowDigestMD5 = false;
					this.client.AllowPlain = false;
					this.client.AllowScramSHA1 = false;
				}

				this.client.OnStateChanged += Client_OnStateChanged;
				this.client.OnConnectionError += Client_OnConnectionError;

				this.client.Connect(this.XmppServer);
			}
			catch (Exception ex)
			{
				MainWindow.MouseDefault();
				MainWindow.ErrorBox("Unable to connect to the XMPP network. Error reported: " + ex.Message);
			}
		}

		private Task Client_OnConnectionError(object Sender, Exception Exception)
		{
			if (this.CreateAccount || !this.ConnectOnStartup)
			{
				this.client.Dispose();
				this.client = null;
			}

			this.connect.RaiseCanExecuteChanged();

			MainWindow.ErrorBox(Exception.Message);
			return Task.CompletedTask;
		}

		private async Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			try
			{
				this.State = NewState;

				switch (NewState)
				{
					case XmppState.Connected:
						MainWindow.MouseDefault();
						this.client.OnConnectionError -= Client_OnConnectionError;

						this.ConnectOnStartup = true;
						this.CreateAccount = false;
						this.ApiKey = string.Empty;
						this.ApiKeySecret = string.Empty;

						if (string.IsNullOrEmpty(PasswordMethod) && !this.StorePasswordInsteadOfDigest)
						{
							this.Password = this.client.PasswordHash;
							this.PasswordMethod = this.client.PasswordHashMethod;
						}

						await this.Save();

						if (this.legalModel is null || this.walletModel is null)
						{
							if (string.IsNullOrEmpty(this.LegalComponentJid) || string.IsNullOrEmpty(this.EDalerComponentJid))
							{
								ServiceItemsDiscoveryEventArgs e = await this.client.ServiceItemsDiscoveryAsync(string.Empty);
								if (e.Ok)
								{
									foreach (Item Component in e.Items)
									{
										ServiceDiscoveryEventArgs e2 = await this.client.ServiceDiscoveryAsync(Component.JID);

										if (e2.HasFeature(ContractsClient.NamespaceLegalIdentities) &&
											e2.HasFeature(ContractsClient.NamespaceSmartContracts))
										{
											this.LegalComponentJid = Component.JID;
										}
										else if (e2.HasFeature(EDalerClient.NamespaceEDaler))
											this.EDalerComponentJid = Component.JID;
									}
								}
							}

							if (!string.IsNullOrEmpty(this.LegalComponentJid))
							{
								if (this.legalModel is null)
								{
									this.legalModel = new LegalModel(this.client, this.LegalComponentJid);
									await this.legalModel.Load();
									await this.legalModel.Start();
								}

								if (!string.IsNullOrEmpty(this.EDalerComponentJid) && this.walletModel is null)
								{
									this.walletModel = new WalletModel(this.client, this.legalModel.Contracts, this.EDalerComponentJid);
									await this.walletModel.Load();
									await this.walletModel.Start();
								}
							}
						}
						break;

					case XmppState.Error:
					case XmppState.Offline:
						break;
				}

				StateChangedEventHandler h = this.OnStateChanged;
				if (!(h is null))
					await h(this, NewState);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when connection state changes.
		/// </summary>
		public event StateChangedEventHandler OnStateChanged;

		private bool CanExecuteDisconnect()
		{
			return !(this.client is null);
		}

		/// <summary>
		/// Disconnects from the network
		/// </summary>
		public void ExecuteDisconnect()
		{
			this.LegalComponentJid = string.Empty;
			this.EDalerComponentJid = string.Empty;

			this.legalModel?.Dispose();
			this.legalModel = null;

			this.client?.Dispose();
			this.client = null;

			this.State = XmppState.Offline;
			this.ConnectOnStartup = false;
		}

	}
}
