using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace LegalLab.Extensions
{
	/// <summary>
	/// Static class with XAML extensions.
	/// </summary>
	public static class XamlExtensions
	{
		/// <summary>
		/// Parses a string into simple XAML (for inclusion in tables, tooltips, etc.)
		/// </summary>
		/// <param name="Xaml">XAML</param>
		/// <returns>Parsed XAML</returns>
		public static object ParseSimple(this string Xaml)
		{
			if (string.IsNullOrEmpty(Xaml))
				return null;

			object Result = XamlReader.Parse(Xaml);

			if (Result is StackPanel Panel && Panel.Children.Count == 1)
			{
				UIElement Child = Panel.Children[0];
				Panel.Children.RemoveAt(0);

				if (Child is FrameworkElement E)
					E.Margin = new Thickness(0);

				return Child;
			}
			else
				return Result;
		}

		/// <summary>
		/// Converts the human-readable description of a role to simple XAML.
		/// </summary>
		/// <param name="Role">Role</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <returns>Simple XAML</returns>
		public static object ToSimpleXAML(this Role Role, string Language, Contract Contract)
		{
			return Role.ToXAML(Language, Contract).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to simple XAML.
		/// </summary>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <returns>Simple XAML</returns>
		public static object ToSimpleXAML(this Parameter Parameter, string Language, Contract Contract)
		{
			return Parameter.ToXAML(Language, Contract).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to simple XAML.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Contract">Contract</param>
		/// <param name="Language">Language code</param>
		/// <returns>Simple XAML</returns>
		public static object ToSimpleXAML(this string Markdown, Contract Contract, string Language)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			HumanReadableText Text = Markdown.ToHumanReadableText(Language);
			string Xaml = Text.GenerateXAML(Contract);
			return Xaml.ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to XAML.
		/// </summary>
		/// <param name="Markdown">Current set of variables.</param>
		/// <param name="Contract">Contract</param>
		/// <param name="Language">Language code</param>
		/// <returns>XAML</returns>
		public static object ToXAML(this string Markdown, Contract Contract, string Language)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			HumanReadableText Text = Markdown.ToHumanReadableText(Language);
			string Xaml = Text.GenerateXAML(Contract);
			return XamlReader.Parse(Xaml);
		}
	}
}
