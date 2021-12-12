using LegalLab.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Waher.Content;

namespace LegalLab.Converters
{
	/// <summary>
	/// Converts <see cref="Duration"/> values to strings.
	/// </summary>
	public class DurationToString : MarkupExtension, IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.ToString();
		}

		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && Duration.TryParse(s, out Duration D))
				return D;

			return value;
		}

		/// <inheritdoc/>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}
