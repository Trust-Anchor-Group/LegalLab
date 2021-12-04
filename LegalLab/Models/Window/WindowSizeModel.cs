using System;
using System.Windows;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Window
{
	/// <summary>
	/// Main window size model
	/// </summary>
	[Singleton]
	public class WindowSizeModel : PersistantModel
	{
		private readonly PersistantProperty<double> width;
		private readonly PersistantProperty<double> height;
		private readonly PersistantProperty<double> left;
		private readonly PersistantProperty<double> top;
		private readonly PersistantProperty<WindowState> state;

		/// <summary>
		/// Main window size model
		/// </summary>
		public WindowSizeModel()
			: this(WindowState.Normal, 0, 0, 0, 0)
		{
		}

		/// <summary>
		/// Main window size model
		/// </summary>
		/// <param name="State">Window State</param>
		/// <param name="Left">Left coordinate</param>
		/// <param name="Top">Top coordinate</param>
		/// <param name="Width">Width of window</param>
		/// <param name="Height">Height of window</param>
		public WindowSizeModel(WindowState State, double Left, double Top, double Width, double Height)
			: base()
		{
			this.Add(this.width = new PersistantProperty<double>("MainWindow", "Width", true, Width));
			this.Add(this.height = new PersistantProperty<double>("MainWindow", "Height", true, Height));
			this.Add(this.left = new PersistantProperty<double>("MainWindow", "Left", true, Left));
			this.Add(this.top = new PersistantProperty<double>("MainWindow", "Top", true, Top));
			this.Add(this.state = new PersistantProperty<WindowState>("MainWindow", "State", true, State));
		}

		/// <summary>
		/// Width of main window
		/// </summary>
		public double Width
		{
			get => this.width.Value;
			set => this.width.Value = value;
		}

		/// <summary>
		/// Height of main window
		/// </summary>
		public double Height
		{
			get => this.height.Value;
			set => this.height.Value = value;
		}

		/// <summary>
		/// Left of main window
		/// </summary>
		public double Left
		{
			get => this.left.Value;
			set => this.left.Value = value;
		}

		/// <summary>
		/// Top of main window
		/// </summary>
		public double Top
		{
			get => this.top.Value;
			set => this.top.Value = value;
		}

		/// <summary>
		/// Window State
		/// </summary>
		public WindowState State
		{
			get => this.state.Value;
			set => this.state.Value = value;
		}
	}
}
