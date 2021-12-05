﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Inventory;

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

		private readonly Dictionary<string, IdentityWrapper> identities = new Dictionary<string, IdentityWrapper>();

		private readonly Command apply;

		private readonly ContractsClient contracts;

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
			});

			await this.contracts.LoadKeys(true);

			LegalIdentity[] Identities = await this.contracts.GetLegalIdentitiesAsync();

			lock (this.identities)
			{
				foreach (LegalIdentity Identity in Identities)
					this.identities[Identity.Id] = new IdentityWrapper(this.contracts.Client.Domain, Identity);
			}

			this.RaisePropertyChanged(nameof(this.Identities));

			await base.Start();
		}

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
		public Iso3166_1.Record[] CountryCodes
		{
			get => Iso3166_1.Data;
		}

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

		/// <summary>
		/// Apply command
		/// </summary>
		public ICommand Apply => this.apply;

		private bool CanExecuteApply()
		{
			return this.contracts.Client.State == XmppState.Connected;
		}

		private async void ExecuteApply()
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
				if (Iso3166_1.CodeToCountry(s, out string Country))
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
	}
}
