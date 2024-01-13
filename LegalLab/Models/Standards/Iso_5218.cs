using System.Collections.Generic;

namespace LegalLab.Models.Standards
{
	/// <summary>
	/// Static class containing ISO 5218 gender codes
	/// </summary>
	public static class Iso_5218
	{
		/// <summary>
		/// Contains one record of the ISO 5218 data set.
		/// </summary>
		public class Record
		{
			/// <summary>
			/// Contains one record of the ISO 5218 data set.
			/// </summary>
			/// <param name="Gender">Gender name</param>
			/// <param name="Code">ISO 5218 designated Code</param>
			/// <param name="LegalId">Character used in Legal IDs GENDER property.</param>
			public Record(string Gender, int Code, string LegalId)
			{
				this.Gender = Gender;
				this.Code = Code;
				this.LegalId = LegalId;
			}

			/// <summary>
			/// Gender
			/// </summary>
			public string Gender { get; set; }

			/// <summary>
			/// ISO 5218 gender code
			/// </summary>
			public int Code { get; set; }

			/// <summary>
			/// Character used in Legal IDs GENDER property.
			/// </summary>
			public string LegalId { get; set; }
		}

		private static Dictionary<int, string> genderByCode = null;

		/// <summary>
		/// Tries to get the gender label corresponding to an ISO 5218 gender code.
		/// </summary>
		/// <param name="Code">Gender code</param>
		/// <param name="Gender">Gender, if found.</param>
		/// <returns>If a corresponding gender code was found.</returns>
		public static bool CodeToGender(int Code, out string Gender)
		{
			if (genderByCode is null)
			{
				Dictionary<int, string> Temp = new();

				foreach (Record Rec in Data)
					Temp[Rec.Code] = Rec.Gender;

				genderByCode = Temp;
			}

			return genderByCode.TryGetValue(Code, out Gender);
		}

		/// <summary>
		/// Available gender codes
		/// </summary>
		public static readonly Record[] Data = new Record[]
		{
			new("Male", 1, "M"),
			new("Female", 2, "F"),
			new("Other", 9, "X")
		};
	}
}
