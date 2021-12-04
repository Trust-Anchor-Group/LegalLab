﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.contracts.Dispose();
		}

		/// <inheritdoc/>
		public override async Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.LegalIdTab.DataContext = this;
				MainWindow.currentInstance.ContractsTab.DataContext = this;
			});

			await this.contracts.LoadKeys(true);

			await base.Start();
		}

		/// <inheritdoc/>
		public override Task Stop()
		{
			return base.Stop();
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
	}
}
