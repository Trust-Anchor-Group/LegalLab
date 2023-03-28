using System.Windows.Media;

namespace LegalLab.Models.Items
{
	/// <summary>
	/// Abstract base class for selectable colorable item.
	/// From the IoTGateway project, with permission.
	/// </summary>
	public abstract class ColorableItem : SelectableItem
	{
		private Color foregroundColor;
		private Color backgroundColor;

		/// <summary>
		/// Abstract base class for selectable colorable item.
		/// From the IoTGateway project, with permission.
		/// </summary>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public ColorableItem(Color ForegroundColor, Color BackgroundColor)
		{
			this.foregroundColor = ForegroundColor;
			this.backgroundColor = BackgroundColor;
		}

		/// <summary>
		/// Foreground color
		/// </summary>
		public Color ForegroundColor
		{
			get { return this.foregroundColor; }
			set { this.foregroundColor = value; }
		}

		/// <summary>
		/// Background color
		/// </summary>
		public Color BackgroundColor
		{
			get { return this.backgroundColor; }
			set { this.backgroundColor = value; }
		}
	}
}
