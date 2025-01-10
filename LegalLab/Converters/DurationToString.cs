using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="Duration"/> values to strings.
	/// </summary>
	public class DurationToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && Duration.TryParse(s, out Duration D))
				return D;

			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		/// <summary>
		/// Converts a Duration to a human-readable text.
		/// </summary>
		/// <param name="Duration">Duration to convert.</param>
		/// <returns>Human-readable text.</returns>
		public static string ToString(Duration Duration)
		{
			StringBuilder sb = new();
			bool First = true;

			if (Duration.Negation)
				sb.Append("Negative ");

			Append(sb, Duration.Years, ref First, "Year", "Years");
			Append(sb, Duration.Months, ref First, "Month", "Months");
			Append(sb, Duration.Days, ref First, "Day", "Days");
			Append(sb, Duration.Hours, ref First, "Hour", "Hours");
			Append(sb, Duration.Minutes, ref First, "Minute", "Minutes");
			Append(sb, Duration.Seconds, ref First, "Second", "Seconds");

			if (First)
				sb.Append('-');

			return sb.ToString();
		}

		private static void Append(StringBuilder sb, int Nr, ref bool First, string SingularUnit, string PluralUnit)
		{
			if (Nr != 0)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				sb.Append(Nr);
				sb.Append(' ');

				if (Nr == 1)
					sb.Append(SingularUnit);
				else
					sb.Append(PluralUnit);
			}
		}

		private static void Append(StringBuilder sb, double Nr, ref bool First, string SingularUnit, string PluralUnit)
		{
			if (Nr != 0)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				sb.Append(Nr);
				sb.Append(' ');

				if (Nr == 1)
					sb.Append(SingularUnit);
				else
					sb.Append(PluralUnit);
			}
		}
	}
}
