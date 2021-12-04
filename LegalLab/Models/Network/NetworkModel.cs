using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Network
{
	/// <summary>
	/// Network Model
	/// </summary>
	[Singleton]
	public class NetworkModel : PersistedModel
	{
		private readonly PersistedProperty<string> xmppServer = new PersistedProperty<string>("XMPP", "Server", false, string.Empty);
		private readonly PersistedProperty<string> account = new PersistedProperty<string>("XMPP", "Account", false, string.Empty);
		private readonly PersistedProperty<string> password = new PersistedProperty<string>("XMPP", "Password", false, string.Empty);
		private readonly PersistedProperty<string> passwordMethod = new PersistedProperty<string>("XMPP", "PasswordMethod", false, string.Empty);
		private readonly PersistedProperty<string> apiKey = new PersistedProperty<string>("XMPP", "ApiKey", false, string.Empty);
		private readonly PersistedProperty<string> apiKeySecret = new PersistedProperty<string>("XMPP", "ApiKeySecret", false, string.Empty);
		private readonly PersistedProperty<bool> createAccount = new PersistedProperty<bool>("XMPP", "CreateAccount", false, false);
		private readonly PersistedProperty<bool> trustServerCertificate = new PersistedProperty<bool>("XMPP", "TrustServer", false, false);
		private readonly PersistedProperty<bool> allowInsecureAlgorithms = new PersistedProperty<bool>("XMPP", "InsecureAuthentication", false, false);
		private readonly PersistedProperty<bool> allowUnencryptedCommunication = new PersistedProperty<bool>("XMPP", "AllowUnencrypted", false, false);
		private readonly PersistedProperty<bool> storePasswordInsteadOfDigest = new PersistedProperty<bool>("XMPP", "StorePassword", false, false);

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
			this.Add(this.allowUnencryptedCommunication = new PersistedProperty<bool>("XMPP", "AllowUnencrypted", false, false));
			this.Add(this.storePasswordInsteadOfDigest = new PersistedProperty<bool>("XMPP", "StorePassword", false, false));
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
		/// If unencrypted communication is allowed.
		/// </summary>
		public bool AllowUnencryptedCommunication
		{
			get => this.allowUnencryptedCommunication.Value;
			set => this.allowUnencryptedCommunication.Value = value;
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
		/// Starts the model.
		/// </summary>
		public override Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.NetworkTab.DataContext = this;

				MainWindow.currentInstance.XmppPassword.Password = this.Password;
				MainWindow.currentInstance.ApiKeySecret.Password = this.ApiKeySecret;
			});

			return base.Start();
		}
	}
}
