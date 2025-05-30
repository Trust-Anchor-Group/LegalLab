﻿using System;
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
		private readonly PersistedProperty<WindowState> state;
		private readonly PersistedProperty<double> width;
		private readonly PersistedProperty<double> height;
		private readonly PersistedProperty<double> left;
		private readonly PersistedProperty<double> top;
		private readonly PersistedProperty<long> tabIndex;
		private bool skipNextPosition = false;

		/// <summary>
		/// Main window size model
		/// </summary>
		public WindowSizeModel()
			: this(WindowState.Normal, 0, 0, 0, 0, 0)
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
		public WindowSizeModel(WindowState State, double Left, double Top, double Width, double Height, int TabIndex)
			: base()
		{
			this.Add(this.width = new PersistedProperty<double>("MainWindow", nameof(this.Width), true, Width, this));
			this.Add(this.height = new PersistedProperty<double>("MainWindow", nameof(this.Height), true, Height, this));
			this.Add(this.left = new PersistedProperty<double>("MainWindow", nameof(this.Left), true, Left, this));
			this.Add(this.top = new PersistedProperty<double>("MainWindow", nameof(this.Top), true, Top, this));
			this.Add(this.state = new PersistedProperty<WindowState>("MainWindow", nameof(this.State), true, State, this));
			this.Add(this.tabIndex = new PersistedProperty<long>("MainWindow", nameof(this.TabIndex), true, TabIndex, this));
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
		/// Selected tab
		/// </summary>
		public int TabIndex
		{
			get => (int)this.tabIndex.Value;
			set => this.tabIndex.Value = value;
		}

		/// <summary>
		/// Starts the model.
		/// </summary>
		public override Task Start()
		{
			MainWindow.UpdateGui(() =>
			{
				this.skipNextPosition = double.IsNaN(MainWindow.currentInstance.Left);

				MainWindow.currentInstance.WindowState = this.State;

				if (this.Left > -25600)
					MainWindow.currentInstance.Left = this.Left;

				if (this.Top > -25600)
					MainWindow.currentInstance.Top = this.Top;

				MainWindow.currentInstance.Width = this.Width;
				MainWindow.currentInstance.Height = this.Height;
				MainWindow.currentInstance.TabControl.SelectedIndex = this.TabIndex;
				MainWindow.currentInstance.Visibility = Visibility.Visible;

				MainWindow.currentInstance.SizeChanged += this.WindowSizeChanged;
				MainWindow.currentInstance.LocationChanged += this.WindowLocationChanged;
				MainWindow.currentInstance.StateChanged += this.WindowStateChanged;
				MainWindow.currentInstance.TabControl.SelectionChanged += this.TabIndexChanged;

				return Task.CompletedTask;
			});

			return base.Start();
		}

		/// <summary>
		/// Stops the model.
		/// </summary>
		public override Task Stop()
		{
			MainWindow.currentInstance.SizeChanged -= this.WindowSizeChanged;
			MainWindow.currentInstance.LocationChanged -= this.WindowLocationChanged;
			MainWindow.currentInstance.StateChanged -= this.WindowStateChanged;
			MainWindow.currentInstance.TabControl.SelectionChanged -= this.TabIndexChanged;

			return base.Stop();
		}

		private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (MainWindow.currentInstance.WindowState == WindowState.Normal)
			{
				if (e.WidthChanged && e.NewSize.Width > 0)
					this.Width = e.NewSize.Width;

				if (e.HeightChanged && e.NewSize.Height > 0)
					this.Height = e.NewSize.Height;
			}
		}

		private void WindowLocationChanged(object sender, EventArgs e)
		{
			if (MainWindow.currentInstance.WindowState == WindowState.Normal)
			{
				if (this.skipNextPosition)
				{
					if (MainWindow.currentInstance.Left != this.Left)
						MainWindow.currentInstance.Left = this.Left;

					if (MainWindow.currentInstance.Top != this.Top)
						MainWindow.currentInstance.Top = this.Top;
				
					this.skipNextPosition = false;
				}
				else
				{
					if (MainWindow.currentInstance.Left > -25600)
						this.Left = MainWindow.currentInstance.Left;

					if (MainWindow.currentInstance.Top > -25600)
						this.Top = MainWindow.currentInstance.Top;
				}
			}
		}

		private void WindowStateChanged(object sender, EventArgs e)
		{
			this.State = MainWindow.currentInstance.WindowState;
		}

		private void TabIndexChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			this.TabIndex = MainWindow.currentInstance.TabControl.SelectedIndex;
		}
	}
}
