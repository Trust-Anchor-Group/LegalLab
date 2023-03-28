using LegalLab.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LegalLab.Dialogs.TransferEDaler
{
	/// <summary>
	/// View model for the Transfer eDaler dialog.
	/// </summary>
	public class TransferEDalerModel : Model
	{
		private readonly Property<string> recipient;
		private readonly Property<decimal> amount;
		private readonly Property<decimal> amountExtra;
		private readonly Property<string> currency;
		private readonly Property<string> message;
		private readonly TransferEDalerDialog dialog;
		private readonly Command transfer;
		private readonly Command cancel;

		public TransferEDalerModel(TransferEDalerDialog Dialog, string DefaultCurrency)
		{
			this.recipient = new Property<string>(nameof(this.Recipient), string.Empty, this);
			this.amount = new Property<decimal>(nameof(this.Amount), 0M, this);
			this.amountExtra = new Property<decimal>(nameof(this.AmountExtra), 0M, this);
			this.currency = new Property<string>(nameof(this.Currency), DefaultCurrency, this);
			this.message = new Property<string>(nameof(this.Message), string.Empty, this);

			this.dialog = Dialog;
			this.dialog.DataContext = this;

			this.transfer = new Command(this.CanExecuteTransfer, this.ExecuteTransfer);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// Recipient
		/// </summary>
		public string Recipient
		{
			get => this.recipient.Value;
			set
			{
				this.recipient.Value = value;
				this.transfer.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Amount
		/// </summary>
		public decimal Amount
		{
			get => this.amount.Value;
			set
			{
				this.amount.Value = value;
				this.transfer.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Amount
		/// </summary>
		public decimal AmountExtra
		{
			get => this.amountExtra.Value;
			set
			{
				this.amountExtra.Value = value;
				this.transfer.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Currency
		/// </summary>
		public string Currency
		{
			get => this.currency.Value;
			set
			{
				this.currency.Value = value;
				this.transfer.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Message
		/// </summary>
		public string Message
		{
			get => this.message.Value;
			set
			{
				this.message.Value = value;
				this.transfer.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Transfer Command
		/// </summary>
		public ICommand Transfer => this.transfer;

		/// <summary>
		/// Cancel Command
		/// </summary>
		public ICommand Cancel => this.cancel;

		private bool CanExecuteTransfer()
		{
			return
				!string.IsNullOrEmpty(this.Recipient) &&
				this.Amount > 0 &&
				this.AmountExtra >= 0 &&
				this.Currency.Length == 3 &&
				this.Currency == this.Currency.ToUpper();
		}

		private Task ExecuteTransfer()
		{
			this.dialog.DialogResult = true;
			return Task.CompletedTask;
		}

		private Task ExecuteCancel()
		{
			this.dialog.DialogResult = false;
			return Task.CompletedTask;
		}
	}
}
