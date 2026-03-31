using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts binary data to a BASE64-encoded string.
/// </summary>
public class ToBase64 : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is byte[] Bin)
			return System.Convert.ToBase64String(Bin);
		else
			return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value;
	}
}
