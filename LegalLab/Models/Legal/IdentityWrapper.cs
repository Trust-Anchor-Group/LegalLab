using LegalLab.Models.Items;
using System;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Wrapper around a <see cref="LegalIdentity"/>, for purposes of displaying it to the user.
	/// </summary>
	public class IdentityWrapper : SelectableItem
	{
		private readonly string domain;
		private readonly LegalIdentity identity;

		/// <summary>
		/// Wrapper around a <see cref="LegalIdentity"/>, for purposes of displaying it to the user.
		/// </summary>
		///	<param name="Domain">Domain of XMPP Broker.</param>
		/// <param name="Identity">Legal Identity object.</param>
		public IdentityWrapper(string Domain, LegalIdentity Identity)
		{
			this.domain = Domain;
			this.identity = Identity;
		}

		/// <summary>
		/// Legal Identity ID
		/// </summary>
		public string Id => this.identity.Id;

		/// <summary>
		/// When object was created
		/// </summary>
		public DateTime Created => this.identity.Created;

		/// <summary>
		/// Current state of object
		/// </summary>
		public IdentityState State => this.identity.State;

		/// <summary>
		/// URL to Legal Identity
		/// </summary>
		public string URL => ContractsClient.LegalIdUriString(this.identity.Id);

		/// <summary>
		/// URL for displaying a QR-code for the <see cref="URL"/>.
		/// </summary>
		public string QrCodeUrl
		{
			get
			{
				if (string.IsNullOrEmpty(this.domain))
					return "https://lab.tagroot.io/QR/" + this.URL;
				else
					return "https://" + this.domain + "/QR/" + this.URL;
			}
		}
	}
}
