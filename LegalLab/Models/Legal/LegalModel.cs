using LegalLab.Extensions;
using LegalLab.Models.Standards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Persistence;
using Waher.Runtime.Collections;
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
		private readonly PersistedProperty<string> nationality;
		private readonly PersistedProperty<string> gender;
		private readonly PersistedProperty<string> eMail;
		private readonly PersistedProperty<DateTime?> birthDate;

		private readonly PersistedProperty<string> orgName;
		private readonly PersistedProperty<string> orgDepartment;
		private readonly PersistedProperty<string> orgRole;
		private readonly PersistedProperty<string> orgNumber;
		private readonly PersistedProperty<string> orgAddress;
		private readonly PersistedProperty<string> orgAddress2;
		private readonly PersistedProperty<string> orgZip;
		private readonly PersistedProperty<string> orgArea;
		private readonly PersistedProperty<string> orgCity;
		private readonly PersistedProperty<string> orgRegion;
		private readonly PersistedProperty<string> orgCountry;

		private readonly Property<Contract> template;
		private readonly Property<TemplateReferenceModel[]> templates;
		private readonly Property<string> contractTemplateName;

		private readonly Dictionary<string, IdentityWrapper> identities = [];

		private readonly Command apply;

		private readonly ContractsClient contracts;
		private readonly HttpFileUploadClient httpFileUploadClient;
		private ContractModel currentContract;

		/// <summary>
		/// Legal ID Model
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ComponentJid">Component JID</param>
		/// <param name="FileUploadJid">HTTP File Upload JID.</param>
		/// <param name="MaxFileSize">Maximum file size for file uploads.</param>
		public LegalModel(XmppClient Client, string ComponentJid, string FileUploadJid, long? MaxFileSize)
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
			this.Add(this.nationality = new PersistedProperty<string>("Legal", nameof(this.Nationality), true, string.Empty, this));
			this.Add(this.gender = new PersistedProperty<string>("Legal", nameof(this.Gender), true, string.Empty, this));
			this.Add(this.eMail = new PersistedProperty<string>("Legal", nameof(this.EMail), true, string.Empty, this));
			this.Add(this.birthDate = new PersistedProperty<DateTime?>("Legal", nameof(this.BirthDate), true, null, this));

			this.Add(this.orgName = new PersistedProperty<string>("Legal", nameof(this.OrgName), true, string.Empty, this));
			this.Add(this.orgDepartment = new PersistedProperty<string>("Legal", nameof(this.OrgDepartment), true, string.Empty, this));
			this.Add(this.orgRole = new PersistedProperty<string>("Legal", nameof(this.OrgRole), true, string.Empty, this));
			this.Add(this.orgNumber = new PersistedProperty<string>("Legal", nameof(this.OrgNumber), true, string.Empty, this));
			this.Add(this.orgAddress = new PersistedProperty<string>("Legal", nameof(this.OrgAddress), true, string.Empty, this));
			this.Add(this.orgAddress2 = new PersistedProperty<string>("Legal", nameof(this.OrgAddress2), true, string.Empty, this));
			this.Add(this.orgZip = new PersistedProperty<string>("Legal", nameof(this.OrgZip), true, string.Empty, this));
			this.Add(this.orgArea = new PersistedProperty<string>("Legal", nameof(this.OrgArea), true, string.Empty, this));
			this.Add(this.orgCity = new PersistedProperty<string>("Legal", nameof(this.OrgCity), true, string.Empty, this));
			this.Add(this.orgRegion = new PersistedProperty<string>("Legal", nameof(this.OrgRegion), true, string.Empty, this));
			this.Add(this.orgCountry = new PersistedProperty<string>("Legal", nameof(this.OrgCountry), true, string.Empty, this));

			this.template = new Property<Contract>(nameof(this.Template), null, this);
			this.templates = new Property<TemplateReferenceModel[]>(nameof(this.Templates), [], this);
			this.contractTemplateName = new Property<string>(nameof(this.ContractTemplateName), string.Empty, this);

			this.apply = new Command(this.CanExecuteApply, this.ExecuteApply);

			this.contracts = new ContractsClient(Client, ComponentJid);
			this.contracts.EnableE2eEncryption(true);
			this.contracts.IdentityUpdated += this.Contracts_IdentityUpdated;
			this.contracts.ContractUpdated += this.Contracts_ContractUpdated;
			this.contracts.ContractDeleted += this.Contracts_ContractDeleted;
			this.contracts.PetitionForIdentityReceived += this.Contracts_PetitionForIdentityReceived;
			this.contracts.ClientMessage += this.Contracts_ClientMessage;

			this.httpFileUploadClient = new HttpFileUploadClient(Client, FileUploadJid, MaxFileSize);
		}

		private Task Contracts_ContractDeleted(object Sender, ContractReferenceEventArgs e)
		{
			return this.RemoveContract(e.ContractId);
		}

		private async Task Contracts_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			try
			{
				Contract Contract = await this.contracts.GetContractAsync(e.ContractId);

				switch (Contract.State)
				{
					case ContractState.Failed:
					case ContractState.Obsoleted:
					case ContractState.Deleted:
					case ContractState.Rejected:
						await this.RemoveContract(e.ContractId);
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		public async Task RemoveContract(string ContractId)
		{
			ChunkedList<TemplateReferenceModel> TemplatesToKeep = [];
			bool Changed = false;

			foreach (TemplateReferenceModel Model in this.Templates)
			{
				if (Model.ContractId == ContractId)
				{
					await RuntimeSettings.DeleteAsync("Contract.Template." + Model.TemplateName);
					Changed = true;
				}
				else
					TemplatesToKeep.Add(Model);
			}

			if (Changed)
			{
				await MainWindow.UpdateGui(() =>
				{
					this.Templates = [.. TemplatesToKeep];
					return Task.CompletedTask;
				});
			}
		}

		private Task Contracts_ClientMessage(object Sender, ClientMessageEventArgs e)
		{
			StringBuilder sb = new();

			sb.Append("Identity application processed.");

			if (e.IsValid.HasValue)
			{
				if (e.IsValid.Value)
					sb.AppendLine(" Application has been validated.");
				else
					sb.AppendLine(" Application has been rejected.");
			}
			else
				sb.AppendLine(" Application could not be validated or rejected.");

			ValidClaim[] ValidClaims = e.ValidClaims;
			int i, c;

			if ((c = ValidClaims.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Valid claims: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(ValidClaims[i].Claim);
				}

				sb.AppendLine();
			}

			ValidPhoto[] ValidPhotos = e.ValidPhotos;

			if ((c = ValidPhotos.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Valid photos: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(ValidPhotos[i].FileName);
				}

				sb.AppendLine();
			}

			InvalidClaim[] InvalidClaims = e.InvalidClaims;

			if ((c = InvalidClaims.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Invalid claims: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(InvalidClaims[i].Claim);

					if (!string.IsNullOrEmpty(InvalidClaims[i].Reason))
					{
						sb.Append(" (");
						sb.Append(InvalidClaims[i].Reason);
						sb.Append(')');
					}
				}

				sb.AppendLine();
			}

			InvalidPhoto[] InvalidPhotos = e.InvalidPhotos;

			if ((c = InvalidPhotos.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Invalid photos: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(InvalidPhotos[i].FileName);

					if (!string.IsNullOrEmpty(InvalidClaims[i].Reason))
					{
						sb.Append(" (");
						sb.Append(InvalidClaims[i].Reason);
						sb.Append(')');
					}
				}

				sb.AppendLine();
			}

			string[] UnvalidatedClaims = e.UnvalidatedClaims;

			if ((c = UnvalidatedClaims.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Unvalidated claims: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(UnvalidatedClaims[i]);
				}

				sb.AppendLine();
			}

			string[] UnvalidatedPhotos = e.UnvalidatedPhotos;

			if ((c = UnvalidatedPhotos.Length) > 0)
			{
				sb.AppendLine();
				sb.Append("Unvalidated photos: ");

				for (i = 0; i < c; i++)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else if (i > 0)
						sb.Append(", ");

					sb.Append(UnvalidatedPhotos[i]);
				}

				sb.AppendLine();
			}

			MainWindow.MessageBox(sb.ToString(), "Identity Application Validation Result",
				MessageBoxButton.OK, MessageBoxImage.Information);

			return Task.CompletedTask;
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

		/// <summary>
		/// HTTP File Upload client.
		/// </summary>
		public HttpFileUploadClient FileUpload => this.httpFileUploadClient;

		/// <summary>
		/// Current contract model.
		/// </summary>
		public ContractModel CurrentContract => this.currentContract;

		/// <inheritdoc/>
		public override async Task Start()
		{
			await MainWindow.UpdateGui(() =>
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
			List<TemplateReferenceModel> Templates = [];

			foreach (KeyValuePair<string, object> Setting in Settings)
			{
				if (Setting.Value is string ContractId)
					Templates.Add(new TemplateReferenceModel(Setting.Key[18..], ContractId));
			}

			this.Templates = [.. Templates];

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
		public static Iso_3166_1.Record[] CountryCodes => Iso_3166_1.Data;

		/// <summary>
		/// ISO 5218 gender codes
		/// </summary>
		public static Iso_5218.Record[] GenderCodes => Iso_5218.Data;

		/// <summary>
		/// Nationality
		/// </summary>
		public string Nationality
		{
			get => this.nationality.Value;
			set => this.nationality.Value = value;
		}

		/// <summary>
		/// Gender
		/// </summary>
		public string Gender
		{
			get => this.gender.Value;
			set => this.gender.Value = value;
		}

		/// <summary>
		/// Birth Date
		/// </summary>
		public DateTime? BirthDate
		{
			get => this.birthDate.Value;
			set => this.birthDate.Value = value;
		}

		/// <summary>
		/// e-Mail address
		/// </summary>
		public string EMail
		{
			get => this.eMail.Value;
			set => this.eMail.Value = value;
		}

		/// <summary>
		/// Organization name
		/// </summary>
		public string OrgName
		{
			get => this.orgName.Value;
			set => this.orgName.Value = value;
		}

		/// <summary>
		/// Organization Department
		/// </summary>
		public string OrgDepartment
		{
			get => this.orgDepartment.Value;
			set => this.orgDepartment.Value = value;
		}

		/// <summary>
		/// Organization Role
		/// </summary>
		public string OrgRole
		{
			get => this.orgRole.Value;
			set => this.orgRole.Value = value;
		}

		/// <summary>
		/// Organization number
		/// </summary>
		public string OrgNumber
		{
			get => this.orgNumber.Value;
			set => this.orgNumber.Value = value;
		}

		/// <summary>
		/// Organization Address
		/// </summary>
		public string OrgAddress
		{
			get => this.orgAddress.Value;
			set => this.orgAddress.Value = value;
		}

		/// <summary>
		/// Organization Address, second row
		/// </summary>
		public string OrgAddress2
		{
			get => this.orgAddress2.Value;
			set => this.orgAddress2.Value = value;
		}

		/// <summary>
		/// Organization ZIP or postal code
		/// </summary>
		public string OrgZip
		{
			get => this.orgZip.Value;
			set => this.orgZip.Value = value;
		}

		/// <summary>
		/// Organization Area
		/// </summary>
		public string OrgArea
		{
			get => this.orgArea.Value;
			set => this.orgArea.Value = value;
		}

		/// <summary>
		/// Organization City
		/// </summary>
		public string OrgCity
		{
			get => this.orgCity.Value;
			set => this.orgCity.Value = value;
		}

		/// <summary>
		/// Organization Region
		/// </summary>
		public string OrgRegion
		{
			get => this.orgRegion.Value;
			set => this.orgRegion.Value = value;
		}

		/// <summary>
		/// Organization Country
		/// </summary>
		public string OrgCountry
		{
			get => this.orgCountry.Value;
			set => this.orgCountry.Value = value;
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
				LegalIdentity[] Identities = await this.contracts.GetLegalIdentitiesAsync();

				foreach (LegalIdentity OldIdentity in Identities)
				{
					if (OldIdentity.State == IdentityState.Approved)
					{
						try
						{
							await this.contracts.ObsoleteLegalIdentityAsync(OldIdentity.Id);
						}
						catch (Exception ex)
						{
							MainWindow.ErrorBox(ex.Message);
						}
					}
				}

				List<Property> Properties = [];

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
				AddProperty(Properties, "NATIONALITY", this.Nationality);
				AddProperty(Properties, "GENDER", this.Gender);
				AddProperty(Properties, "EMAIL", this.EMail);

				if (this.BirthDate.HasValue)
				{
					AddProperty(Properties, "BDAY", this.BirthDate.Value.Day.ToString());
					AddProperty(Properties, "BMONTH", this.BirthDate.Value.Month.ToString());
					AddProperty(Properties, "BYEAR", this.BirthDate.Value.Year.ToString());
				}

				AddProperty(Properties, "ORGNAME", this.OrgName);
				AddProperty(Properties, "ORGDEPT", this.OrgDepartment);
				AddProperty(Properties, "ORGROLE", this.OrgRole);
				AddProperty(Properties, "ORGNR", this.OrgNumber);
				AddProperty(Properties, "ORGADDR", this.OrgAddress);
				AddProperty(Properties, "ORGADDR2", this.OrgAddress2);
				AddProperty(Properties, "ORGZIP", this.OrgZip);
				AddProperty(Properties, "ORGAREA", this.OrgArea);
				AddProperty(Properties, "ORGCITY", this.OrgCity);
				AddProperty(Properties, "ORGREGION", this.OrgRegion);
				AddProperty(Properties, "ORGCOUNTRY", this.OrgCountry);

				AddProperty(Properties, "JID", this.contracts.Client.BareJID);

				LegalIdentity Identity = await this.contracts.ApplyAsync([.. Properties]);

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
			StringBuilder Question = new();

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

			string s = e.RequestorIdentity["COUNTRY"];
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
				MainWindow.UpdateGui(() => this.LoadTemplate(value, null));
			}
		}

		/// <summary>
		/// Selects the template to use.
		/// </summary>
		/// <param name="Name">Name of contract template to select.</param>
		/// <param name="PresetValues">Optional preset values. Can be null.</param>
		public async Task SetContractTemplateName(string Name, Dictionary<CaseInsensitiveString, object> PresetValues)
		{
			this.contractTemplateName.Value = Name;
			this.RaisePropertyChanged(nameof(this.ContractTemplateName));
			await this.LoadTemplate(Name, PresetValues);
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
			SortedDictionary<string, TemplateReferenceModel> Templates = [];

			foreach (TemplateReferenceModel Ref in this.Templates)
				Templates[Ref.TemplateName] = Ref;

			Templates[TemplateName] = new TemplateReferenceModel(TemplateName, Contract.ContractId);

			this.Templates = Templates.ToValueArray();
		}

		private async Task LoadTemplate(string TemplateName, Dictionary<CaseInsensitiveString, object> PresetValues)
		{
			string ContractId = await RuntimeSettings.GetAsync("Contract.Template." + TemplateName, string.Empty);
			if (string.IsNullOrEmpty(ContractId))
				return;

			try
			{
				MainWindow.MouseHourglass();
				this.Template = await this.contracts.GetContractAsync(ContractId);
				MainWindow.MouseDefault();

				if (this.currentContract is not null)
					await this.currentContract.Stop();

				this.currentContract = await ContractModel.CreateAsync(this.contracts, this.Template, MainWindow.DesignModel,
					MainWindow.currentInstance.ContractsTab.MachineReadableXmlEditor);

				await this.currentContract.Start();

				await this.currentContract.PopulateParameters(
					MainWindow.currentInstance.ContractsTab.LanguageOptions,
					MainWindow.currentInstance.ContractsTab.CreateParameters,
					MainWindow.currentInstance.ContractsTab.TemplateCommands,
					PresetValues);

				await this.currentContract.PopulateContract(
					MainWindow.currentInstance.ContractsTab.ContractToCreate,
					MainWindow.currentInstance.ContractsTab.ContractToCreateHumanReadable);
			}
			catch (Exception ex)
			{
				if (await MainWindow.MessageBox("Unable to load template. Do you want to remove it?\r\n\r\nError returned: " +
					ex.Message, "Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					await this.RemoveContract(ContractId);
				}
			}
		}

		#endregion
	}
}
