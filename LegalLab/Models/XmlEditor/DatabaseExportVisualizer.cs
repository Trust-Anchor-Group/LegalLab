using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Wpf;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.Text;
using Waher.Script;

namespace LegalLab.Models.XmlEditor
{
	/// <summary>
	/// Visualizes changes to the database export XML file, by comparing the old and new 
	/// versions of the file, and generating a Markdown report of the differences.
	/// </summary>
	public class DatabaseExportVisualizer : IXmlVisualizer
	{
		private static string lastXml = null;
		private static string[] lastXmlRows = null;
		private static object lastVisualization = null;

		/// <summary>
		/// Visualizes changes to the database export XML file, by comparing the old and new 
		/// versions of the file, and generating a Markdown report of the differences.
		/// </summary>
		public DatabaseExportVisualizer()
		{
		}

		/// <summary>
		/// How well the visualizer handles the provided XML document. 
		/// </summary>
		/// <param name="Object">XML document to visualize</param>
		/// <returns>How well the visualizer supports the document.</returns>
		public Grade Supports(XmlDocument Doc)
		{
			if (Doc.DocumentElement.LocalName == "Database" &&
				string.IsNullOrEmpty(Doc.DocumentElement.NamespaceURI))
			{
				return Grade.Ok;
			}
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Transforms the XML document before visualizing it.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Variables">Current variables.</param>
		/// <returns>Transformed object.</returns>
		public async Task<object> TransformXml(XmlDocument Xml, Variables Variables)
		{
			string s = Xml.OuterXml;

			if (s == lastXml)
				return lastVisualization;

			StringBuilder Markdown = new();
			string[] Rows = Difference.ExtractRows(s);

			Markdown.AppendLine("# Database export");
			Markdown.AppendLine();

			Markdown.Append("Number of rows: ");
			Markdown.AppendLine(Rows.Length.ToString());
			Markdown.AppendLine();

			if (string.IsNullOrEmpty(lastXml))
			{
				Markdown.AppendLine("Next database export will display changes since this export.");

				Log.Informational("Database export visualized for the first time.");
			}
			else
			{
				try
				{
					EditScript<string> Script = Difference.Analyze(lastXmlRows, Rows);
					int NrAdditions = 0;
					int NrRemovals = 0;
					string Prefix;

					Markdown.AppendLine("## Changes");
					Markdown.AppendLine();

					foreach (Step<string> Step in Script.Steps)
					{
						Markdown.AppendLine();

						switch (Step.Operation)
						{
							case EditOperation.Insert:
								Prefix = "+>\t";
								NrAdditions++;
								break;

							case EditOperation.Delete:
								Prefix = "->\t";
								NrRemovals++;
								break;

							default:
								continue;
						}

						Markdown.Append(Prefix);
						Markdown.AppendLine("```xml");

						foreach (string Row in Step.Symbols)
						{
							Markdown.Append(Prefix);
							Markdown.AppendLine(Row);
						}

						Markdown.Append(Prefix);
						Markdown.AppendLine("```");
					}

					Log.Informational("Database export visualized.",
						new KeyValuePair<string, object>("NrAdditions", NrAdditions),
						new KeyValuePair<string, object>("NrRemovals", NrRemovals));
				}
				catch (OutOfMemoryException)
				{
					Markdown.AppendLine("Unable to analyze differences between database exports, due to memory limitations.");

					Log.Informational("Unable to analyze differences in database exports due to memory limitations.");
				}
			}

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString());
			string Xaml = await Doc.GenerateXAML();

			await MainWindow.UpdateGui(() =>
			{
				lastVisualization = XamlReader.Parse(Xaml);
				lastXmlRows = Rows;
				lastXml = s;

				return Task.CompletedTask;
			});
			
			return lastVisualization;
		}

	}
}
