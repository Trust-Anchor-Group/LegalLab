using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Waher.Content.Xml;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts <see cref="DateTime"/> values to XML strings.
/// </summary>
public class DateTimeToXmlString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is DateTime Value)
			return XML.Encode(Value, false);
		else
			return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && XML.TryParse(s, out DateTime TP))
			return TP;
		else
			return value;
	}
}
