using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LegalLab.Converters
{
	/// <summary>
	/// Performs a logical not
	/// </summary>
	public class Not : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
				return !b;
			else
				return true;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
				return !b;
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
