using LegalLabMaui.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Command = LegalLabMaui.Models.Command;

namespace LegalLabMaui.Views.Dialogs
{
	/// <summary>
	/// View model for the Transfer eDaler® dialog.
	/// </summary>
	public class TransferEDalerModel : Model
	{
		private readonly Property<string> recipient;
		private readonly Property<decimal> amount;
		private readonly Property<decimal> amountExtra;
		private readonly Property<string> currency;
		private readonly Property<int> validNrDays;
		private readonly Property<string> message;
		private readonly Command transfer;
		private readonly Command cancel;

		/// <summary>
		/// Completion source resolved when the user confirms or cancels.
		/// True = transfer confirmed, False = cancelled.
		/// </summary>
		private System.Threading.Tasks.TaskCompletionSource<bool> tcs = new();

		public TransferEDalerModel(string DefaultCurrency)
		{
			this.recipient = new Property<string>(nameof(this.Recipient), string.Empty, this);
			this.amount = new Property<decimal>(nameof(this.Amount), 0M, this);
			this.amountExtra = new Property<decimal>(nameof(this.AmountExtra), 0M, this);
			this.currency = new Property<string>(nameof(this.Currency), DefaultCurrency, this);
			this.validNrDays = new Property<int>(nameof(this.ValidNrDays), 3, this);
			this.message = new Property<string>(nameof(this.Message), string.Empty, this);

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
		/// Amount extra (e.g. tip or fee)
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
		/// Valid number of days.
		/// </summary>
		public int ValidNrDays
		{
			get => this.validNrDays.Value;
			set
			{
				this.validNrDays.Value = value;
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
				this.ValidNrDays > 0 &&
				this.Currency.Length == 3 &&
				this.Currency.Equals(this.Currency, System.StringComparison.CurrentCultureIgnoreCase);
		}

		private Task ExecuteTransfer()
		{
			this.tcs.TrySetResult(true);
			return Task.CompletedTask;
		}

		private Task ExecuteCancel()
		{
			this.tcs.TrySetResult(false);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits for the user to confirm or cancel the transfer.
		/// Returns true if Transfer was confirmed, false if cancelled.
		/// </summary>
		public Task<bool> WaitForResult() => this.tcs.Task;

		/// <summary>
		/// Resets the completion source so the model can be reused.
		/// </summary>
		public void Reset() => this.tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
	}
}
