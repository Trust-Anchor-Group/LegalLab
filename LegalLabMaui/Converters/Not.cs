using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace LegalLabMaui.Converters;

/// <summary>
/// Performs a logical not on a boolean value.
/// </summary>
public class Not : IValueConverter
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
