using System;
using System.Windows;

namespace LegalLab.Dialogs.AddXmlNote
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class AddXmlNoteDialog : Window
	{
		public AddXmlNoteDialog()
		{
			this.InitializeComponent();
		}

		private void XmlEditor_TextChanged(object sender, EventArgs e)
		{
			if (this.DataContext is AddXmlNoteModel Model)
				Model.XmlInput = this.XmlEditor.Text;
		}
    }
}
