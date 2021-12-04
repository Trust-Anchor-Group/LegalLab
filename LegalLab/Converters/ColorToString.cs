using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts color values to and from <see cref="Visibility"/> values.
	/// </summary>
	public class ColorToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Colors.Transparent;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
