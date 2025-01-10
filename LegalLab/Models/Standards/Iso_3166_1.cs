using System;
using System.Collections.Generic;

namespace LegalLab.Models.Standards
{
	/// <summary>
	/// Static class containing ISO 3166-1 country codes
	/// </summary>
	public static class Iso_3166_1
	{
		/// <summary>
		/// Contains one record of the ISO 3166-1 data set.
		/// </summary>
		/// <param name="Country">Country name</param>
		/// <param name="Code">Country Code</param>
		public class Record(string Country, string Code)
		{
			/// <summary>
			/// Country
			/// </summary>
			public string Country { get; set; } = Country;

			/// <summary>
			/// ISO 3166-1 country code
			/// </summary>
			public string Code { get; set; } = Code;
		}

		private static Dictionary<string, string> countryByCode = null;

		/// <summary>
		/// Tries to get the country name corresponding to an ISO 3166-1 country code.
		/// </summary>
		/// <param name="Code">Country code</param>
		/// <param name="Country">Country, if found.</param>
		/// <returns>If a corresponding country code was found.</returns>
		public static bool CodeToCountry(string Code, out string Country)
		{
			if (countryByCode is null)
			{
				Dictionary<string, string> Temp = new(StringComparer.CurrentCultureIgnoreCase);

				foreach (Record Rec in Data)
					Temp[Rec.Code] = Rec.Country;

				countryByCode = Temp;
			}

			return countryByCode.TryGetValue(Code, out Country);
		}

