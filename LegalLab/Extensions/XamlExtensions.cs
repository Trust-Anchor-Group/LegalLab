using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Wpf;
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
		/// Creates a human-readable XAML document for the contract.
		/// </summary>
		/// <param name="Contract">Contract reference.</param>
		/// <param name="Language">Desired language</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Markdown</returns>
		public static Task<string> ToXAML(this Contract Contract, string Language,
			string AdditionalMarkdown = null)
		{
			return Contract.ToXAML(Contract.ForHumans, Language, AdditionalMarkdown);
		}

		/// <summary>
		/// Selects a human-readable text, and generates a XAML document from it.
		/// </summary>
		/// <param name="Contract">Contract reference.</param>
		/// <param name="Text">Collection of texts in different languages.</param>
		/// <param name="Language">Language</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>XAML document.</returns>
		public static Task<string> ToXAML(this Contract Contract, HumanReadableText[] Text, string Language,
			string AdditionalMarkdown = null)
		{
			return Contract.Select(Text, Language)?.GenerateXAML(Contract, AdditionalMarkdown) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Selects a human-readable text, and generates a XAML document from it.
		/// </summary>
		/// <param name="Contract">Contract reference.</param>
		/// <param name="Text">Collection of labels in different languages.</param>
		/// <param name="Language">Language</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>XAML document.</returns>
		public static Task<string> ToXAML(this Contract Contract, Waher.Networking.XMPP.Contracts.HumanReadable.Label[] Text,
			string Language, string AdditionalMarkdown = null)
		{
			return Contract.Select(Text, Language)?.GenerateXAML(Contract, AdditionalMarkdown) ?? Task.FromResult<string>(null);
		}

		/// <summary>
		/// Generates XAML for the human-readable text.
		/// </summary>
		/// <param name="Text">Human-readable text being rendered.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>XAML</returns>
		public static async Task<string> GenerateXAML(this HumanReadableText Text, Contract Contract,
			string AdditionalMarkdown = null)
		{
			MarkdownDocument Doc = await Text.GenerateMarkdownDocument(Contract);
			Doc = await Doc.Append(AdditionalMarkdown);
			return await Doc.GenerateXAML();
		}

		/// <summary>
		/// Appends Markdown to a parsed Markdown document.
		/// </summary>
		/// <param name="Markdown">Parsed Markdown document.</param>
		/// <param name="AdditionalMarkdown">Additional Markdown to append.</param>
		/// <returns>Appended Markdown document.</returns>
		public static async Task<MarkdownDocument> Append(this MarkdownDocument Markdown, string AdditionalMarkdown = null)
		{
			if (!string.IsNullOrEmpty(AdditionalMarkdown))
			{
				StringBuilder sb = new StringBuilder();
				await Markdown.GenerateMarkdown(sb);

				sb.AppendLine();
				sb.AppendLine(AdditionalMarkdown);

				Markdown = await MarkdownDocument.CreateAsync(sb.ToString());
			}

			return Markdown;
		}

		/// <summary>
		/// Generates XAML for the human-readable text.
		/// </summary>
		/// <param name="Text">Human-readable text being rendered.</param>
		/// <param name="Contract">Contract, of which the human-readable text is part.</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>XAML</returns>
		public static async Task<string> GenerateXAML(this Waher.Networking.XMPP.Contracts.HumanReadable.Label Text,
			Contract Contract, string AdditionalMarkdown = null)
		{
			MarkdownDocument Doc = await Text.GenerateMarkdownDocument(Contract);
			Doc = await Doc.Append(AdditionalMarkdown);
			return await Doc.GenerateXAML();
		}

		/// <summary>
		/// Creates a human-readable XAML document for the contract.
		/// </summary>
		/// <param name="Description">Description being rendered.</param>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Markdown</returns>
		public static Task<string> ToXAML(this LocalizableDescription Description, string Language,
			Contract Contract, string AdditionalMarkdown = null)
		{
			return Contract.ToXAML(Description.Descriptions, Language, AdditionalMarkdown);
		}

		/// <summary>
		/// Converts the human-readable description of a role to simple XAML.
		/// </summary>
		/// <param name="Role">Role</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Simple XAML</returns>
		public static async Task<object> ToSimpleXAML(this Role Role, string Language,
			Contract Contract, string AdditionalMarkdown = null)
		{
			return (await Role.ToXAML(Language, Contract, AdditionalMarkdown)).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to simple XAML.
		/// </summary>
		/// <param name="Parameter">Parameter</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Simple XAML</returns>
		public static async Task<object> ToSimpleXAML(this Parameter Parameter, string Language,
			Contract Contract, string AdditionalMarkdown = null)
		{
			return (await Parameter.ToXAML(Language, Contract, AdditionalMarkdown)).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to simple XAML.
		/// </summary>
		/// <param name="Text">Human-readable description.</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Simple XAML</returns>
		public static async Task<object> ToSimpleXAML(this HumanReadableText[] Text, string Language,
			Contract Contract, string AdditionalMarkdown = null)
		{
			return (await Contract.ToXAML(Text, Language, AdditionalMarkdown)).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable label of a parameter to simple XAML.
		/// </summary>
		/// <param name="Label">Human-readable label.</param>
		/// <param name="Language">Language</param>
		/// <param name="Contract">Contract</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Simple XAML</returns>
		public static async Task<object> ToSimpleXAML(this Waher.Networking.XMPP.Contracts.HumanReadable.Label[] Label, string Language,
			Contract Contract, string AdditionalMarkdown = null)
		{
			return (await Contract.ToXAML(Label, Language, AdditionalMarkdown)).ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to simple XAML.
		/// </summary>
		/// <param name="Markdown">Markdown text</param>
		/// <param name="Contract">Contract</param>
		/// <param name="Language">Language code</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>Simple XAML</returns>
		public static async Task<object> ToSimpleXAML(this string Markdown, Contract Contract,
			string Language, string AdditionalMarkdown = null)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			HumanReadableText Text = await Markdown.ToHumanReadableText(Language);
			string Xaml = await Text.GenerateXAML(Contract, AdditionalMarkdown);
			return Xaml.ParseSimple();
		}

		/// <summary>
		/// Converts the human-readable description of a parameter to XAML.
		/// </summary>
		/// <param name="Markdown">Current set of variables.</param>
		/// <param name="Contract">Contract</param>
		/// <param name="Language">Language code</param>
		/// <param name="AdditionalMarkdown">Optional additional Markdown to add to the end.</param>
		/// <returns>XAML</returns>
		public static async Task<object> ToXAML(this string Markdown, Contract Contract,
			string Language, string AdditionalMarkdown = null)
		{
			if (string.IsNullOrEmpty(Markdown))
				return null;

			HumanReadableText Text = await Markdown.ToHumanReadableText(Language);
			string Xaml = await Text.GenerateXAML(Contract, AdditionalMarkdown);
			return XamlReader.Parse(Xaml);
		}
	}
}
