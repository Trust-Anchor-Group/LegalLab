using LegalLab.Models.Design;
using System;
using System.Windows.Controls;
using System.Windows.Documents;
using Waher.Runtime.Inventory;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for DesignTab.xaml
	/// </summary>
	public partial class DesignTab : UserControl
	{
		public DesignTab()
		{
			InitializeComponent();
		}

		private void BingTranslationKey_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			DesignModel Model = Types.InstantiateDefault<DesignModel>(false);
			Model.BingTranslatorKey = this.BingTranslationKey.Password;
		}

		private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (sender is Hyperlink Link)
				MainWindow.OpenUrl(Link.NavigateUri);
		}
	}
}
