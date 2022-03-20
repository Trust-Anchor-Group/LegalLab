using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content.Xml;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="DateTime"/> values to XML strings.
	/// </summary>
	public class DateTimeToXmlString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime Value)
				return XML.Encode(Value, false);
			else
				return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && XML.TryParse(s, out DateTime TP))
				return TP;
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
