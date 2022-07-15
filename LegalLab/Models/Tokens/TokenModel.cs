using LegalLab.Extensions;
using LegalLab.Models.Items;
using LegalLab.Tabs;
using NeuroFeatures;
using NeuroFeatures.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Contracts;

namespace LegalLab.Models.Tokens
{
	/// <summary>
	/// Token model.
	/// </summary>
	public class TokenModel : SelectableItem
	{
		private readonly NeuroFeaturesClient client;
		private readonly Token token;
		private TokenDetail[] details;
		private BitmapImage glyph;

		private readonly Command viewPresentReport;
		private readonly Command viewHistoryReport;
		private readonly Command viewStateDiagramReport;
		private readonly Command viewProfilingReport;

		/// <summary>
		/// Token model.
		/// </summary>
		/// <param name="Client">Client</param>
		/// <param name="Token">Neuro-Feature token</param>
		private TokenModel(NeuroFeaturesClient Client, Token Token)
		{
			this.client = Client;
			this.token = Token;

			this.viewPresentReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewPresentReport);
			this.viewHistoryReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewHistoryReport);
			this.viewStateDiagramReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewStateDiagramReport);
			this.viewProfilingReport = new Command(this.CanExecuteViewStateMachineReport, this.ExecuteViewProfilingReport);
		}

		/// <summary>
		/// Referenced token.
		/// </summary>
		public Token Token => this.token;

		public static async Task<TokenModel> CreateAsync(NeuroFeaturesClient Client, Token Token)
		{
			TokenModel Result = new TokenModel(Client, Token);

			List<TokenDetail> Details = new List<TokenDetail>()
			{
				new TokenDetail("Token ID", Result.token.TokenId),
				new TokenDetail("Token ID Method", Result.token.TokenIdMethod),
				new TokenDetail("Ordinal", Result.token.Ordinal),
				new TokenDetail("Batch Size", Result.token.BatchSize),
				new TokenDetail("Created", Result.token.Created),
				new TokenDetail("Updated", Result.token.Updated),
				new TokenDetail("Value", Result.token.Value),
				new TokenDetail("Currency", Result.token.Currency),
				new TokenDetail("Expires", Result.token.Expires),
				new TokenDetail("Archivig time (Required)", Result.token.ArchiveRequired),
				new TokenDetail("Archivig time (Optional)", Result.token.ArchiveOptional),
				new TokenDetail("Signature Timestamp", Result.token.SignatureTimestamp),
				new TokenDetail("Signature", Convert.ToBase64String(Result.token.Signature)),
				new TokenDetail("Definition Schema Digest", Convert.ToBase64String(Result.token.DefinitionSchemaDigest)),
				new TokenDetail("Definition Schema Hash Function", Result.token.DefinitionSchemaHashFunction),
				new TokenDetail("Creator Can Destroy", Result.token.CreatorCanDestroy),
				new TokenDetail("Owner Can Destroy Batch", Result.token.OwnerCanDestroyBatch),
				new TokenDetail("Owner Can Destroy Individual", Result.token.OwnerCanDestroyIndividual),
				new TokenDetail("Certifier Can Destroy", Result.token.CertifierCanDestroy),
				new TokenDetail("Friendly Name", Result.token.FriendlyName),
				new TokenDetail("Category", Result.token.Category),
				new TokenDetail("Description", await MarkdownToXaml(Result.token.Description)),
				new TokenDetail("Glyph", Convert.ToBase64String(Result.token.Glyph)),
				new TokenDetail("Glyph Content Type", Result.token.GlyphContentType),
				new TokenDetail("Glyph Width", Result.token.GlyphWidth),
				new TokenDetail("Glyph Height", Result.token.GlyphHeight),
				new TokenDetail("Visibility", Result.token.Visibility),
				new TokenDetail("Creator", Result.token.Creator),
				new TokenDetail("CreatorJid", Result.token.CreatorJid),
				new TokenDetail("Owner", Result.token.Owner),
				new TokenDetail("OwnerJid", Result.token.OwnerJid),
				new TokenDetail("TrustProvider", Result.token.TrustProvider),
				new TokenDetail("TrustProviderJid", Result.token.TrustProviderJid),
				new TokenDetail("Reference", Result.token.Reference),
				new TokenDetail("Definition", Result.token.Definition),
				new TokenDetail("DefinitionNamespace", Result.token.DefinitionNamespace),
				new TokenDetail("CreationContract", Result.token.CreationContract),
				new TokenDetail("OwnershipContract", Result.token.OwnershipContract)
			};

			foreach (string s in Result.token.Witness)
				Details.Add(new TokenDetail("Witness", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.CertifierJids)
				Details.Add(new TokenDetail("CertifierJid", s));

			foreach (string s in Result.token.Certifier)
				Details.Add(new TokenDetail("Certifier", s));

			foreach (string s in Result.token.Valuator)
				Details.Add(new TokenDetail("Valuator", s));

			foreach (string s in Result.token.Assessor)
				Details.Add(new TokenDetail("Assessor", s));

			foreach (TokenTag Tag in Result.token.Tags)
				Details.Add(new TokenDetail(Tag.Name, Tag.Value));

			if (Result.token.HasStateMachine)
			{
				Details.Add(new TokenDetail("State-Machine Present State", new Button()
				{
					Command = Result.viewPresentReport,
					Content = "View Report"
				}));

				Details.Add(new TokenDetail("State-Machine History", new Button()
				{
					Command = Result.viewHistoryReport,
					Content = "View Report"
				}));

				Details.Add(new TokenDetail("State-Machine State Diagram", new Button()
				{
					Command = Result.viewStateDiagramReport,
					Content = "View Report"
				}));

				Details.Add(new TokenDetail("State-Machine Profiling", new Button()
				{
					Command = Result.viewProfilingReport,
					Content = "View Report"
				}));
			}

			Result.details = Details.ToArray();

			return Result;
		}

		private static async Task<object> MarkdownToXaml(string Markdown)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown);
			string Xaml = await Doc.GenerateXAML();
			return Xaml.ParseSimple();
		}

		/// <summary>
		/// Token glyph.
		/// </summary>
		public BitmapImage Glyph
		{
			get
			{
				if (this.glyph is null && !(this.token.Glyph is null))
				{
					BitmapImage Result = new BitmapImage();
					Result.BeginInit();
					Result.CreateOptions = BitmapCreateOptions.None;
					Result.CacheOption = BitmapCacheOption.Default;
					Result.StreamSource = new MemoryStream(this.token.Glyph);
					Result.EndInit();

					this.glyph = Result;
				}

				return this.glyph;
			}
		}

		/// <summary>
		/// Friendly Name of token
		/// </summary>
		public string FriendlyName => this.token.FriendlyName;

		/// <summary>
		/// Category of token
		/// </summary>
		public string Category => this.token.Category;

		/// <summary>
		/// Description of token
		/// </summary>
		public string Description => this.token.Description;

		/// <summary>
		/// ID of token
		/// </summary>
		public string TokenId => this.token.TokenId;

		/// <summary>
		/// When token expires
		/// </summary>
		public DateTime Expires => this.token.Expires;

		/// <summary>
		/// Current value of token
		/// </summary>
		public decimal Value => this.token.Value;

		/// <summary>
		/// Currency
		/// </summary>
		public string Currency => this.token.Currency;

		/// <summary>
		/// When Token was last updated (or created)
		/// </summary>
		public DateTime Updated => this.token.Updated;

		/// <summary>
		/// Creator of token
		/// </summary>
		public string Creator => this.token.Creator;

		/// <summary>
		/// Visibility of token
		/// </summary>
		public ContractVisibility Visibility => this.token.Visibility;

		/// <summary>
		/// Token details.
		/// </summary>
		public TokenDetail[] Details => this.details;

		private bool CanExecuteViewStateMachineReport()
		{
			return this.token.HasStateMachine;
		}

		private async Task ExecuteViewPresentReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
			{
				MainWindow.MouseHourglass();
				try
				{
					this.AddReport(
						await this.client.GeneratePresentReportAsync(this.TokenId, ReportFormat.Xaml),
						"Present");
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
				finally
				{
					MainWindow.MouseDefault();
				}
			}
		}

		private async Task ExecuteViewHistoryReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
			{
				MainWindow.MouseHourglass();
				try
				{
					this.AddReport(
						await this.client.GenerateHistoryReportAsync(this.TokenId, ReportFormat.Xaml),
						"History");
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
				finally
				{
					MainWindow.MouseDefault();
				}
			}
		}

		private async Task ExecuteViewStateDiagramReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
			{
				MainWindow.MouseHourglass();
				try
				{
					this.AddReport(
						await this.client.GenerateStateDiagramAsync(this.TokenId, ReportFormat.Xaml),
						"State Diagram");
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
				finally
				{
					MainWindow.MouseDefault();
				}
			}
		}

		private async Task ExecuteViewProfilingReport()
		{
			if (!string.IsNullOrEmpty(this.TokenId))
			{
				MainWindow.MouseHourglass();
				try
				{
					this.AddReport(
						await this.client.GenerateProfilingReportAsync(this.TokenId, ReportFormat.Xaml),
						"Profiling");
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
				}
				finally
				{
					MainWindow.MouseDefault();
				}
			}
		}

		private class DynamicImageRef
		{
			public string Border;
			public string Width;
			public string Height;
			public string Alt;
			public string ContentType;
			public string Base64;
		}

		private void AddReport(ReportEventArgs e, string Title)
		{
			if (e.Ok)
			{
				List<DynamicImageRef> DynamicImages = new List<DynamicImageRef>();
				string Xaml = e.ReportText;
				StringBuilder sb = new StringBuilder();
				int i = 0;
				int c = Xaml.Length;

				while (i < c)
				{
					Match M = dynamicImage.Match(Xaml, i);
					if (!M.Success)
					{
						sb.Append(Xaml.Substring(i, c - i));
						i = c;
					}
					else
					{
						sb.Append(Xaml.Substring(i, M.Index));
						i = M.Index + M.Length;

						sb.Append("<Image Name=\"DynamicImage");
						sb.Append(DynamicImages.Count.ToString());
						sb.Append("\"/>");

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

				if (Parsed is IContentHost Host)
				{
					LinkedList<IContentHost> ToProcess = new LinkedList<IContentHost>();
					LinkedList<Image> Images = new LinkedList<Image>();
					ToProcess.AddLast(Host);

					while (!(ToProcess.First is null))
					{
						Host = ToProcess.First.Value;
						ToProcess.RemoveFirst();

						IEnumerator<IInputElement> e2 = Host.HostedElements;
						while (e2.MoveNext())
						{
							if (e2.Current is IContentHost Host2)
								ToProcess.AddLast(Host2);
							else if (e2.Current is Image Image)
								Images.AddLast(Image);
						}
					}

					foreach (Image Image in Images)
					{
						if (Image.Name.StartsWith("DynamicImage") &&
							int.TryParse(Image.Name.Substring(12), out int Nr) &&
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

							using (MemoryStream ms = new MemoryStream(Bin))
							{
								BitmapImage BitmapImage = new BitmapImage();
								BitmapImage.BeginInit();
								BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
								BitmapImage.StreamSource = ms;
								BitmapImage.EndInit();

								Image.Source = BitmapImage;
							}
						}
					}

				}

				ReportTab ReportTab = new ReportTab();
				ReportTab.ReportPanel.Children.Add((UIElement)Parsed);

				TabItem Tab = MainWindow.NewTab(Title);
				Tab.Content = ReportTab;

				MainWindow.currentInstance.TabControl.Items.Add(Tab);
				MainWindow.currentInstance.TabControl.SelectedItem = Tab;
			}
			else
				MainWindow.ErrorBox(e.ErrorText);
		}

		private static readonly Regex dynamicImage = new Regex("<!--<img(\\s+((border=[\"'](?'Border'\\d+)[\"'])|(width=[\"'](?'Width'\\d+)[\"'])|(height=[\"'](?'Height'\\d+)[\"'])|(alt=[\"'](?'Alt'[^\"']*)[\"'])|(src=[\"']data:(?'ContentType'\\w+\\/\\w+);base64,(?'Base64'[A-Za-z0-9+-\\/_=]+)[\"'])))*\\s*\\/>-->", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

	}
}
