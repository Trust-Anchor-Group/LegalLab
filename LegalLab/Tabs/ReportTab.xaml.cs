using LegalLab.Models.Tokens.Reports;
using System.Windows.Controls;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for ReportTab.xaml
	/// </summary>
	public partial class ReportTab : UserControl
	{
		public ReportTab(TokenReport Report)
		{
			this.Report = Report;

			this.InitializeComponent();
		}

		/// <summary>
		/// Reference to underlying report model.
		/// </summary>
		public TokenReport Report { get; }
	}
}
