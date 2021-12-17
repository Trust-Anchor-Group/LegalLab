using System;
using System.Collections.Generic;

namespace LegalLab.Models.Standards
{
	/// <summary>
	/// Static class containing ISO 3166-1 country codes
	/// </summary>
	public static class Iso3166_1
	{
		/// <summary>
		/// Contains one record of the ISO 3166-1 data set.
		/// </summary>
		public class Record
		{
			/// <summary>
			/// Contains one record of the ISO 3166-1 data set.
			/// </summary>
			/// <param name="Country">Country name</param>
			/// <param name="Code">Country Code</param>
			public Record(string Country, string Code)
			{
				this.Country = Country;
				this.Code = Code;
			}

			/// <summary>
			/// Country
			/// </summary>
			public string Country { get; set; }

			/// <summary>
			/// ISO 3166-1 country code
			/// </summary>
			public string Code { get; set; }
		}

		private static Dictionary<string, string> countryByCode = null;

		/// <summary>
		/// Tries to get the country name corresponding to an ISO 3166-1 country code.
		/// </summary>
		/// <param name="Code">Country code</param>
		/// <param name="Country">Country, if found.</param>
		/// <returns>If a corresponding country code was foudn.</returns>
		public static bool CodeToCountry(string Code, out string Country)
		{
			if (countryByCode is null)
			{
				Dictionary<string, string> Temp = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

				foreach (Record Rec in Data)
					Temp[Rec.Code] = Rec.Country;

				countryByCode = Temp;
			}

			return countryByCode.TryGetValue(Code, out Country);
		}

