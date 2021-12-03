using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts a boolean values to and from <see cref="Visibility"/> values.
	/// </summary>
	public class BooleanToVisibility : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
				return b ? Visibility.Visible : Visibility.Collapsed;
			else
				return Visibility.Visible;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility v)
				return v == Visibility.Visible;
			else
				return true;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
