using LegalLab.Models.Tokens.Reports;
using System;
using System.Windows;
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

			this.ReportPanel.LayoutUpdated += this.ReportPanel_LayoutUpdated;
		}

		/// <summary>
		/// Reference to underlying report model.
		/// </summary>
		public TokenReport Report { get; }

		/// <summary>
		/// Updates the report displayed in the tab.
		/// </summary>
		/// <param name="Report">Parsed report</param>
		public void UpdateReport(UIElement Report)
		{
			this.ReportPanel.Children.Clear();
			this.ReportPanel.Children.Add(Report);

			this.ReportPanel.UpdateLayout();
		}

		private void ReportPanel_LayoutUpdated(object sender, EventArgs e)
		{
			double AvailableWidth = this.ReportScrollView.ViewportWidth;
			double ContentWidth = this.ReportPanel.ActualWidth;

			if (AvailableWidth > 0 && ContentWidth > 0)
			{
				double Scale = Math.Min(1.0, AvailableWidth / ContentWidth);

				if (Scale < this.ReportScaleTransform.ScaleX)
				{
					this.ReportScaleTransform.ScaleX = Scale;
					this.ReportScaleTransform.ScaleY = Scale;
				}
			}
		}
	}
}
