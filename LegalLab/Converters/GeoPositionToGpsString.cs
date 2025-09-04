using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Runtime.Geo;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="GeoPosition"/> values to XML strings.
	/// </summary>
	public class GeoPositionToGpsString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is GeoPosition Position)
				return Position.HumanReadable;
			else
				return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && GeoPosition.TryParse(s, out GeoPosition Position))
				return Position;
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
