using LegalLab.Extensions;
using LegalLab.Models.Standards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace LegalLab.Models.Legal
{
	/// <summary>
	/// Legal ID Model
	/// </summary>
	[Singleton]
	public class LegalModel : PersistedModel, IDisposable
	{
		private readonly PersistedProperty<string> firstName;
		private readonly PersistedProperty<string> middleName;
		private readonly PersistedProperty<string> lastName;
		private readonly PersistedProperty<string> personalNumber;
		private readonly PersistedProperty<string> address;
		private readonly PersistedProperty<string> address2;
		private readonly PersistedProperty<string> zip;
		private readonly PersistedProperty<string> area;
		private readonly PersistedProperty<string> city;
		private readonly PersistedProperty<string> region;
		private readonly PersistedProperty<string> country;

		private readonly Property<Contract> template;
		private readonly Property<TemplateReferenceModel[]> templates;
		private readonly Property<string> contractTemplateName;

		private readonly Dictionary<string, IdentityWrapper> identities = new Dictionary<string, IdentityWrapper>();

		private readonly Command apply;

		private readonly ContractsClient contracts;
		private ContractModel currentContract;

		/// <summary>
		/// Legal ID Model
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ComponentJid">Component JID</param>
		public LegalModel(XmppClient Client, string ComponentJid)
			: base()
		{
			this.Add(this.firstName = new PersistedProperty<string>("Legal", nameof(this.FirstName), true, string.Empty, this));
			this.Add(this.middleName = new PersistedProperty<string>("Legal", nameof(this.MiddleName), true, string.Empty, this));
			this.Add(this.lastName = new PersistedProperty<string>("Legal", nameof(this.LastName), true, string.Empty, this));
			this.Add(this.personalNumber = new PersistedProperty<string>("Legal", nameof(this.PersonalNumber), true, string.Empty, this));
			this.Add(this.address = new PersistedProperty<string>("Legal", nameof(this.Address), true, string.Empty, this));
			this.Add(this.address2 = new PersistedProperty<string>("Legal", nameof(this.Address2), true, string.Empty, this));
			this.Add(this.zip = new PersistedProperty<string>("Legal", nameof(this.Zip), true, string.Empty, this));
			this.Add(this.area = new PersistedProperty<string>("Legal", nameof(this.Area), true, string.Empty, this));
			this.Add(this.city = new PersistedProperty<string>("Legal", nameof(this.City), true, string.Empty, this));
			this.Add(this.region = new PersistedProperty<string>("Legal", nameof(this.Region), true, string.Empty, this));
			this.Add(this.country = new PersistedProperty<string>("Legal", nameof(this.Country), true, string.Empty, this));

			this.template = new Property<Contract>(nameof(this.Template), null, this);
			this.templates = new Property<TemplateReferenceModel[]>(nameof(this.Templates), new TemplateReferenceModel[0], this);
			this.contractTemplateName = new Property<string>(nameof(ContractTemplateName), string.Empty, this);

			this.apply = new Command(this.CanExecuteApply, this.ExecuteApply);

			this.contracts = new ContractsClient(Client, ComponentJid);
			this.contracts.IdentityUpdated += Contracts_IdentityUpdated;
			this.contracts.PetitionForIdentityReceived += Contracts_PetitionForIdentityReceived;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.contracts.Dispose();
		}

		/// <summary>
		/// Contracts client.
		/// </summary>
		public ContractsClient Contracts => this.contracts;

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.LegalIdTab.DataContext = this;
				MainWindow.currentInstance.ContractsTab.DataContext = this;
				MainWindow.currentInstance.ContractsTab.ContractCommands.DataContext = this;

				return Task.CompletedTask;
			});

			await this.contracts.LoadKeys(true);

			LegalIdentity[] Identities = await this.contracts.GetLegalIdentitiesAsync();

			lock (this.identities)
			{
				foreach (LegalIdentity Identity in Identities)
					this.identities[Identity.Id] = new IdentityWrapper(this.contracts.Client.Domain, Identity);
			}

			this.RaisePropertyChanged(nameof(this.Identities));

			Dictionary<string, object> Settings = await RuntimeSettings.GetWhereKeyLikeAsync("Contract.Template.*", "*");
			List<TemplateReferenceModel> Templates = new List<TemplateReferenceModel>();

			foreach (KeyValuePair<string, object> Setting in Settings)
			{
				if (Setting.Value is string ContractId)
					Templates.Add(new TemplateReferenceModel(Setting.Key[18..], ContractId));
			}

			this.Templates = Templates.ToArray();

			await base.Start();
		}

		#region Legal Identity properties

		/// <summary>
		/// First name of person
		/// </summary>
		public string FirstName
		{
			get => this.firstName.Value;
			set => this.firstName.Value = value;
		}

		/// <summary>
		/// Middle name of person
		/// </summary>
		public string MiddleName
		{
			get => this.middleName.Value;
			set => this.middleName.Value = value;
		}

		/// <summary>
		/// Last name of person
		/// </summary>
		public string LastName
		{
			get => this.lastName.Value;
			set => this.lastName.Value = value;
		}

		/// <summary>
		/// Personal number
		/// </summary>
		public string PersonalNumber
		{
			get => this.personalNumber.Value;
			set => this.personalNumber.Value = value;
		}

		/// <summary>
		/// Address
		/// </summary>
		public string Address
		{
			get => this.address.Value;
			set => this.address.Value = value;
		}

		/// <summary>
		/// Address, second row
		/// </summary>
		public string Address2
		{
			get => this.address2.Value;
			set => this.address2.Value = value;
		}

		/// <summary>
		/// ZIP or postal code
		/// </summary>
		public string Zip
		{
			get => this.zip.Value;
			set => this.zip.Value = value;
		}

		/// <summary>
		/// Area
		/// </summary>
		public string Area
		{
			get => this.area.Value;
			set => this.area.Value = value;
		}

		/// <summary>
		/// City
		/// </summary>
		public string City
		{
			get => this.city.Value;
			set => this.city.Value = value;
		}

		/// <summary>
		/// Region
		/// </summary>
		public string Region
		{
			get => this.region.Value;
			set => this.region.Value = value;
		}

		/// <summary>
		/// Country
		/// </summary>
		public string Country
		{
			get => this.country.Value;
			set => this.country.Value = value;
		}

		/// <summary>
		/// ISO 3166-1 country codes
		/// </summary>
		public Iso_3166_1.Record[] CountryCodes
		{
			get => Iso_3166_1.Data;
		}

		#endregion

		#region Legal Identities

		/// <summary>
		/// Legal Identities registered on the account.
		/// </summary>
		public IdentityWrapper[] Identities
		{
			get
			{
				lock (this.identities)
				{
					IdentityWrapper[] Result = new IdentityWrapper[this.identities.Count];
					this.identities.Values.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		private Task Contracts_IdentityUpdated(object Sender, LegalIdentityEventArgs e)
		{
			lock (this.identities)
			{
				this.identities[e.Identity.Id] = new IdentityWrapper(this.contracts.Client.Domain, e.Identity);
			}

			this.RaisePropertyChanged(nameof(this.Identities));

			return Task.CompletedTask;
		}

		#endregion

		#region Apply for new Legal Identity

		/// <summary>
		/// Apply command
		/// </summary>
		public ICommand Apply => this.apply;

		private bool CanExecuteApply()
		{
			return this.contracts.Client.State == XmppState.Connected;
		}

		private async Task ExecuteApply()
		{
			try
			{
				List<Property> Properties = new List<Property>();

				AddProperty(Properties, "FIRST", this.FirstName);
				AddProperty(Properties, "MIDDLE", this.MiddleName);
				AddProperty(Properties, "LAST", this.LastName);
				AddProperty(Properties, "PNR", this.PersonalNumber);
				AddProperty(Properties, "ADDR", this.Address);
				AddProperty(Properties, "ADDR2", this.Address2);
				AddProperty(Properties, "ZIP", this.Zip);
				AddProperty(Properties, "AREA", this.Area);
				AddProperty(Properties, "CITY", this.City);
				AddProperty(Properties, "REGION", this.Region);
				AddProperty(Properties, "COUNTRY", this.Country);

				LegalIdentity Identity = await this.contracts.ApplyAsync(Properties.ToArray());

				lock (this.identities)
				{
					this.identities[Identity.Id] = new IdentityWrapper(this.contracts.Client.Domain, Identity);
				}

				this.RaisePropertyChanged(nameof(this.Identities));
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private static void AddProperty(List<Property> Properties, string Name, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
				Properties.Add(new Property(Name, Value));
		}

		#endregion

		#region Legal Identity Petitions

		private Task Contracts_PetitionForIdentityReceived(object Sender, LegalIdentityPetitionEventArgs e)
		{
			StringBuilder Question = new StringBuilder();

			Question.Append("A petition for your legal identity has been received for the following purpose: ");
			Question.Append(e.Purpose);
			Question.Append(" The petition was sent by");

			Append(Question, e.RequestorIdentity["FIRST"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["MIDDLE"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["LAST"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["PNR"], " (", ")");

			Question.Append(", at");

			Append(Question, e.RequestorIdentity["ADDR"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["ADDR2"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["ZIP"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["AREA"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["CITY"], " ", string.Empty);
			Append(Question, e.RequestorIdentity["REGION"], " ", string.Empty);

			string s = e.RequestorIdentity["ADDR"];
			if (!string.IsNullOrEmpty(s))
			{
				if (Iso_3166_1.CodeToCountry(s, out string Country))
					s = Country;

				Append(Question, s, " ", string.Empty);
			}

			Append(Question, e.RequestorFullJid, ", from ", string.Empty);

			Question.Append(". Do you want to return your identity information?");

			MainWindow.UpdateGui(() =>
			{
				switch (MessageBox.Show(Question.ToString(), "Petition received", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No))
				{
					case MessageBoxResult.Yes:
						Task.Run(() => this.contracts.PetitionIdentityResponseAsync(e.RequestedIdentityId, e.PetitionId, e.RequestorFullJid, true));
						break;

					case MessageBoxResult.No:
						Task.Run(() => this.contracts.PetitionIdentityResponseAsync(e.RequestedIdentityId, e.PetitionId, e.RequestorFullJid, false));
						break;
				}

				return Task.CompletedTask;
			});

			return Task.CompletedTask;
		}

		private static void Append(StringBuilder Question, string Value, string PrefixIfNotEmpty, string SuffixIfNotEmpty)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				Question.Append(PrefixIfNotEmpty);
				Question.Append(Value);
				Question.Append(SuffixIfNotEmpty);
			}
		}

		#endregion

		#region Create Contracts

		/// <summary>
		/// Template References
		/// </summary>
		public TemplateReferenceModel[] Templates
		{
			get => this.templates.Value;
			set => this.templates.Value = value;
		}

		/// <summary>
		/// Contract Template Name
		/// </summary>
		public string ContractTemplateName
		{
			get => this.contractTemplateName.Value;
			set
			{
				this.contractTemplateName.Value = value;
				this.LoadTemplate(value);
			}
		}

		/// <summary>
		/// Template selected as basis for new contracts
		/// </summary>
		public Contract Template
		{
			get => this.template.Value;
			set => this.template.Value = value;
		}

		/// <summary>
		/// A contract template has been added.
		/// </summary>
		/// <param name="TemplateName">Template name</param>
		/// <param name="Contract">Contract</param>
		public void ContractTemplateAdded(string TemplateName, Contract Contract)
		{
			SortedDictionary<string, TemplateReferenceModel> Templates = new SortedDictionary<string, TemplateReferenceModel>();

			foreach (TemplateReferenceModel Ref in this.Templates)
				Templates[Ref.TemplateName] = Ref;

			Templates[TemplateName] = new TemplateReferenceModel(TemplateName, Contract.ContractId);

			this.Templates = Templates.ToValueArray();
		}

		private async void LoadTemplate(string TemplateName)
		{
			try
			{
				string ContractId = await RuntimeSettings.GetAsync("Contract.Template." + TemplateName, string.Empty);
				if (string.IsNullOrEmpty(ContractId))
					return;

				MainWindow.MouseHourglass();
				this.Template = await this.contracts.GetContractAsync(ContractId);
				MainWindow.MouseDefault();

				if (!(this.currentContract is null))
					await this.currentContract.Stop();

				this.currentContract = new ContractModel(this.contracts, this.Template, this, MainWindow.currentInstance.ContractsTab);
				await this.currentContract.Start();

				await this.currentContract.PopulateParameters(MainWindow.currentInstance.ContractsTab.CreateParameters, MainWindow.currentInstance.ContractsTab.CreateCommands);
				await this.currentContract.PopulateContract(MainWindow.currentInstance.ContractsTab.ContractToCreate, MainWindow.currentInstance.ContractsTab.ContractToCreateHumanReadable);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		#endregion
	}
}
