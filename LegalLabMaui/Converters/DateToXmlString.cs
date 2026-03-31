using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Waher.Content.Xml;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts the date part of a <see cref="DateTime"/> value to an XML date string.
/// </summary>
public class DateToXmlString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is DateTime Value)
			return XML.Encode(Value, true);
		else
			return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && XML.TryParse(s, out DateTime TP))
			return TP.Date;
		else
			return value;
	}
}
