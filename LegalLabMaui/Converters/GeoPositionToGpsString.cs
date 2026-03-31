using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Waher.Runtime.Geo;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts <see cref="GeoPosition"/> values to GPS strings.
/// </summary>
public class GeoPositionToGpsString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is GeoPosition Position)
			return Position.HumanReadable;
		else
			return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && GeoPosition.TryParse(s, out GeoPosition Position))
			return Position;
		else
			return value;
	}
}
