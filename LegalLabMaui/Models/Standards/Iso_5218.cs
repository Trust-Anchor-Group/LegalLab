using System.Collections.Generic;

namespace LegalLabMaui.Models.Standards
{
	/// <summary>
	/// Static class containing ISO 5218 gender codes
	/// </summary>
	public static class Iso_5218
	{
		public class Record(string Gender, int Code, string LegalId)
		{
			public string Gender { get; set; } = Gender;
			public int Code { get; set; } = Code;
			public string LegalId { get; set; } = LegalId;
		}

		private static Dictionary<int, string>? genderByCode;

		public static bool CodeToGender(int Code, out string Gender)
		{
			if (genderByCode is null)
			{
				Dictionary<int, string> Temp = [];
				foreach (Record Rec in Data)
					Temp[Rec.Code] = Rec.Gender;
				genderByCode = Temp;
			}
			if (genderByCode.TryGetValue(Code, out string? gender))
			{
				Gender = gender;
				return true;
			}

			Gender = string.Empty;
			return false;
		}

		public static readonly Record[] Data =
		[
			new("Male", 1, "M"),
			new("Female", 2, "F"),
			new("Other", 9, "X")
		];
	}
}
