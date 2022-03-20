using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="Duration"/> values to XML strings.
	/// </summary>
	public class DurationToXmlString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && Duration.TryParse(s, out Duration TS))
				return TS;
			else
				return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
