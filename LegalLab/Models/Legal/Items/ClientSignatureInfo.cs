using System;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal.Items
{
    /// <summary>
    /// Contains information about a client signature
    /// </summary>
    public class ClientSignatureInfo : Model
    {
        private readonly Property<string> bareJid;
        private readonly Property<string> legalId;
        private readonly Property<string> legalIdUri;
        private readonly Property<string> legalIdQrCodeUri;
        private readonly Property<string> role;
        private readonly Property<byte[]> signature;
        private readonly Property<DateTime> timestamp;
        private readonly Property<bool> transferable;

        /// <summary>
        /// Contains information about a client signature
        /// </summary>
        /// <param name="Contracts">Contracts client.</param>
        /// <param name="ClientSignature">Client Signature</param>
        public ClientSignatureInfo(ContractsClient Contracts, ClientSignature ClientSignature)
		{
			string Domain = Contracts.Client.Domain;
			if (string.IsNullOrEmpty(Domain))
				Domain = "lab.tagroot.io";

            this.bareJid = new Property<string>(nameof(this.BareJid), ClientSignature.BareJid, this);
            this.signature = new Property<byte[]>(nameof(this.Signature), ClientSignature.DigitalSignature, this);
            this.legalId = new Property<string>(nameof(this.LegalId), ClientSignature.LegalId, this);
            this.legalIdUri = new Property<string>(nameof(this.LegalIdUri), ClientSignature.LegalIdUriString, this);
            this.role = new Property<string>(nameof(this.Role), ClientSignature.Role, this);
            this.timestamp = new Property<DateTime>(nameof(this.Timestamp), ClientSignature.Timestamp, this);
            this.transferable = new Property<bool>(nameof(this.Transferable), ClientSignature.Transferable, this);
            this.legalIdQrCodeUri = new Property<string>(nameof(this.LegalIdQrCodeUri), "https://" + Domain + "/QR/" + ClientSignature.LegalIdUriString, this);
        }

        /// <summary>
        /// Bare JID
        /// </summary>
        public string BareJid
		{
            get => this.bareJid.Value;
            set => this.bareJid.Value = value;
		}

        /// <summary>
        /// Legal ID
        /// </summary>
        public string LegalId
        {
            get => this.legalId.Value;
            set => this.legalId.Value = value;
        }

        /// <summary>
        /// Legal ID URI
        /// </summary>
        public string LegalIdUri
        {
            get => this.legalIdUri.Value;
            set => this.legalIdUri.Value = value;
        }

        /// <summary>
        /// URI for QR Code encoding Legal ID URI
        /// </summary>
        public string LegalIdQrCodeUri
        {
            get => this.legalIdQrCodeUri.Value;
            set => this.legalIdQrCodeUri.Value = value;
        }

        /// <summary>
        /// Role
        /// </summary>
        public string Role
        {
            get => this.role.Value;
            set => this.role.Value = value;
        }

        /// <summary>
        /// Digital Signature
        /// </summary>
        public byte[] Signature
        {
            get => this.signature.Value;
            set => this.signature.Value = value;
        }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp
        {
            get => this.timestamp.Value;
            set => this.timestamp.Value = value;
        }

        /// <summary>
        /// If role is transferable
        /// </summary>
        public bool Transferable
        {
            get => this.transferable.Value;
            set => this.transferable.Value = value;
        }

    }
}
