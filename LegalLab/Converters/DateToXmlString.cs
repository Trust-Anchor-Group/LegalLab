using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content.Xml;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts the date part of a <see cref="DateTime"/> values to XML strings.
	/// </summary>
	public class DateToXmlString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime Value)
				return XML.Encode(Value, true);
			else
				return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && XML.TryParse(s, out DateTime TP))
				return TP.Date;
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
