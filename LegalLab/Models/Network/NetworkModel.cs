using System;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Network
{
	/// <summary>
	/// Network Model
	/// </summary>
	[Singleton]
	public class NetworkModel : PersistantModel
	{
		private readonly PersistantProperty<string> xmppServer = new PersistantProperty<string>("XMPP", "Server", false, string.Empty);
		private readonly PersistantProperty<string> account = new PersistantProperty<string>("XMPP", "Account", false, string.Empty);
		private readonly PersistantProperty<string> password = new PersistantProperty<string>("XMPP", "Password", false, string.Empty);
		private readonly PersistantProperty<string> passwordMethod = new PersistantProperty<string>("XMPP", "PasswordMethod", false, string.Empty);
		private readonly PersistantProperty<string> apiKey = new PersistantProperty<string>("XMPP", "ApiKey", false, string.Empty);
		private readonly PersistantProperty<string> apiKeySecret = new PersistantProperty<string>("XMPP", "ApiKeySecret", false, string.Empty);
		private readonly PersistantProperty<bool> createAccount = new PersistantProperty<bool>("XMPP", "CreateAccount", false, false);
		private readonly PersistantProperty<bool> trustServerCertificate = new PersistantProperty<bool>("XMPP", "TrustServer", false, false);
		private readonly PersistantProperty<bool> allowInsecureAlgorithms = new PersistantProperty<bool>("XMPP", "InsecureAuthentication", false, false);
		private readonly PersistantProperty<bool> allowUnencryptedCommunication = new PersistantProperty<bool>("XMPP", "AllowUnencrypted", false, false);
		private readonly PersistantProperty<bool> storePasswordInsteadOfDigest = new PersistantProperty<bool>("XMPP", "StorePassword", false, false);

		public NetworkModel()
			: base()
		{
			this.Add(this.xmppServer = new PersistantProperty<string>("XMPP", "Server", false, string.Empty));
			this.Add(this.account = new PersistantProperty<string>("XMPP", "Account", false, string.Empty));
			this.Add(this.password = new PersistantProperty<string>("XMPP", "Password", false, string.Empty));
			this.Add(this.passwordMethod = new PersistantProperty<string>("XMPP", "PasswordMethod", false, string.Empty));
			this.Add(this.apiKey = new PersistantProperty<string>("XMPP", "ApiKey", false, string.Empty));
			this.Add(this.apiKeySecret = new PersistantProperty<string>("XMPP", "ApiKeySecret", false, string.Empty));
			this.Add(this.createAccount = new PersistantProperty<bool>("XMPP", "CreateAccount", false, false));
			this.Add(this.trustServerCertificate = new PersistantProperty<bool>("XMPP", "TrustServer", false, false));
			this.Add(this.allowInsecureAlgorithms = new PersistantProperty<bool>("XMPP", "InsecureAuthentication", false, false));
			this.Add(this.allowUnencryptedCommunication = new PersistantProperty<bool>("XMPP", "AllowUnencrypted", false, false));
			this.Add(this.storePasswordInsteadOfDigest = new PersistantProperty<bool>("XMPP", "StorePassword", false, false));
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
	}
}
