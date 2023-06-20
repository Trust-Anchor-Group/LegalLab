using LegalLab.Extensions;
using LegalLab.Tabs;
using NeuroFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace LegalLab.Models.Tokens.Reports
{
	/// <summary>
	/// Abstract base class for token reports.
	/// </summary>
	public abstract class TokenReport : IDelayedAction
	{
		private readonly string tokenId;
		private ReportTab tab;

		/// <summary>
		/// Refernce to the Neuro-Features client.
		/// </summary>
		protected readonly NeuroFeaturesClient client;

		public TokenReport(NeuroFeaturesClient Client, string TokenId)
		{
			this.client = Client;
			this.tokenId = TokenId;
		}

		/// <summary>
		/// Token ID associated with the state-machine.
		/// </summary>
		public string TokenId => this.tokenId;

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public abstract Task<string> GetTitle();

		/// <summary>
		/// Gets the XAML for the report.
		/// </summary>
		/// <returns>String-representation of XAML of report.</returns>
		public abstract Task<string> GetReportXaml();

		/// <summary>
		/// Parses report XAML
		/// </summary>
		/// <param name="Xaml">String-representation of XAML</param>
		/// <returns>Parsed XAML</returns>
		public UIElement ParseReport(string Xaml)
		{
			List<DynamicImageRef> DynamicImages = new List<DynamicImageRef>();
			StringBuilder sb = new StringBuilder();
			int i = 0;
			int c = Xaml.Length;

			while (i < c)
			{
				Match M = dynamicImage.Match(Xaml, i);
				if (!M.Success)
				{
					sb.Append(Xaml[i..c]);
					i = c;
				}
				else
				{
					sb.Append(Xaml.Substring(i, M.Index - i));
					i = M.Index + M.Length;

					sb.Append("<InlineUIContainer><Image Name=\"DynamicImage");
					sb.Append(DynamicImages.Count.ToString());
					sb.Append("\"/></InlineUIContainer>");

					DynamicImages.Add(new DynamicImageRef()
					{
						Border = M.Groups["Border"].Value,
						Width = M.Groups["Width"].Value,
						Height = M.Groups["Height"].Value,
						ContentType = M.Groups["ContentType"].Value,
						Base64 = M.Groups["Base64"].Value,
						Alt = M.Groups["Alt"].Value
					});
				}
			}

			object Parsed = sb.ToString().ParseSimple();

			if (Parsed is Panel Panel)
			{
				LinkedList<Panel> ToProcess = new LinkedList<Panel>();
				LinkedList<Image> Images = new LinkedList<Image>();
				ToProcess.AddLast(Panel);

				while (!(ToProcess.First is null))
				{
					Panel = ToProcess.First.Value;
					ToProcess.RemoveFirst();

					if (!(Panel.Children is null))
					{
						foreach (UIElement Element in Panel.Children)
						{
							if (Element is Panel Panel2)
								ToProcess.AddLast(Panel2);
							else if (Element is Image Image)
								Images.AddLast(Image);
							else if (Element is TextBlock TextBlock)
							{
								if (!(TextBlock.Inlines is null))
								{
									foreach (Inline Inline in TextBlock.Inlines)
									{
										if (Inline is InlineUIContainer Container &&
											Container.Child is Image Image2)
										{
											Images.AddLast(Image2);
										}
									}
								}
							}
						}
					}
				}

				foreach (Image Image in Images)
				{
					if (Image.Name.StartsWith("DynamicImage") &&
						int.TryParse(Image.Name[12..], out int Nr) &&
						Nr >= 0 && Nr < DynamicImages.Count)
					{
						DynamicImageRef Ref = DynamicImages[Nr];

						if (int.TryParse(Ref.Border, out int Border))
							Image.Margin = new Thickness(Border);

						if (int.TryParse(Ref.Width, out int Width))
							Image.Width = Width;

						if (int.TryParse(Ref.Height, out int Height))
							Image.Height = Height;

						if (!string.IsNullOrEmpty(Ref.Alt))
							Image.ToolTip = Ref.Alt;

						byte[] Bin = Convert.FromBase64String(Ref.Base64);

						using MemoryStream ms = new MemoryStream(Bin);

						BitmapImage BitmapImage = new BitmapImage();
						BitmapImage.BeginInit();
						BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						BitmapImage.StreamSource = ms;
						BitmapImage.EndInit();

						Image.Source = BitmapImage;
					}
				}
			}

			return (UIElement)Parsed;
		}

		private static readonly Regex dynamicImage = new Regex("<!--<img(\\s+((border=[\"'](?'Border'\\d+)[\"'])|(width=[\"'](?'Width'\\d+)[\"'])|(height=[\"'](?'Height'\\d+)[\"'])|(alt=[\"'](?'Alt'[^\"']*)[\"'])|(src=[\"']data:(?'ContentType'\\w+\\/\\w+);base64,(?'Base64'[A-Za-z0-9+-\\/_=]+)[\"'])))*\\s*\\/>-->", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		private class DynamicImageRef
		{
			public string Border;
			public string Width;
			public string Height;
			public string Alt;
			public string ContentType;
			public string Base64;
		}

		/// <summary>
		/// Method called when the underlying state-machine has changed state.
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		public virtual Task OnNewState(object Sender, NewStateEventArgs e)
		{
			DelayedActions.Add(this, DateTime.Now.AddSeconds(1));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Method called when variables in the underlying state-machine have been updated.
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		public virtual Task OnVariablesUpdated(object Sender, VariablesUpdatedEventArgs e)
		{
			DelayedActions.Add(this, DateTime.Now.AddSeconds(1));
			return Task.CompletedTask;
		}

		/// <summary>
		/// When action is scheduled.
		/// </summary>
		public DateTime ScheduledFor { get; set; }

		/// <summary>
		/// Method called when it is time to execute action.
		/// </summary>
		public async Task Action()
		{
			if (!(this.tab is null))
				await this.GenerateReport(this.tab);
		}

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="Tab">Report Tab</param>
		public async Task GenerateReport(ReportTab Tab)
		{
			this.tab = Tab;
			
			string Xaml = await this.GetReportXaml();

			MainWindow.UpdateGui(() =>
			{
				UIElement Report = this.ParseReport(Xaml);

				Tab.ReportPanel.Children.Clear();
				Tab.ReportPanel.Children.Add(Report);

				return Task.CompletedTask;
			});
		}
	}
}
