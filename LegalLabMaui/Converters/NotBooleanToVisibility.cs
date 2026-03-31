using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts boolean values to IsVisible bool values, including a logical not.
/// </summary>
public class NotBooleanToVisibility : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is bool b)
			return !b;
		else
			return true;
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is bool b)
			return !b;
		else
			return true;
	}
}
