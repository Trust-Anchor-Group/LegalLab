using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts values to strings.
	/// </summary>
	public class MoneyToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ToString(value);
		}

		/// <summary>
		/// Converts a monetary value to a string, removing any round-off errors.
		/// </summary>
		/// <param name="Value">Monetary value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(object Value)
		{
			if (Value is decimal Money)
				return ToString(Money);
			else if (Value is double d)
				return ToString((decimal)d);
			else
				return Value?.ToString() ?? string.Empty;
		}

		/// <summary>
		/// Converts a monetary value to a string, removing any round-off errors.
		/// </summary>
		/// <param name="Money">Monetary value</param>
		/// <returns>String representation of value.</returns>
		public static string ToString(decimal Money)
		{
			string s = Money.ToString("F9");
			int c = s.Length;
			while (c > 0 && s[c - 1] == '0')
				c--;

			s = s[..c];

			if (s.EndsWith(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
				s = s[..(c - NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Length)];

			return s;
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
