using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LegalLab.Models.Design
{
	/// <summary>
	/// Avalon editor that does not capture mouse wheel (scrolling) events.
	/// </summary>
	public class NonScrollingDataGrid : DataGrid
	{
		/// <summary>
		/// Avalon editor that does not capture mouse wheel (scrolling) events.
		/// </summary>
		public NonScrollingDataGrid()
			: base()
		{
		}

		/// <inheritdoc/>
		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (!e.Handled)
			{
				e.Handled = true;
				MouseWheelEventArgs e2 = new(e.MouseDevice, e.Timestamp, e.Delta)
				{
					RoutedEvent = UIElement.MouseWheelEvent,
					Source = this
				};
				UIElement Parent = VisualTreeHelper.GetParent(this) as UIElement;
				
				while (Parent is not null && Parent is not ScrollViewer)
					Parent = VisualTreeHelper.GetParent(Parent) as UIElement;

				Parent?.RaiseEvent(e2);
			}
		}
	}
}
