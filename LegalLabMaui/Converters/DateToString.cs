using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts the date part of a <see cref="DateTime"/> value to a short date string.
/// </summary>
public class DateToString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is DateTime Value)
			return Value.ToShortDateString();
		else
			return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && DateTime.TryParse(s, out DateTime TP))
			return TP.Date;
		else
			return value;
	}
}
