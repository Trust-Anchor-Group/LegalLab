using LegalLab.Models;
using LegalLab.Models.Network;
using LegalLab.Models.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;

namespace LegalLab
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static MainWindow currentInstance = null;
		private static readonly LinkedList<GuiUpdateTask> guiUpdateQueue = new LinkedList<GuiUpdateTask>();
		private static string appDataFolder;
		private static string databaseFolder;
		private static string eventsFolder;
		private static FilesProvider database;
		private static WindowSizeModel windowSizeModel;
		private static NetworkModel networkModel;

		public MainWindow()
		{
			this.Visibility = Visibility.Hidden;
			InitializeComponent();
			Initialize();
		}

		#region Initialization & Setup

		private async void Initialize()
		{
			try
			{
				if (currentInstance is null)
					currentInstance = this;

				appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LegalLab");

				// Implementation Inventory. Used by persistence, networking and scripting modules.

				TypesLoader.Initialize();   // Makes an inventory of all assemblies in project.

				// Setting up internal encrypted object Database

				databaseFolder = Path.Combine(appDataFolder, "Data");
				if (!Directory.Exists(databaseFolder))
					Directory.CreateDirectory(databaseFolder);

				database = await FilesProvider.CreateAsync(databaseFolder, "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, this.GetFileEncryptionKeys);
				await database.RepairIfInproperShutdown(string.Empty);

				Database.Register(database);

				// Event logs

				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				eventsFolder = Path.Combine(appDataFolder, "Events");
				if (!Directory.Exists(eventsFolder))
					Directory.CreateDirectory(eventsFolder);

				Log.Register(new PersistedEventLog(90));
				Log.Register(new XmlFileEventSink("XML File Event Log", Path.Combine(eventsFolder, "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml"), 7));

				// Modules

				await Types.StartAllModules(60000);

				// View Models

				windowSizeModel = await InstantiateModel<WindowSizeModel>(this.WindowState, this.Left, this.Top, this.Width, this.Height, this.TabControl.SelectedIndex);
				networkModel = await InstantiateModel<NetworkModel>();
			}
			catch (Exception ex)
			{
				ErrorBox(ex.Message);
			}
		}

		/// <summary>
		/// Instantiates a view model.
		/// </summary>
		/// <typeparam name="T">Type of view model</typeparam>
		/// <param name="Arguments">Optional list of arguments.</param>
		/// <returns>Instantiated view model.</returns>
		private static async Task<T> InstantiateModel<T>(params object[] Arguments)
			where T : Model
		{
			T Result = Types.InstantiateDefault<T>(false, Arguments);

			if (Result is PersistedModel P)
				await P.Load();

			await Result.Start();

			return Result;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Types.StopAllModules().Wait();
			Log.Terminate();
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
			});
		}

		/// <summary>
		/// Displays a message box on the screen, on the main UI thread.
		/// </summary>
		/// <param name="Text">Text message</param>
		/// <param name="Caption">Caption text</param>
		/// <param name="Button">Button text</param>
		/// <param name="Icon">Icon to display</param>
		public static void MessageBox(string Text, string Caption, MessageBoxButton Button, MessageBoxImage Icon)
		{
			UpdateGui(() =>
			{
				Mouse.OverrideCursor = null;
				System.Windows.MessageBox.Show(currentInstance, Text, Caption, Button, Icon);
			});
		}

		/// <summary>
		/// Returns to default mouse pointer.
		/// </summary>
		public static void MouseDefault()
		{
			UpdateGui(() => Mouse.OverrideCursor = null);
		}

		/// <summary>
		/// Displays an hourglass cursor (or similar for waiting purposes).
		/// </summary>
		public static void MouseHourglass()
		{
			UpdateGui(() => Mouse.OverrideCursor = Cursors.Wait);
		}

		/// <summary>
		/// Calls a method from the Main UI thread.
		/// </summary>
		/// <param name="Method">Method to call.</param>
		public static void UpdateGui(ThreadStart Method)
		{
			UpdateGui((State) => ((ThreadStart)State)(), Method.Method.DeclaringType + "." + Method.Method.Name, Method);
		}

		/// <summary>
		/// Calls a method from the Main UI thread.
		/// </summary>
		/// <param name="Method">Method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public static void UpdateGui(ParameterizedThreadStart Method, object State)
		{
			UpdateGui(Method, Method.Method.DeclaringType + "." + Method.Method.Name, State);
		}

		private static void UpdateGui(ParameterizedThreadStart Method, string Name, object State)
		{
			bool Start;
			GuiUpdateTask Rec = new GuiUpdateTask()
			{
				Method = Method,
				State = State,
				Name = Name,
				Requested = DateTime.Now
			};

			lock (guiUpdateQueue)
			{
				Start = guiUpdateQueue.First is null;
				guiUpdateQueue.AddLast(Rec);
			}

			if (Start)
				currentInstance.Dispatcher.BeginInvoke(new ThreadStart(DoUpdates));
		}

		private static void DoUpdates()
		{
			GuiUpdateTask Rec = null;
			GuiUpdateTask Prev;

			while (true)
			{
				lock (guiUpdateQueue)
				{
					if (!(Rec is null))
						guiUpdateQueue.RemoveFirst();

					Prev = Rec;
					Rec = guiUpdateQueue.First?.Value;
					if (Rec is null)
						return;
				}

				try
				{
					Rec.Started = DateTime.Now;
					Rec.Method(Rec.State);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
				finally
				{
					Rec.Ended = DateTime.Now;
				}

				TimeSpan TS;

				if ((TS = (Rec.Ended - Rec.Started)).TotalSeconds >= 1)
					Log.Notice("GUI update method is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
				else if ((TS = (Rec.Ended - Rec.Requested)).TotalSeconds >= 1)
					Log.Notice("GUI update pipeline is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
			}
		}

		private class GuiUpdateTask
		{
			public ParameterizedThreadStart Method;
			public object State;
			public string Name;
			public DateTime Requested;
			public DateTime Started;
			public DateTime Ended;
		}

		#endregion

	}
}
