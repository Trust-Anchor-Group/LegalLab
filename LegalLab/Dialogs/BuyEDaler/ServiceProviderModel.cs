using EDaler;

namespace LegalLab.Dialogs.BuyEDaler
{
	/// <summary>
	/// View mode for service providers.
	/// </summary>
	public class ServiceProviderModel
	{
		private readonly IServiceProvider serviceProvider;

		/// <summary>
		/// View mode for service providers.
		/// </summary>
		/// <param name="ServiceProvider">Service provider reference.</param>
		public ServiceProviderModel(IServiceProvider ServiceProvider)
		{
			this.serviceProvider = ServiceProvider;
		}

		/// <summary>
		/// Reference to service provider.
		/// </summary>
		public IServiceProvider ServiceProvider => this.serviceProvider;

		/// <summary>
		/// Name of service provider.
		/// </summary>
		public string Name => this.serviceProvider.Name;

		/// <summary>
		/// ID of service provider.
		/// </summary>
		public string Id => this.serviceProvider.Type + "|" + this.serviceProvider.Id;

		/// <summary>
		/// Icon URL
		/// </summary>
		public string IconUrl => this.serviceProvider.IconUrl;

		/// <summary>
		/// Scaling factor to apply for Icon.
		/// </summary>
		public double IconScale
		{
			get
			{
				return System.Math.Min(150.0 / this.serviceProvider.IconHeight, 1.0);
			}
		}

		/// <summary>
		/// Icon Width
		/// </summary>
		public int IconWidth => (int)(this.serviceProvider.IconWidth * this.IconScale + 0.5);

		/// <summary>
		/// Icon Height
		/// </summary>
		public int IconHeight => (int)(this.serviceProvider.IconHeight * this.IconScale + 0.5);

	}
}
