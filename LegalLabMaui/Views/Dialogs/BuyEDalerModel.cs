using LegalLabMaui.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Command = LegalLabMaui.Models.Command;

namespace LegalLabMaui.Views.Dialogs
{
	/// <summary>
	/// View model for the buy eDaler® dialog.
	/// </summary>
	public class BuyEDalerModel : Model
	{
		private readonly ServiceProviderModel[] providers;
		private readonly Property<ServiceProviderModel> selectedServiceProvider;
		private readonly Property<decimal> amount;
		private readonly Property<string> currency;
		private readonly Command buy;
		private readonly Command cancel;

		/// <summary>
		/// Completion source resolved when the user confirms or cancels.
		/// True = buy confirmed, False = cancelled.
		/// </summary>
		private System.Threading.Tasks.TaskCompletionSource<bool> tcs = new();

		public BuyEDalerModel(Waher.Networking.XMPP.Contracts.IServiceProvider[] Providers, string DefaultCurrency)
		{
			this.selectedServiceProvider = new Property<ServiceProviderModel>(nameof(this.SelectedServiceProvider), null, this);
			this.amount = new Property<decimal>(nameof(this.Amount), 0M, this);
			this.currency = new Property<string>(nameof(this.Currency), DefaultCurrency, this);

			int i, c = Providers.Length;
			this.providers = new ServiceProviderModel[c];

			for (i = 0; i < c; i++)
				this.providers[i] = new ServiceProviderModel(Providers[i]);

			this.buy = new Command(this.CanExecuteBuy, this.ExecuteBuy);
			this.cancel = new Command(this.ExecuteCancel);
		}

		/// <summary>
		/// Service Provider selected
		/// </summary>
		public ServiceProviderModel SelectedServiceProvider
		{
			get => this.selectedServiceProvider.Value;
			set
			{
				this.selectedServiceProvider.Value = value;
				this.buy.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Service Providers
		/// </summary>
		public ServiceProviderModel[] ServiceProviders => this.providers;

		/// <summary>
		/// Amount
		/// </summary>
		public decimal Amount
		{
			get => this.amount.Value;
			set
			{
				this.amount.Value = value;
				this.buy.RaiseCanExecuteChanged();
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
				this.buy.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Buy Command
		/// </summary>
		public ICommand Buy => this.buy;

		/// <summary>
		/// Cancel Command
		/// </summary>
		public ICommand Cancel => this.cancel;

		private bool CanExecuteBuy()
		{
			return
				this.SelectedServiceProvider is not null &&
				this.Amount > 0 &&
				this.Currency.Length == 3 &&
				this.Currency.Equals(this.Currency, System.StringComparison.CurrentCultureIgnoreCase);
		}

		private Task ExecuteBuy()
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
		/// Waits for the user to confirm or cancel the purchase.
		/// Returns true if Buy was confirmed, false if cancelled.
		/// </summary>
		public Task<bool> WaitForResult() => this.tcs.Task;

		/// <summary>
		/// Resets the completion source so the model can be reused.
		/// </summary>
		public void Reset() => this.tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
	}
}
