using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts <see cref="TimeSpan"/> values to XML strings.
/// </summary>
public class TimeSpanToXmlString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && TimeSpan.TryParse(s, out TimeSpan TS) && TS >= TimeSpan.Zero && TS.TotalDays == 0)
			return TS;
		else
			return value;
	}
}
