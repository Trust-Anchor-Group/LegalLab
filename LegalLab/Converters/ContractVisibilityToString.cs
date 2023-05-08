using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="ContractVisibility"/> values to strings.
	/// </summary>
	public class ContractVisibilityToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && ContractVisibility.TryParse(s, out ContractVisibility Visibility))
				return Visibility;

			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(System.IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
