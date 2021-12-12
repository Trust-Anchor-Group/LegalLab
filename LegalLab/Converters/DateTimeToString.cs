using LegalLab.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="DateTime"/> values to strings.
	/// </summary>
	public class DateTimeToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime Value)
				return Value.ToStringTZ();
			else
				return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s)
			{
				DateTimeKind Kind;

				if (s.EndsWith("z", StringComparison.CurrentCultureIgnoreCase))
				{
					s = s.Substring(0, s.Length - 1);
					Kind = DateTimeKind.Utc;
				}
				else
					Kind = DateTimeKind.Local;

				int i = s.IndexOf(',');
				if (i < 0)
				{
					if (DateTime.TryParse(s.Trim(), out DateTime TP))
						return new DateTime(TP.Ticks, Kind);
				}
				else
				{
					if (DateTime.TryParse(s.Substring(0, i).Trim(), out DateTime TP) &&
						TimeSpan.TryParse(s.Substring(i + 1).Trim(), out TimeSpan TS))
						return new DateTime((TP + TS).Ticks, Kind);
					else if (DateTime.TryParse(s, out TP))
						return new DateTime(TP.Ticks, Kind);
				}
			}

			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
