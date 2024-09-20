using LegalLab.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="ProtectionLevel"/> values to background colors.
	/// </summary>
	public class ProtectionLevelToBrush : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ProtectionLevel Protection)
				return Protection.DefaultBrush();
			else
				return null;
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(System.IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
