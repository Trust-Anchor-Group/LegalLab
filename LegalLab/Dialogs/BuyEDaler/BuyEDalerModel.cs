using EDaler;
using LegalLab.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LegalLab.Dialogs.BuyEDaler
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
		private readonly BuyEDalerDialog dialog;
		private readonly Command buy;
		private readonly Command cancel;

		public BuyEDalerModel(BuyEDalerDialog Dialog, IServiceProvider[] Providers, string DefaultCurrency)
		{
			this.selectedServiceProvider = new Property<ServiceProviderModel>(nameof(this.SelectedServiceProvider), null, this);
			this.amount = new Property<decimal>(nameof(this.Amount), 0M, this);
			this.currency = new Property<string>(nameof(this.Currency), DefaultCurrency, this);

			int i, c = Providers.Length;
			this.providers = new ServiceProviderModel[c];

			for (i = 0; i < c; i++)
				this.providers[i] = new ServiceProviderModel(Providers[i]);

			this.dialog = Dialog;
			this.dialog.DataContext = this;

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
				!(this.SelectedServiceProvider is null) &&
				this.Amount > 0 &&
				this.Currency.Length == 3 &&
				this.Currency.ToUpper() == this.Currency;
		}

		private Task ExecuteBuy()
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