		/// <summary>
		/// Available country codes
		/// </summary>
		public static readonly Record[] Data =
		[
			new("AFGHANISTAN", "AF"),
			new("ÅLAND ISLANDS", "AX"),
			new("ALBANIA", "AL"),
			new("ALGERIA", "DZ"),
			new("AMERICAN SAMOA", "AS"),
			new("ANDORRA", "AD"),
			new("ANGOLA", "AO"),
			new("ANGUILLA", "AI"),
			new("ANTARCTICA", "AQ"),
			new("ANTIGUA AND BARBUDA", "AG"),
			new("ARGENTINA", "AR"),
			new("ARMENIA", "AM"),
			new("ARUBA", "AW"),
			new("AUSTRALIA", "AU"),
			new("AUSTRIA", "AT"),
			new("AZERBAIJAN", "AZ"),
			new("BAHAMAS", "BS"),
			new("BAHRAIN", "BH"),
			new("BANGLADESH", "BD"),
			new("BARBADOS", "BB"),
			new("BELARUS", "BY"),
			new("BELGIUM", "BE"),
			new("BELIZE", "BZ"),
			new("BENIN", "BJ"),
			new("BERMUDA", "BM"),
			new("BHUTAN", "BT"),
			new("BOLIVIA", "BO"),
			new("BOSNIA AND HERZEGOVINA", "BA"),
			new("BOTSWANA", "BW"),
			new("BOUVET ISLAND", "BV"),
			new("BRAZIL", "BR"),
			new("BRITISH INDIAN OCEAN TERRITORY", "IO"),
			new("BRUNEI DARUSSALAM", "BN"),
			new("BULGARIA", "BG"),
			new("BURKINA FASO", "BF"),
			new("BURUNDI", "BI"),
			new("CAMBODIA", "KH"),
			new("CAMEROON", "CM"),
			new("CANADA", "CA"),
			new("CAPE VERDE", "CV"),
			new("CAYMAN ISLANDS", "KY"),
			new("CENTRAL AFRICAN REPUBLIC", "CF"),
			new("CHAD", "TD"),
			new("CHILE", "CL"),
			new("CHINA", "CN"),
			new("CHRISTMAS ISLAND", "CX"),
			new("COCOS (KEELING) ISLANDS", "CC"),
			new("COLOMBIA", "CO"),
			new("COMOROS", "KM"),
			new("CONGO", "CG"),
			new("CONGO, THE DEMOCRATIC REPUBLIC OF THE", "CD"),
			new("COOK ISLANDS", "CK"),
			new("COSTA RICA", "CR"),
			new("COTE D'IVOIRE", "CI"),
			new("CROATIA", "HR"),
			new("CUBA", "CU"),
			new("CYPRUS", "CY"),
			new("CZECH REPUBLIC", "CZ"),
			new("DENMARK", "DK"),
			new("DJIBOUTI", "DJ"),
			new("DOMINICA", "DM"),
			new("DOMINICAN REPUBLIC", "DO"),
			new("ECUADOR", "EC"),
			new("EGYPT", "EG"),
			new("EL SALVADOR", "SV"),
			new("EQUATORIAL GUINEA", "GQ"),
			new("ERITREA", "ER"),
			new("ESTONIA", "EE"),
			new("ETHIOPIA", "ET"),
			new("FALKLAND ISLANDS (MALVINAS)", "FK"),
			new("FAROE ISLANDS", "FO"),
			new("FIJI", "FJ"),
			new("FINLAND", "FI"),
			new("FRANCE", "FR"),
			new("FRENCH GUIANA", "GF"),
			new("FRENCH POLYNESIA", "PF"),
			new("FRENCH SOUTHERN TERRITORIES", "TF"),
			new("GABON", "GA"),
			new("GAMBIA", "GM"),
			new("GEORGIA", "GE"),
			new("GERMANY", "DE"),
			new("GHANA", "GH"),
			new("GIBRALTAR", "GI"),
			new("GREECE", "GR"),
			new("GREENLAND", "GL"),
			new("GRENADA", "GD"),
			new("GUADELOUPE", "GP"),
			new("GUAM", "GU"),
			new("GUATEMALA", "GT"),
			new("GUERNSEY", "GG"),
			new("GUINEA", "GN"),
			new("GUINEA-BISSAU", "GW"),
			new("GUYANA", "GY"),
			new("HAITI", "HT"),
			new("HEARD ISLAND AND MCDONALD ISLANDS", "HM"),
			new("HOLY SEE (VATICAN CITY STATE)", "VA"),
			new("HONDURAS", "HN"),
			new("HONG KONG", "HK"),
			new("HUNGARY", "HU"),
			new("ICELAND", "IS"),
			new("INDIA", "IN"),
			new("INDONESIA", "ID"),
			new("IRAN, ISLAMIC REPUBLIC OF", "IR"),
			new("IRAQ", "IQ"),
			new("IRELAND", "IE"),
			new("ISLE OF MAN", "IM"),
			new("ISRAEL", "IL"),
			new("ITALY", "IT"),
			new("JAMAICA", "JM"),
			new("JAPAN", "JP"),
			new("JERSEY", "JE"),
			new("JORDAN", "JO"),
			new("KAZAKHSTAN", "KZ"),
			new("KENYA", "KE"),
			new("KIRIBATI", "KI"),
			new("KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF", "KP"),
			new("KOREA, REPUBLIC OF", "KR"),
			new("KUWAIT", "KW"),
			new("KYRGYZSTAN", "KG"),
			new("LAO PEOPLE'S DEMOCRATIC REPUBLIC", "LA"),
			new("LATVIA", "LV"),
			new("LEBANON", "LB"),
			new("LESOTHO", "LS"),
			new("LIBERIA", "LR"),
			new("LIBYAN ARAB JAMAHIRIYA", "LY"),
			new("LIECHTENSTEIN", "LI"),
			new("LITHUANIA", "LT"),
			new("LUXEMBOURG", "LU"),
			new("MACAO", "MO"),
			new("MADAGASCAR", "MG"),
			new("MALAWI", "MW"),
			new("MALAYSIA", "MY"),
			new("MALDIVES", "MV"),
			new("MALI", "ML"),
			new("MALTA", "MT"),
			new("MARSHALL ISLANDS", "MH"),
			new("MARTINIQUE", "MQ"),
			new("MAURITANIA", "MR"),
			new("MAURITIUS", "MU"),
			new("MAYOTTE", "YT"),
			new("MEXICO", "MX"),
			new("MICRONESIA, FEDERATED STATES OF", "FM"),
			new("MOLDOVA, REPUBLIC OF", "MD"),
			new("MONACO", "MC"),
			new("MONGOLIA", "MN"),
			new("MONTENEGRO", "ME"),
			new("MONTSERRAT", "MS"),
			new("MOROCCO", "MA"),
			new("MOZAMBIQUE", "MZ"),
			new("MYANMAR", "MM"),
			new("NAMIBIA", "NA"),
			new("NAURU", "NR"),
			new("NEPAL", "NP"),
			new("NETHERLANDS", "NL"),
			new("NETHERLANDS ANTILLES", "AN"),
			new("NEW CALEDONIA", "NC"),
			new("NEW ZEALAND", "NZ"),
			new("NICARAGUA", "NI"),
			new("NIGER", "NE"),
			new("NIGERIA", "NG"),
			new("NIUE", "NU"),
			new("NORFOLK ISLAND", "NF"),
			new("NORTH MACEDONIA", "MK"),
			new("NORTHERN MARIANA ISLANDS", "MP"),
			new("NORWAY", "NO"),
			new("OMAN", "OM"),
			new("PAKISTAN", "PK"),
			new("PALAU", "PW"),
			new("PALESTINIAN TERRITORY, OCCUPIED", "PS"),
			new("PANAMA", "PA"),
			new("PAPUA NEW GUINEA", "PG"),
			new("PARAGUAY", "PY"),
			new("PERU", "PE"),
			new("PHILIPPINES", "PH"),
			new("PITCAIRN", "PN"),
			new("POLAND", "PL"),
			new("PORTUGAL", "PT"),
			new("PUERTO RICO", "PR"),
			new("QATAR", "QA"),
			new("REUNION", "RE"),
			new("ROMANIA", "RO"),
			new("RUSSIAN FEDERATION", "RU"),
			new("RWANDA", "RW"),
			new("SAINT HELENA", "SH"),
			new("SAINT KITTS AND NEVIS", "KN"),
			new("SAINT LUCIA", "LC"),
			new("SAINT PIERRE AND MIQUELON", "PM"),
			new("SAINT VINCENT AND THE GRENADINES", "VC"),
			new("SAMOA", "WS"),
			new("SAN MARINO", "SM"),
			new("SAO TOME AND PRINCIPE", "ST"),
			new("SAUDI ARABIA", "SA"),
			new("SENEGAL", "SN"),
			new("SERBIA", "RS"),
			new("SEYCHELLES", "SC"),
			new("SIERRA LEONE", "SL"),
			new("SINGAPORE", "SG"),
			new("SLOVAKIA", "SK"),
			new("SLOVENIA", "SI"),
			new("SOLOMON ISLANDS", "SB"),
			new("SOMALIA", "SO"),
			new("SOUTH AFRICA", "ZA"),
			new("SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS", "GS"),
			new("SPAIN", "ES"),
			new("SRI LANKA", "LK"),
			new("SUDAN", "SD"),
			new("SURINAME", "SR"),
			new("SVALBARD AND JAN MAYEN", "SJ"),
			new("SWAZILAND", "SZ"),
			new("SWEDEN", "SE"),
			new("SWITZERLAND", "CH"),
			new("SYRIAN ARAB REPUBLIC", "SY"),
			new("TAIWAN, PROVINCE OF CHINA", "TW"),
			new("TAJIKISTAN", "TJ"),
			new("TANZANIA, UNITED REPUBLIC OF", "TZ"),
			new("THAILAND", "TH"),
			new("TIMOR-LESTE", "TL"),
			new("TOGO", "TG"),
			new("TOKELAU", "TK"),
			new("TONGA", "TO"),
			new("TRINIDAD AND TOBAGO", "TT"),
			new("TUNISIA", "TN"),
			new("TURKEY", "TR"),
			new("TURKMENISTAN", "TM"),
			new("TURKS AND CAICOS ISLANDS", "TC"),
			new("TUVALU", "TV"),
			new("UGANDA", "UG"),
			new("UKRAINE", "UA"),
			new("UNITED ARAB EMIRATES", "AE"),
			new("UNITED KINGDOM", "GB"),
			new("UNITED STATES", "US"),
			new("UNITED STATES MINOR OUTLYING ISLANDS", "UM"),
			new("URUGUAY", "UY"),
			new("UZBEKISTAN", "UZ"),
			new("VANUATU", "VU"),
			new("VENEZUELA", "VE"),
			new("VIET NAM", "VN"),
			new("VIRGIN ISLANDS, BRITISH", "VG"),
			new("VIRGIN ISLANDS, U.S.", "VI"),
			new("WALLIS AND FUTUNA", "WF"),
			new("WESTERN SAHARA", "EH"),
			new("YEMEN", "YE"),
			new("ZAMBIA", "ZM"),
			new("ZIMBABWE", "ZW")
		];
	}
}
