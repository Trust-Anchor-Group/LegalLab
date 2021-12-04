using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Waher.Events;
using Waher.Networking.DNS;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
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
		private readonly PersistedProperty<bool> createAccount;
		private readonly PersistedProperty<bool> trustServerCertificate;
		private readonly PersistedProperty<bool> allowInsecureAlgorithms;
		private readonly PersistedProperty<bool> storePasswordInsteadOfDigest;
		private readonly PersistedProperty<bool> connectOnStartup;

		private string password2 = string.Empty;
		private bool connecting = false;

		private readonly Command connect;
		private readonly Command randomizePassword;

		private XmppClient client;
		private ContractsClient contracts;

		public NetworkModel()
			: base()
		{
			this.Add(this.xmppServer = new PersistedProperty<string>("XMPP", "Server", false, string.Empty));
			this.Add(this.account = new PersistedProperty<string>("XMPP", "Account", false, string.Empty));
			this.Add(this.password = new PersistedProperty<string>("XMPP", "Password", false, string.Empty));
			this.Add(this.passwordMethod = new PersistedProperty<string>("XMPP", "PasswordMethod", false, string.Empty));
			this.Add(this.apiKey = new PersistedProperty<string>("XMPP", "ApiKey", false, string.Empty));
			this.Add(this.apiKeySecret = new PersistedProperty<string>("XMPP", "ApiKeySecret", false, string.Empty));
			this.Add(this.createAccount = new PersistedProperty<bool>("XMPP", "CreateAccount", false, false));
			this.Add(this.trustServerCertificate = new PersistedProperty<bool>("XMPP", "TrustServer", false, false));
			this.Add(this.allowInsecureAlgorithms = new PersistedProperty<bool>("XMPP", "InsecureAuthentication", false, false));
			this.Add(this.storePasswordInsteadOfDigest = new PersistedProperty<bool>("XMPP", "StorePassword", false, false));
			this.Add(this.connectOnStartup = new PersistedProperty<bool>("XMPP", "ConnectOnStartup", false, false));

			this.connect = new Command(this.CanExecuteConnect, this.ExecuteConnect);
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
			set => this.createAccount.Value = value;
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
		/// Connection command
		/// </summary>
		public ICommand Connect => this.connect;

		/// <summary>
		/// Password randomization command
		/// </summary>
		public ICommand RandomizePassword => this.randomizePassword;

		/// <summary>
		/// If a connection procedure is underway
		/// </summary>
		public bool Connecting
		{ 
			get => this.connecting;
			set
			{
				if (this.connecting != value)
				{
					this.connecting = value;
					this.connect.RaiseCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Second entry of password
		/// </summary>
		public string Password2
		{
			get => this.password2;
			set
			{
				if (this.password2 != value)
				{
					this.password2 = value;
					this.connect.RaiseCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.DataContext = this;

				MainWindow.currentInstance.XmppPassword.Password = this.Password;
				MainWindow.currentInstance.ApiKeySecret.Password = this.ApiKeySecret;
			});

			if (this.ConnectOnStartup)
				this.ExecuteConnect();

			await base.Start();
		}

		public void ExecuteRandomizePassword()
		{
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			byte[] Bin = new byte[28];
			Rnd.GetBytes(Bin);
			string Password = Convert.ToBase64String(Bin);

			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.XmppPassword.Password = Password;
				MainWindow.currentInstance.XmppPassword2.Password = Password;
			});
		}

		private bool CanExecuteConnect()
		{
			return !this.Connecting && this.password.Value == this.password2;
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

				this.Connecting = true;

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

				if (string.IsNullOrEmpty(this.PasswordMethod))
					this.client = new XmppClient(Host, Port, this.Account, this.Password, "en", typeof(MainWindow).Assembly);
				else
					this.client = new XmppClient(Host, Port, this.Account, this.Password, this.PasswordMethod, "en", typeof(MainWindow).Assembly);

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
				MainWindow.ErrorBox("Unable to connect to the XMPP network. Error reported: " + ex.Message);
				this.Connecting = false;
			}
		}

		private Task Client_OnConnectionError(object Sender, Exception Exception)
		{
			MainWindow.ErrorBox(Exception.Message);
			return Task.CompletedTask;
		}

		private async Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			MainWindow.UpdateGui(() => MainWindow.currentInstance.XmppState.Content = NewState.ToString());

			switch (NewState)
			{
				case XmppState.Connected:
					this.client.OnConnectionError -= Client_OnConnectionError;

					this.Connecting = false;
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
					break;

				case XmppState.Error:
				case XmppState.Offline:
					this.Connecting = false;
					break;
			}
		}
	}
}
