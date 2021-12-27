using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="ContractParts"/> values to strings.
	/// </summary>
	public class ContractPartsToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && ContractParts.TryParse(s, out ContractParts Parts))
				return Parts;

			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
