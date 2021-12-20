using LegalLab.Models.Script;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Waher.Events;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for ScriptTab.xaml
	/// </summary>
	public partial class ScriptTab : UserControl
	{
		private static ScriptModel scriptModel;

		public ScriptTab()
		{
			InitializeComponent();
		}

		protected override async void OnInitialized(EventArgs e)
		{
			try
			{
				base.OnInitialized(e);
		
				scriptModel = await MainWindow.InstantiateModel<ScriptModel>(this.HistoryPanel);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void OpenScriptReference(object sender, RoutedEventArgs e)
		{
			if (sender is Hyperlink Link)
				MainWindow.OpenUrl(Link.NavigateUri);
		}

		private void InputPreviewKeyDown(object sender, KeyEventArgs e)
		{
			scriptModel?.InputPreviewKeyDown(e);
		}
	}
}
