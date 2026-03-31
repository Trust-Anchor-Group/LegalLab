using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts <see cref="ContractVisibility"/> values to strings.
/// </summary>
public class ContractVisibilityToString : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value?.ToString();
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string s && Enum.TryParse(s, out ContractVisibility Visibility))
			return Visibility;

		return value;
	}
}
