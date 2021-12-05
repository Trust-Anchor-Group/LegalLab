using System;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Legal
{
    /// <summary>
    /// Contains information about a client signature
    /// </summary>
    public class ServerSignatureInfo : Model
    {
        private readonly Property<byte[]> signature;
        private readonly Property<DateTime> timestamp;

        /// <summary>
        /// Contains information about a client signature
        /// </summary>
        /// <param name="ServerSignature">Server Signature</param>
        public ServerSignatureInfo(ServerSignature ServerSignature)
		{
            this.signature = new Property<byte[]>(nameof(this.Signature), ServerSignature.DigitalSignature, this);
            this.timestamp = new Property<DateTime>(nameof(this.Timestamp), ServerSignature.Timestamp, this);
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
    }
}
