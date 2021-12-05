using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

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
			if (value is DateTime TP)
			{
				if (TP == DateTime.MinValue || TP == DateTime.MaxValue)
					return string.Empty;
				else
				{
					string s = TP.ToShortDateString() + ", " + TP.ToLongTimeString();

					if (TP.Kind == DateTimeKind.Utc)
						s += "Z";

					return s;
				}
			}
			else
				return value.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
