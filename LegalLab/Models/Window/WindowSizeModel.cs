using System;
using System.Threading.Tasks;
using System.Windows;
using Waher.Runtime.Inventory;

namespace LegalLab.Models.Window
{
	/// <summary>
	/// Main window size model
	/// </summary>
	[Singleton]
	public class WindowSizeModel : PersistedModel
	{
		private readonly PersistedProperty<double> width;
		private readonly PersistedProperty<double> height;
		private readonly PersistedProperty<double> left;
		private readonly PersistedProperty<double> top;
		private readonly PersistedProperty<WindowState> state;

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
			this.Add(this.width = new PersistedProperty<double>("MainWindow", "Width", true, Width));
			this.Add(this.height = new PersistedProperty<double>("MainWindow", "Height", true, Height));
			this.Add(this.left = new PersistedProperty<double>("MainWindow", "Left", true, Left));
			this.Add(this.top = new PersistedProperty<double>("MainWindow", "Top", true, Top));
			this.Add(this.state = new PersistedProperty<WindowState>("MainWindow", "State", true, State));
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

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.WindowState = this.State;
				MainWindow.currentInstance.Left = this.Left;
				MainWindow.currentInstance.Top = this.Top;
				MainWindow.currentInstance.Width = this.Width;
				MainWindow.currentInstance.Height = this.Height;
				MainWindow.currentInstance.Visibility = Visibility.Visible;
			});

			return base.Start();
		}
	}
}