		/// <summary>
		/// Available country codes
		/// </summary>
		public static readonly Record[] Data = new Record[]
		{
			new Record("AFGHANISTAN", "AF"),
			new Record("ÅLAND ISLANDS", "AX"),
			new Record("ALBANIA", "AL"),
			new Record("ALGERIA", "DZ"),
			new Record("AMERICAN SAMOA", "AS"),
			new Record("ANDORRA", "AD"),
			new Record("ANGOLA", "AO"),
			new Record("ANGUILLA", "AI"),
			new Record("ANTARCTICA", "AQ"),
			new Record("ANTIGUA AND BARBUDA", "AG"),
			new Record("ARGENTINA", "AR"),
			new Record("ARMENIA", "AM"),
			new Record("ARUBA", "AW"),
			new Record("AUSTRALIA", "AU"),
			new Record("AUSTRIA", "AT"),
			new Record("AZERBAIJAN", "AZ"),
			new Record("BAHAMAS", "BS"),
			new Record("BAHRAIN", "BH"),
			new Record("BANGLADESH", "BD"),
			new Record("BARBADOS", "BB"),
			new Record("BELARUS", "BY"),
			new Record("BELGIUM", "BE"),
			new Record("BELIZE", "BZ"),
			new Record("BENIN", "BJ"),
			new Record("BERMUDA", "BM"),
			new Record("BHUTAN", "BT"),
			new Record("BOLIVIA", "BO"),
			new Record("BOSNIA AND HERZEGOVINA", "BA"),
			new Record("BOTSWANA", "BW"),
			new Record("BOUVET ISLAND", "BV"),
			new Record("BRAZIL", "BR"),
			new Record("BRITISH INDIAN OCEAN TERRITORY", "IO"),
			new Record("BRUNEI DARUSSALAM", "BN"),
			new Record("BULGARIA", "BG"),
			new Record("BURKINA FASO", "BF"),
			new Record("BURUNDI", "BI"),
			new Record("CAMBODIA", "KH"),
			new Record("CAMEROON", "CM"),
			new Record("CANADA", "CA"),
			new Record("CAPE VERDE", "CV"),
			new Record("CAYMAN ISLANDS", "KY"),
			new Record("CENTRAL AFRICAN REPUBLIC", "CF"),
			new Record("CHAD", "TD"),
			new Record("CHILE", "CL"),
			new Record("CHINA", "CN"),
			new Record("CHRISTMAS ISLAND", "CX"),
			new Record("COCOS (KEELING) ISLANDS", "CC"),
			new Record("COLOMBIA", "CO"),
			new Record("COMOROS", "KM"),
			new Record("CONGO", "CG"),
			new Record("CONGO, THE DEMOCRATIC REPUBLIC OF THE", "CD"),
			new Record("COOK ISLANDS", "CK"),
			new Record("COSTA RICA", "CR"),
			new Record("COTE D'IVOIRE", "CI"),
			new Record("CROATIA", "HR"),
			new Record("CUBA", "CU"),
			new Record("CYPRUS", "CY"),
			new Record("CZECH REPUBLIC", "CZ"),
			new Record("DENMARK", "DK"),
			new Record("DJIBOUTI", "DJ"),
			new Record("DOMINICA", "DM"),
			new Record("DOMINICAN REPUBLIC", "DO"),
			new Record("ECUADOR", "EC"),
			new Record("EGYPT", "EG"),
			new Record("EL SALVADOR", "SV"),
			new Record("EQUATORIAL GUINEA", "GQ"),
			new Record("ERITREA", "ER"),
			new Record("ESTONIA", "EE"),
			new Record("ETHIOPIA", "ET"),
			new Record("FALKLAND ISLANDS (MALVINAS)", "FK"),
			new Record("FAROE ISLANDS", "FO"),
			new Record("FIJI", "FJ"),
			new Record("FINLAND", "FI"),
			new Record("FRANCE", "FR"),
			new Record("FRENCH GUIANA", "GF"),
			new Record("FRENCH POLYNESIA", "PF"),
			new Record("FRENCH SOUTHERN TERRITORIES", "TF"),
			new Record("GABON", "GA"),
			new Record("GAMBIA", "GM"),
			new Record("GEORGIA", "GE"),
			new Record("GERMANY", "DE"),
			new Record("GHANA", "GH"),
			new Record("GIBRALTAR", "GI"),
			new Record("GREECE", "GR"),
			new Record("GREENLAND", "GL"),
			new Record("GRENADA", "GD"),
			new Record("GUADELOUPE", "GP"),
			new Record("GUAM", "GU"),
			new Record("GUATEMALA", "GT"),
			new Record("GUERNSEY", "GG"),
			new Record("GUINEA", "GN"),
			new Record("GUINEA-BISSAU", "GW"),
			new Record("GUYANA", "GY"),
			new Record("HAITI", "HT"),
			new Record("HEARD ISLAND AND MCDONALD ISLANDS", "HM"),
			new Record("HOLY SEE (VATICAN CITY STATE)", "VA"),
			new Record("HONDURAS", "HN"),
			new Record("HONG KONG", "HK"),
			new Record("HUNGARY", "HU"),
			new Record("ICELAND", "IS"),
			new Record("INDIA", "IN"),
			new Record("INDONESIA", "ID"),
			new Record("IRAN, ISLAMIC REPUBLIC OF", "IR"),
			new Record("IRAQ", "IQ"),
			new Record("IRELAND", "IE"),
			new Record("ISLE OF MAN", "IM"),
			new Record("ISRAEL", "IL"),
			new Record("ITALY", "IT"),
			new Record("JAMAICA", "JM"),
			new Record("JAPAN", "JP"),
			new Record("JERSEY", "JE"),
			new Record("JORDAN", "JO"),
			new Record("KAZAKHSTAN", "KZ"),
			new Record("KENYA", "KE"),
			new Record("KIRIBATI", "KI"),
			new Record("KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF", "KP"),
			new Record("KOREA, REPUBLIC OF", "KR"),
			new Record("KUWAIT", "KW"),
			new Record("KYRGYZSTAN", "KG"),
			new Record("LAO PEOPLE'S DEMOCRATIC REPUBLIC", "LA"),
			new Record("LATVIA", "LV"),
			new Record("LEBANON", "LB"),
			new Record("LESOTHO", "LS"),
			new Record("LIBERIA", "LR"),
			new Record("LIBYAN ARAB JAMAHIRIYA", "LY"),
			new Record("LIECHTENSTEIN", "LI"),
			new Record("LITHUANIA", "LT"),
			new Record("LUXEMBOURG", "LU"),
			new Record("MACAO", "MO"),
			new Record("MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF", "MK"),
			new Record("MADAGASCAR", "MG"),
			new Record("MALAWI", "MW"),
			new Record("MALAYSIA", "MY"),
			new Record("MALDIVES", "MV"),
			new Record("MALI", "ML"),
			new Record("MALTA", "MT"),
			new Record("MARSHALL ISLANDS", "MH"),
			new Record("MARTINIQUE", "MQ"),
			new Record("MAURITANIA", "MR"),
			new Record("MAURITIUS", "MU"),
			new Record("MAYOTTE", "YT"),
			new Record("MEXICO", "MX"),
			new Record("MICRONESIA, FEDERATED STATES OF", "FM"),
			new Record("MOLDOVA, REPUBLIC OF", "MD"),
			new Record("MONACO", "MC"),
			new Record("MONGOLIA", "MN"),
			new Record("MONTSERRAT", "MS"),
			new Record("MOROCCO", "MA"),
			new Record("MOZAMBIQUE", "MZ"),
			new Record("MYANMAR", "MM"),
			new Record("NAMIBIA", "NA"),
			new Record("NAURU", "NR"),
			new Record("NEPAL", "NP"),
			new Record("NETHERLANDS", "NL"),
			new Record("NETHERLANDS ANTILLES", "AN"),
			new Record("NEW CALEDONIA", "NC"),
			new Record("NEW ZEALAND", "NZ"),
			new Record("NICARAGUA", "NI"),
			new Record("NIGER", "NE"),
			new Record("NIGERIA", "NG"),
			new Record("NIUE", "NU"),
			new Record("NORFOLK ISLAND", "NF"),
			new Record("NORTHERN MARIANA ISLANDS", "MP"),
			new Record("NORWAY", "NO"),
			new Record("OMAN", "OM"),
			new Record("PAKISTAN", "PK"),
			new Record("PALAU", "PW"),
			new Record("PALESTINIAN TERRITORY, OCCUPIED", "PS"),
			new Record("PANAMA", "PA"),
			new Record("PAPUA NEW GUINEA", "PG"),
			new Record("PARAGUAY", "PY"),
			new Record("PERU", "PE"),
			new Record("PHILIPPINES", "PH"),
			new Record("PITCAIRN", "PN"),
			new Record("POLAND", "PL"),
			new Record("PORTUGAL", "PT"),
			new Record("PUERTO RICO", "PR"),
			new Record("QATAR", "QA"),
			new Record("REUNION", "RE"),
			new Record("ROMANIA", "RO"),
			new Record("RUSSIAN FEDERATION", "RU"),
			new Record("RWANDA", "RW"),
			new Record("SAINT HELENA", "SH"),
			new Record("SAINT KITTS AND NEVIS", "KN"),
			new Record("SAINT LUCIA", "LC"),
			new Record("SAINT PIERRE AND MIQUELON", "PM"),
			new Record("SAINT VINCENT AND THE GRENADINES", "VC"),
			new Record("SAMOA", "WS"),
			new Record("SAN MARINO", "SM"),
			new Record("SAO TOME AND PRINCIPE", "ST"),
			new Record("SAUDI ARABIA", "SA"),
			new Record("SENEGAL", "SN"),
			new Record("SERBIA AND MONTENEGRO", "CS"),
			new Record("SEYCHELLES", "SC"),
			new Record("SIERRA LEONE", "SL"),
			new Record("SINGAPORE", "SG"),
			new Record("SLOVAKIA", "SK"),
			new Record("SLOVENIA", "SI"),
			new Record("SOLOMON ISLANDS", "SB"),
			new Record("SOMALIA", "SO"),
			new Record("SOUTH AFRICA", "ZA"),
			new Record("SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS", "GS"),
			new Record("SPAIN", "ES"),
			new Record("SRI LANKA", "LK"),
			new Record("SUDAN", "SD"),
			new Record("SURINAME", "SR"),
			new Record("SVALBARD AND JAN MAYEN", "SJ"),
			new Record("SWAZILAND", "SZ"),
			new Record("SWEDEN", "SE"),
			new Record("SWITZERLAND", "CH"),
			new Record("SYRIAN ARAB REPUBLIC", "SY"),
			new Record("TAIWAN, PROVINCE OF CHINA", "TW"),
			new Record("TAJIKISTAN", "TJ"),
			new Record("TANZANIA, UNITED REPUBLIC OF", "TZ"),
			new Record("THAILAND", "TH"),
			new Record("TIMOR-LESTE", "TL"),
			new Record("TOGO", "TG"),
			new Record("TOKELAU", "TK"),
			new Record("TONGA", "TO"),
			new Record("TRINIDAD AND TOBAGO", "TT"),
			new Record("TUNISIA", "TN"),
			new Record("TURKEY", "TR"),
			new Record("TURKMENISTAN", "TM"),
			new Record("TURKS AND CAICOS ISLANDS", "TC"),
			new Record("TUVALU", "TV"),
			new Record("UGANDA", "UG"),
			new Record("UKRAINE", "UA"),
			new Record("UNITED ARAB EMIRATES", "AE"),
			new Record("UNITED KINGDOM", "GB"),
			new Record("UNITED STATES", "US"),
			new Record("UNITED STATES MINOR OUTLYING ISLANDS", "UM"),
			new Record("URUGUAY", "UY"),
			new Record("UZBEKISTAN", "UZ"),
			new Record("VANUATU", "VU"),
			new Record("VENEZUELA", "VE"),
			new Record("VIET NAM", "VN"),
			new Record("VIRGIN ISLANDS, BRITISH", "VG"),
			new Record("VIRGIN ISLANDS, U.S.", "VI"),
			new Record("WALLIS AND FUTUNA", "WF"),
			new Record("WESTERN SAHARA", "EH"),
			new Record("YEMEN", "YE"),
			new Record("ZAMBIA", "ZM"),
			new Record("ZIMBABWE", "ZW")
		};
	}
}
