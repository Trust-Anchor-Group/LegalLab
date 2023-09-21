using LegalLab.Models.Legal;
using System.Windows;

namespace LegalLab.Dialogs.Parameters
{
	/// <summary>
	/// Interaction logic for ParametersDialog.xaml
	/// </summary>
	public partial class ParametersDialog : Window
	{
		private readonly ContractParametersModel model;

		/// <summary>
		/// Interaction logic for ParametersDialog.xaml
		/// </summary>
		/// <param name="Title">Title of dialog</param>
		/// <param name="Model">Model containing parameters to edit.</param>
		public ParametersDialog(string Title, ContractParametersModel Model)
		{
			this.model = Model;

			this.InitializeComponent();
		
			this.Title = Title;
		}
	}
}
