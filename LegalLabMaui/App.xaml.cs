using LegalLabMaui.Models;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Events;
using LegalLabMaui.Models.Network;
using LegalLabMaui.Models.Script;
using LegalLabMaui.Models.Window;
using LegalLabMaui.Models.XmlEditor;
using System.Text;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;

namespace LegalLabMaui;

public partial class App : Application
{
    private static FilesProvider? database;
    private static ObservableEventSink? eventSink;

    public App()
    {
        InitializeComponent();

        TypesLoader.Initialize();

        Task.Run(Initialize);
    }

    public static ObservableEventSink? EventSink => eventSink;

    private static async Task Initialize()
    {
        try
        {
            string appDataFolder = FileSystem.AppDataDirectory;

            string databaseFolder = Path.Combine(appDataFolder, "Data");
            if (!Directory.Exists(databaseFolder))
                Directory.CreateDirectory(databaseFolder);

            database = await FilesProvider.CreateAsync(databaseFolder, "Default", 8192, 10000, 8192,
                Encoding.UTF8, 10000, GetFileEncryptionKeys);
            await database.RepairIfInproperShutdown(string.Empty);

            Database.Register(database);
            AppService.SetDbReady();

            Log.RegisterAlertExceptionType(true,
                typeof(OutOfMemoryException),
                typeof(StackOverflowException),
                typeof(AccessViolationException),
                typeof(InsufficientMemoryException));

            Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
            Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

            string eventsFolder = Path.Combine(appDataFolder, "Events");
            if (!Directory.Exists(eventsFolder))
                Directory.CreateDirectory(eventsFolder);

            Log.Register(new PersistedEventLog(90));
            Log.Register(new XmlFileEventSink("XML File Event Log",
                Path.Combine(eventsFolder, "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml"), 7));

            eventSink = new ObservableEventSink(1000);
            Log.Register(eventSink);

            await Types.StartAllModules(10000);

            await AppService.UpdateGui(async () =>
            {
                try
                {
                    WindowSizeModel wsm = await AppService.InstantiateModel<WindowSizeModel>();
                    NetworkModel nm = await AppService.InstantiateModel<NetworkModel>();
                    DesignModel dm = await AppService.InstantiateModel<DesignModel>();
                    ScriptModel sm = await AppService.InstantiateModel<ScriptModel>();
                    XmlEditorModel xem = await AppService.InstantiateModel<XmlEditorModel>();

                    AppService.SetModels(wsm, nm, dm, sm, xem);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    AppService.ErrorBox(ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            AppService.ErrorBox(ex.Message);
        }
    }

    private static async Task<KeyValuePair<byte[], byte[]>> GetFileEncryptionKeys(string FileName)
    {
        string KeyName = "dbkey_" + Path.GetFileNameWithoutExtension(FileName);
        byte[] Key;
        byte[] IV;

        try
        {
            string? keyStr = await SecureStorage.GetAsync(KeyName + "_key");
            string? ivStr = await SecureStorage.GetAsync(KeyName + "_iv");

            if (keyStr != null && ivStr != null)
            {
                Key = Convert.FromBase64String(keyStr);
                IV = Convert.FromBase64String(ivStr);
            }
            else
            {
                Key = new byte[32];
                IV = new byte[16];
                Random.Shared.NextBytes(Key);
                Random.Shared.NextBytes(IV);
                await SecureStorage.SetAsync(KeyName + "_key", Convert.ToBase64String(Key));
                await SecureStorage.SetAsync(KeyName + "_iv", Convert.ToBase64String(IV));
            }
        }
        catch
        {
            // Fallback if SecureStorage is not available
            Key = new byte[32];
            IV = new byte[16];
        }

        return new KeyValuePair<byte[], byte[]>(Key, IV);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new(new AppShell());

        window.Destroying += (s, e) =>
        {
            Types.StopAllModules().Wait(10000);
            Log.TerminateAsync().Wait(10000);
        };

        return window;
    }
}
