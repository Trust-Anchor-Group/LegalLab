using LegalLab.Dialogs.Prompt;
using LegalLab.Models;
using LegalLab.Models.Design;
using LegalLab.Models.Network;
using LegalLab.Models.Events;
using LegalLab.Models.Window;
using LegalLab.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;
using Waher.Runtime.Collections;

namespace LegalLab
{
	/// <summary>
	/// Delegate for GUI update methods with parameter.
	/// </summary>
	public delegate Task GuiDelegate();

	/// <summary>
	/// Delegate for GUI update methods with parameter.
	/// </summary>
	/// <param name="Parameter">Parameter</param>
	public delegate Task GuiDelegateWithParameter(object Parameter);

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static MainWindow currentInstance = null;
		private static readonly LinkedList<GuiUpdateTask> guiUpdateQueue = new();
		private static bool uiInitialized = false;
		private static string appDataFolder;
		private static string databaseFolder;
		private static string eventsFolder;
		private static FilesProvider database;
		private static WindowSizeModel windowSizeModel;
		private static NetworkModel networkModel;
		private static DesignModel designModel;

		public MainWindow()
		{
			TaskCompletionSource<bool> Completed = new();
			bool StartGuiTask = false;

			lock (guiUpdateQueue)
			{
				if (currentInstance is null)
				{
					currentInstance = this;
					StartGuiTask = true;
				}
			}

			appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LegalLab");

			// Implementation Inventory. Used by persistence, networking and scripting modules.

			TypesLoader.Initialize();   // Makes an inventory of all assemblies in project.

			this.Visibility = Visibility.Hidden;

			Task.Run(() => this.Initialize(Completed, StartGuiTask));

			this.InitializeComponent();

			Completed.Task.Wait();
		}

		#region Initialization & Setup

