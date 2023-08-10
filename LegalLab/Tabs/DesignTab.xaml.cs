using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LegalLab.Models.Design;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using Waher.Runtime.Inventory;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for DesignTab.xaml
	/// </summary>
	public partial class DesignTab : UserControl
	{
		private readonly DesignModel model;

		public DesignTab()
		{
			this.model = Types.InstantiateDefault<DesignModel>(false);
			this.model.PropertyChanged += this.Model_PropertyChanged;

			// Register AvalonEdit Syntax Highlighting for Smart Contract Markdown documents:

			byte[] Bin = Waher.Content.Resources.LoadResource(typeof(DesignModel).Namespace + ".AvalonExtensions.SmartContractMarkdown.xshd", typeof(App).Assembly);
			using MemoryStream ms = new(Bin);
			using XmlReader r = XmlReader.Create(ms);
			IHighlightingDefinition Def = HighlightingLoader.Load(r, HighlightingManager.Instance);
			HighlightingManager.Instance.RegisterHighlighting(Def.Name, Array.Empty<string>(), Def);

			this.InitializeComponent();
		}

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(DesignModel.HumanReadableMarkdown):
					string Markdown = this.model.HumanReadableMarkdown;
					if (Markdown != this.HumanReadableMarkdownEditor.Text)
						this.HumanReadableMarkdownEditor.Text = Markdown;
					break;

				case nameof(DesignModel.MachineReadable):
					string Xml = this.model.MachineReadable;
					if (Xml != this.MachineReadableXmlEditor.Text)
						this.MachineReadableXmlEditor.Text = Xml;
					break;
			}
		}

		private void OpenAiKey_PasswordChanged(object sender, RoutedEventArgs e)
		{
			this.model.OpenAiKey = this.OpenAiKey.Password;
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Hyperlink Link)
				MainWindow.OpenUrl(Link.NavigateUri);
		}

		private void HumanReadableMarkdownEditor_TextChanged(object sender, EventArgs e)
		{
			this.model.HumanReadableMarkdown = this.HumanReadableMarkdownEditor.Text;
		}

		private void MachineReadableXmlEditor_TextChanged(object sender, EventArgs e)
		{
			this.model.MachineReadable = this.MachineReadableXmlEditor.Text;
		}
	}
}
