using EDaler;
using LegalLab.Models.Items;
using System;
using Waher.Persistence;

namespace LegalLab.Models.Wallet
{
	/// <summary>
	/// Account event wrapper.
	/// </summary>
	public class AccountEventWrapper : SelectableItem
	{
		private readonly AccountEvent @event;

		/// <summary>
		/// Account event wrapper.
		/// </summary>
		/// <param name="Event">Account event</param>
		public AccountEventWrapper(AccountEvent Event)
		{
			this.@event = Event;
		}

		/// <summary>
		/// Transaction ID
		/// </summary>
		public Guid TransactionId => this.@event.TransactionId;

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp => this.@event.Timestamp;

		/// <summary>
		/// Remote Address
		/// </summary>
		public CaseInsensitiveString Remote => this.@event.Remote;

		/// <summary>
		/// Change
		/// </summary>
		public decimal Change => this.@event.Change;

		/// <summary>
		/// Balance
		/// </summary>
		public decimal Balance => this.@event.Balance;

		/// <summary>
		/// Reserved amount
		/// </summary>
		public decimal Reserved => this.@event.Reserved;

		/// <summary>
		/// Encrypted message
		/// </summary>
		public byte[] EncryptedMessage => this.@event.EncryptedMessage;

		/// <summary>
		/// Encryption public key
		/// </summary>
		public byte[] EncryptionPublicKey => this.@event.EncryptionPublicKey;

		/// <summary>
		/// Message
		/// </summary>
		public string Message => this.@event.Message;
	}
}
