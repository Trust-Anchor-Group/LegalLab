using LegalLab.Tabs;
using NeuroFeatures;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Waher.Script;

namespace LegalLab.Models.Tokens.Reports
{
	/// <summary>
	/// Represent the embedded layout of a token.
	/// </summary>
	public class TokenEmbeddedLayout(NeuroFeaturesClient Client, Token Token)
		: TokenReport(Client, Token.TokenId)
	{
		private readonly Token token = Token;

		/// <summary>
		/// Gets the title of report.
		/// </summary>
		/// <returns>Title</returns>
		public override Task<string> GetTitle() => Task.FromResult("Layout");

		/// <summary>
		/// Generates the report.
		/// </summary>
		/// <param name="Tab">Report Tab</param>
		public override async Task GenerateReport(ReportTab Tab)
		{
			this.tab = Tab;
		
			Variables Variables = [];
			using SKImage Image = await this.token.RenderEmbeddedLayout(Variables);
			using SKData Data = Image.Encode(SKEncodedImageFormat.Png, 100);
			byte[] Bin = Data.ToArray();

			await MainWindow.UpdateGui(() =>
			{
				BitmapImage BitmapImage;

				using (MemoryStream ms = new(Bin))
				{
					BitmapImage = new BitmapImage();
					BitmapImage.BeginInit();
					BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					BitmapImage.StreamSource = ms;
					BitmapImage.EndInit();
				}

				Image ImageBlock = new()
				{
					Source = BitmapImage,
					Width = BitmapImage.Width,
					Height = BitmapImage.Height
				};

				Tab.UpdateReport(ImageBlock);
				return Task.CompletedTask;
			});
		}

		/// <summary>
		/// Gets the XAML for the report.
		/// </summary>
		/// <returns>String-representation of XAML of report.</returns>
		public override Task<string> GetReportXaml()
		{
			return Task.FromResult(string.Empty);
		}
	}
}
