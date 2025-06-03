using LegalLab.Models.XmlEditor;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using Waher.Events;

namespace LegalLab.Tabs
{
	/// <summary>
	/// Interaction logic for XmlEditorTab.xaml
	/// </summary>
	public partial class XmlEditorTab : UserControl, ISelectableTab
	{
		private XmlEditorModel xmlEditorModel;
		private bool updatingFromModel = false;
		private bool updatingFromEditor = false;

		public XmlEditorTab()
		{
			this.InitializeComponent();
		}

		protected override async void OnInitialized(EventArgs e)
		{
			try
			{
				base.OnInitialized(e);

				this.xmlEditorModel = await MainWindow.InstantiateModel<XmlEditorModel>(this.VisualizationViewer);
				this.xmlEditorModel.PropertyChanged += this.XmlEditorModel_PropertyChanged;

				// ActualWidth does not get updated until after all events have been raised.

				this.EditorPanel.SizeChanged += (s, e) =>
				{
					Task.Delay(100).ContinueWith((_) =>
					{
						MainWindow.UpdateGui(() =>
						{
							this.xmlEditorModel.EditorWidth = this.EditorColumn.ActualWidth;
							return Task.CompletedTask;
						});
					});
				};
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
					if (!this.updatingFromEditor)
					{
						this.updatingFromModel = true;
						try
						{
							this.XmlEditor.Text = this.xmlEditorModel.Xml;
						}
						finally
						{
							this.updatingFromModel = false;
						}
					}
					break;
			}
		}

		private void XmlEditor_TextChanged(object sender, EventArgs e)
		{
			if (!this.updatingFromModel)
			{
				this.updatingFromEditor = true;
				try
				{
					this.xmlEditorModel.Xml = this.XmlEditor.Text;
				}
				finally
				{
					this.updatingFromEditor = false;
				}
			}
		}

		public void Selected()
		{
			this.xmlEditorModel.UpdateVisualization();
		}
	}
}
