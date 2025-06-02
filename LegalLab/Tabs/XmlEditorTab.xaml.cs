using LegalLab.Models.XmlEditor;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Waher.Events;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for XmlEditorTab.xaml
	/// </summary>
	public partial class XmlEditorTab : UserControl
	{
		private XmlEditorModel xmlEditorModel;

		public XmlEditorTab()
		{
			this.InitializeComponent();
		}

		protected override async void OnInitialized(EventArgs e)
		{
			try
			{
				base.OnInitialized(e);
		
				this.xmlEditorModel = await MainWindow.InstantiateModel<XmlEditorModel>();
				this.xmlEditorModel.PropertyChanged += this.XmlEditorModel_PropertyChanged;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void XmlEditorModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(XmlEditorModel.Xml):
					this.XmlEditor.Text = this.xmlEditorModel.Xml;
					break;
			}
		}

		private void XmlEditor_TextChanged(object sender, EventArgs e)
		{
			this.xmlEditorModel.Xml = this.XmlEditor.Text;
		}

		private void InputPreviewKeyDown(object sender, KeyEventArgs e)
		{
		}
	}
}