		private async Task Initialize(TaskCompletionSource<bool> Completed, bool StartGuiTask)
		{
			try
			{
				// Setting up internal encrypted object Database

				databaseFolder = Path.Combine(appDataFolder, "Data");
				if (!Directory.Exists(databaseFolder))
					Directory.CreateDirectory(databaseFolder);

				database = await FilesProvider.CreateAsync(databaseFolder, "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, this.GetFileEncryptionKeys);
				await database.RepairIfInproperShutdown(string.Empty);

				Database.Register(database);

				TaskCompletionSource<bool>[] WaitingTasks;

				lock (waitingForDb)
				{
					WaitingTasks = [.. waitingForDb];
					waitingForDb.Clear();
				}

				foreach (TaskCompletionSource<bool> Task in WaitingTasks)
					Task.TrySetResult(true);

				// Event logs

				Log.RegisterAlertExceptionType(true,
					typeof(OutOfMemoryException),
					typeof(StackOverflowException),
					typeof(AccessViolationException),
					typeof(InsufficientMemoryException));

				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				eventsFolder = Path.Combine(appDataFolder, "Events");
				if (!Directory.Exists(eventsFolder))
					Directory.CreateDirectory(eventsFolder);

				Log.Register(new PersistedEventLog(90));
				Log.Register(new XmlFileEventSink("XML File Event Log", Path.Combine(eventsFolder, "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml"), 7));
				Log.Register(new ListViewEventSink(this.EventsTab.EventListView, 1000));

				// Modules

				await Types.StartAllModules(10000);

				Completed.TrySetResult(true);

				// View Models

				await this.Dispatcher.BeginInvoke(async () =>
				{
					try
					{
						windowSizeModel = await InstantiateModel<WindowSizeModel>(this.WindowState, this.Left, this.Top, this.Width, this.Height, this.TabControl.SelectedIndex);
						networkModel = await InstantiateModel<NetworkModel>();
						designModel = await InstantiateModel<DesignModel>();
						uiInitialized = true;

						if (StartGuiTask)
							await this.Dispatcher.BeginInvoke(DoUpdates);
					}
					catch (Exception ex)
					{
						ErrorBox(ex.Message);
					}
				});
			}
			catch (Exception ex)
			{
				ErrorBox(ex.Message);
			}
		}

		private static readonly ChunkedList<TaskCompletionSource<bool>> waitingForDb = [];

		/// <summary>
		/// Waits to the database to be configured.
		/// </summary>
		/// <returns></returns>
		public static Task WaitForDB()
		{
			lock (waitingForDb)
			{
				if (Database.HasProvider)
					return Task.CompletedTask;

				TaskCompletionSource<bool> Result = new();
				waitingForDb.Add(Result);

				return Result.Task;
			}
		}

		/// <summary>
		/// Instantiates a view model.
		/// </summary>
		/// <typeparam name="T">Type of view model</typeparam>
		/// <param name="Arguments">Optional list of arguments.</param>
		/// <returns>Instantiated view model.</returns>
		public static async Task<T> InstantiateModel<T>(params object[] Arguments)
			where T : Model
		{
			T Result = Types.InstantiateDefault<T>(false, Arguments);

			if (!Result.Started)
			{
				if (Result is PersistedModel P)
					await P.Load();

				await Result.Start();
			}

			return Result;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Types.StopAllModules().Wait(10000);
			Log.TerminateAsync().Wait(10000);
		}

		/// <summary>
		/// Delegate for custom key callback methods.
		/// </summary>
		/// <param name="FileName">Name of file.</param>
		/// <returns>A pair of (Key, IV).</returns>
		private Task<KeyValuePair<byte[], byte[]>> GetFileEncryptionKeys(string FileName)
		{
			string KeyName = FileName + ".key";
			byte[] Entropy = Encoding.UTF8.GetBytes(FileName);
			byte[] Protected;
			byte[] Data;

			if (File.Exists(KeyName))
				Protected = File.ReadAllBytes(KeyName);
			else
			{
				using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
				Data = new byte[32 + 16];
				Rnd.GetBytes(Data);
				Protected = ProtectedData.Protect(Data, Entropy, DataProtectionScope.CurrentUser);
				File.WriteAllBytes(KeyName, Protected);
			}

			Data = ProtectedData.Unprotect(Protected, Entropy, DataProtectionScope.CurrentUser);

			byte[] Key = new byte[32];
			byte[] IV = new byte[16];

			Array.Copy(Data, 0, Key, 0, 32);
			Array.Copy(Data, 32, IV, 0, 16);

			return Task.FromResult(new KeyValuePair<byte[], byte[]>(Key, IV));
		}

		/// <summary>
		/// Window Size model
		/// </summary>
		public static WindowSizeModel WindowSizeModel => windowSizeModel;

		/// <summary>
		/// Network model
		/// </summary>
		public static NetworkModel NetworkModel => networkModel;

		/// <summary>
		/// Design model
		/// </summary>
		public static DesignModel DesignModel => designModel;

		#endregion

		#region Standard UI

		/// <summary>
		/// Displays an error message on the screen, on the main UI thread.
		/// </summary>
		/// <param name="ErrorMessage">Error message</param>
		public static void ErrorBox(string ErrorMessage)
		{
			MessageBox(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		/// <summary>
		/// Displays a success message on the screen, on the main UI thread.
		/// </summary>
		/// <param name="Message">Success message</param>
		public static void SuccessBox(string Message)
		{
			MessageBox(Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		/// <summary>
		/// Displays a status message on the screen, on the main UI thread.
		/// </summary>
		/// <param name="Message">Status message</param>
		public static void ShowStatus(string Message)
		{
			UpdateGui(() =>
			{
				currentInstance.Status.Content = Message;
				return Task.CompletedTask;
			});
		}

		/// <summary>
		/// Displays a message box on the screen, on the main UI thread.
		/// </summary>
		/// <param name="Text">Text message</param>
		/// <param name="Caption">Caption text</param>
		/// <param name="Button">Button text</param>
		/// <param name="Icon">Icon to display</param>
		/// <returns>Button pressed.</returns>
		public static Task<MessageBoxResult> MessageBox(string Text, string Caption, MessageBoxButton Button, MessageBoxImage Icon)
		{
			TaskCompletionSource<MessageBoxResult> Result = new();

			UpdateGui(() =>
			{
				Mouse.OverrideCursor = null;
				Result.TrySetResult(System.Windows.MessageBox.Show(currentInstance, Text, Caption, Button, Icon));
				return Task.CompletedTask;
			});

			return Result.Task;
		}

		/// <summary>
		/// Prompts the user for input.
		/// </summary>
		/// <param name="Title">Dialog title.</param>
		/// <param name="Label">Label to display to the user.</param>
		/// <param name="Text">Pre-filled input value.</param>
		/// <returns>Text input by the user, null if Cancel has been pressed.</returns>
		public static string PromptUser(string Title, string Label)
		{
			return PromptUser(Title, Label, string.Empty);
		}

		/// <summary>
		/// Prompts the user for input.
		/// </summary>
		/// <param name="Title">Dialog title.</param>
		/// <param name="Label">Label to display to the user.</param>
		/// <param name="Text">Pre-filled input value.</param>
		/// <returns>Text input by the user, null if Cancel has been pressed.</returns>
		public static string PromptUser(string Title, string Label, string Text)
		{
			return PromptUser(Title, Label, Text, "OK", "Cancel");
		}

		/// <summary>
		/// Prompts the user for input.
		/// </summary>
		/// <param name="Title">Dialog title.</param>
		/// <param name="Label">Label to display to the user.</param>
		/// <param name="Text">Pre-filled input value.</param>
		/// <param name="OkButton">Text of the OK button.</param>
		/// <param name="CancelButton">Text of the Cancel button.</param>
		/// <returns>Text input by the user, null if Cancel has been pressed.</returns>
		public static string PromptUser(string Title, string Label, string Text, string OkButton, string CancelButton)
		{
			PromptDialog Dialog = new();
			PromptModel Model = new(Dialog)
			{
				Title = Title,
				Label = Label,
				Text = Text,
				OkText = OkButton,
				CancelText = CancelButton
			};

			bool? Result = Dialog.ShowDialog();
			if (Result.HasValue && Result.Value)
				return Model.Text;
			else
				return null;
		}

		/// <summary>
		/// Returns to default mouse pointer.
		/// </summary>
		public static void MouseDefault()
		{
			UpdateGui(() =>
			{
				Mouse.OverrideCursor = null;
				return Task.CompletedTask;
			});
		}

		/// <summary>
		/// Displays an hourglass cursor (or similar for waiting purposes).
		/// </summary>
		public static void MouseHourglass()
		{
			UpdateGui(() =>
			{
				Mouse.OverrideCursor = Cursors.Wait;
				return Task.CompletedTask;
			});
		}

		/// <summary>
		/// Calls a method from the Main UI thread.
		/// </summary>
		/// <param name="Method">Method to call.</param>
		/// <returns>If update was successful.</returns>
		public static Task<bool> UpdateGui(GuiDelegate Method)
		{
			return UpdateGui((State) => ((GuiDelegate)State)(), Method.Method.DeclaringType + "." + Method.Method.Name, Method);
		}

		/// <summary>
		/// Calls a method from the Main UI thread.
		/// </summary>
		/// <param name="Method">Method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If update was successful.</returns>
		public static Task<bool> UpdateGui(GuiDelegateWithParameter Method, object State)
		{
			return UpdateGui(Method, Method.Method.DeclaringType + "." + Method.Method.Name, State);
		}

		private static async Task<bool> UpdateGui(GuiDelegateWithParameter Method, string Name, object State)
		{
			if (uiInitialized && (currentInstance?.Dispatcher.CheckAccess() ?? false))
			{
				try
				{
					await Method(State);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}

				return true;
			}
			else
			{
				bool Start;
				GuiUpdateTask Rec = new()
				{
					Method = Method,
					State = State,
					Name = Name,
					Requested = DateTime.Now,
				};

				lock (guiUpdateQueue)
				{
					Start = (guiUpdateQueue.First is null) && currentInstance is not null;
					guiUpdateQueue.AddLast(Rec);

					if (!uiInitialized)
						return true;
				}

				if (Start)
					await currentInstance.Dispatcher.BeginInvoke(DoUpdates);

				return await Rec.Done.Task;
			}
		}

		private static async Task DoUpdates()
		{
			try
			{
				GuiUpdateTask Rec = null;
				GuiUpdateTask Prev;

				while (true)
				{
					lock (guiUpdateQueue)
					{
						if (Rec is not null)
							guiUpdateQueue.RemoveFirst();

						Prev = Rec;
						Rec = guiUpdateQueue.First?.Value;
						if (Rec is null)
							return;
					}

					try
					{
						Rec.Started = DateTime.Now;
						await Rec.Method(Rec.State);
						Rec.Done.TrySetResult(true);
					}
					catch (Exception ex)
					{
						Rec.Done.TrySetResult(false);
						Log.Exception(ex);
					}
					finally
					{
						Rec.Ended = DateTime.Now;
					}

					TimeSpan TS;

					if ((TS = Rec.Ended - Rec.Started).TotalSeconds >= 1)
						Log.Notice("GUI update method is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
					else if ((TS = (Rec.Ended - Rec.Requested)).TotalSeconds >= 1)
						Log.Notice("GUI update pipeline is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				lock (guiUpdateQueue)
				{
					guiUpdateQueue.Clear();
				}
			}
		}

		private class GuiUpdateTask
		{
			public TaskCompletionSource<bool> Done = new();
			public GuiDelegateWithParameter Method;
			public object State;
			public string Name;
			public DateTime Requested;
			public DateTime Started;
			public DateTime Ended;
		}

		/// <summary>
		/// Opens an URL in the currently selected browser using the Shell.
		/// </summary>
		/// <param name="Url">URL to open</param>
		public static void OpenUrl(Uri Url)
		{
			try
			{
				Process.Start(new ProcessStartInfo()
				{
					FileName = Url.OriginalString,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

		#region Tab management

		// Note: Copy from IoTGateway WPF Client project, with permission

		internal static TabItem NewTab(string HeaderText)
		{
			return NewTab(HeaderText, out TextBlock _);
		}

		internal static TabItem NewTab(string HeaderText, out TextBlock HeaderLabel)
		{
			TabItem Result = new();
			NewHeader(HeaderText, Result, out HeaderLabel);
			return Result;
		}

		internal static void NewHeader(string HeaderText, TabItem Tab)
		{
			NewHeader(HeaderText, Tab, out TextBlock _);
		}

		internal static void NewHeader(string HeaderText, TabItem Tab, out TextBlock HeaderLabel)
		{
			StackPanel Header = new()
			{
				Orientation = Orientation.Horizontal
			};

			Image CloseImage = new()
			{
				Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative)),
				Width = 16,
				Height = 16,
				ToolTip = "Close tab"
			};

			HeaderLabel = new TextBlock()
			{
				Text = HeaderText,
				Margin = new Thickness(0, 0, 5, 0)
			};

			Header.Children.Add(HeaderLabel);
			Header.Children.Add(CloseImage);

			CloseImage.MouseLeftButtonDown += CloseImage_MouseLeftButtonDown;
			CloseImage.MouseEnter += CloseImage_MouseEnter;
			CloseImage.MouseLeave += CloseImage_MouseLeave;
			CloseImage.Tag = Tab;

			Tab.Header = Header;
		}

		private static void CloseImage_MouseLeave(object sender, MouseEventArgs e)
		{
			if (sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative));
		}

		private static void CloseImage_MouseEnter(object sender, MouseEventArgs e)
		{
			if (sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon.png", UriKind.Relative));
		}

		private static void CloseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is Image { Tag: TabItem Item })
			{
				MainWindow.currentInstance?.TabControl?.Items.Remove(Item);
				if (Item.Content != null && Item.Content is IDisposable Disposable)
					Disposable.Dispose();
			}
		}

		public static void SelectTab(UserControl Tab)
		{
			foreach (TabItem Item in currentInstance.TabControl.Items)
			{
				if (Item.Content == Tab)
				{
					currentInstance.TabControl.SelectedItem = Item;
					break;
				}
			}
		}

		#endregion

		#endregion

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.TabControl.SelectedItem is TabItem TabItem &&
				TabItem.Content is ISelectableTab SelectableTab)
			{
				SelectableTab.Selected();
			}
		}
	}
}
