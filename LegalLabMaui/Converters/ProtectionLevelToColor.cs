using LegalLabMaui.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;
using Waher.Networking.XMPP.Contracts;

namespace LegalLabMaui.Converters;

/// <summary>
/// Converts <see cref="ProtectionLevel"/> values to background colors.
/// </summary>
public class ProtectionLevelToColor : IValueConverter
{
	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is ProtectionLevel Protection)
			return Protection.DefaultColor();
		else
			return null;
	}

	/// <inheritdoc/>
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value;
	}
}
