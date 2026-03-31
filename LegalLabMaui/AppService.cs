using LegalLabMaui.Models;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Events;
using LegalLabMaui.Models.Legal;
using LegalLabMaui.Models.Network;
using LegalLabMaui.Models.Script;
using LegalLabMaui.Models.Wallet;
using LegalLabMaui.Models.Window;
using LegalLabMaui.Models.XmlEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Runtime.Inventory;

namespace LegalLabMaui;

/// <summary>
/// Static application service - replaces MainWindow static methods from WPF version.
/// </summary>
public static class AppService
{
    private static readonly LinkedList<TaskCompletionSource<bool>> waitingForDb = new();
    private static bool dbReady = false;
    private static WindowSizeModel windowSizeModel = null!;
    private static NetworkModel networkModel = null!;
    private static DesignModel designModel = null!;
    private static ScriptModel scriptModel = null!;
    private static XmlEditorModel xmlEditorModel = null!;

    /// <summary>
    /// Window/navigation size model
    /// </summary>
    public static WindowSizeModel WindowSizeModel => windowSizeModel;

    /// <summary>
    /// Network model (XMPP connection)
    /// </summary>
    public static NetworkModel NetworkModel => networkModel;

    /// <summary>
    /// Design model (Smart contracts)
    /// </summary>
    public static DesignModel DesignModel => designModel;

    /// <summary>
    /// Legal model (identities and smart contracts), or null if not connected.
    /// </summary>
    public static LegalModel? LegalModel => networkModel?.Legal;

    /// <summary>
    /// Wallet model (eDaler), or null if not connected.
    /// </summary>
    public static WalletModel? WalletModel => networkModel?.Wallet;

    /// <summary>
    /// Script model (script execution).
    /// </summary>
    public static ScriptModel ScriptModel => scriptModel;

    /// <summary>
    /// XML editor model.
    /// </summary>
    public static XmlEditorModel XmlEditorModel => xmlEditorModel;

    /// <summary>
    /// Event sink (observable application event log).
    /// </summary>
    public static ObservableEventSink? EventSink => App.EventSink;

    /// <summary>
    /// Executes a callback on the main UI thread.
    /// </summary>
    public static Task UpdateGui(Func<Task> Method)
    {
        if (MainThread.IsMainThread)
            return Method();
        else
            return MainThread.InvokeOnMainThreadAsync(Method);
    }

    /// <summary>
    /// Executes a parameterised callback on the main UI thread.
    /// </summary>
    public static Task UpdateGui(Func<object, Task> Method, object State)
    {
        if (MainThread.IsMainThread)
            return Method(State);
        else
            return MainThread.InvokeOnMainThreadAsync(() => Method(State));
    }

    /// <summary>
    /// Signals that the database is ready and resolves any waiting tasks.
    /// </summary>
    public static void SetDbReady()
    {
        TaskCompletionSource<bool>[] waiting;
        lock (waitingForDb)
        {
            dbReady = true;
            waiting = [.. waitingForDb];
            waitingForDb.Clear();
        }

        foreach (TaskCompletionSource<bool> tcs in waiting)
            tcs.TrySetResult(true);
    }

    /// <summary>
    /// Waits for the database to be ready.
    /// </summary>
    public static Task WaitForDB()
    {
        lock (waitingForDb)
        {
            if (dbReady || Database.HasProvider)
                return Task.CompletedTask;

            TaskCompletionSource<bool> tcs = new();
            waitingForDb.AddLast(tcs);
            return tcs.Task;
        }
    }

    /// <summary>
    /// Instantiates a view model.
    /// </summary>
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

    /// <summary>
    /// Sets the top-level models once the app is initialised.
    /// </summary>
    internal static void SetModels(WindowSizeModel wsm, NetworkModel nm, DesignModel dm, ScriptModel sm, XmlEditorModel xem)
    {
        scriptModel = sm;
        xmlEditorModel = xem;
        windowSizeModel = wsm;
        networkModel = nm;
        designModel = dm;
    }

    /// <summary>
    /// Displays an error alert on the main thread.
    /// </summary>
    public static void ErrorBox(string ErrorMessage)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        });
    }

    /// <summary>
    /// Displays a success alert on the main thread.
    /// </summary>
    public static void SuccessBox(string Message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Success", Message, "OK");
        });
    }

    /// <summary>
    /// Shows a status message.
    /// </summary>
    public static void ShowStatus(string Message)
    {
        Log.Informational(Message);
    }

    /// <summary>
    /// Displays a message box and returns the result.
    /// </summary>
    public static async Task<bool> MessageBox(string Text, string Caption, bool IsQuestion = false)
    {
        if (Shell.Current != null)
        {
            if (IsQuestion)
                return await Shell.Current.DisplayAlert(Caption, Text, "OK", "Cancel");
            else
            {
                await Shell.Current.DisplayAlert(Caption, Text, "OK");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Prompts the user for input.
    /// </summary>
    public static async Task<string?> PromptUser(string Title, string Label, string DefaultValue = "",
        string OkButton = "OK", string CancelButton = "Cancel")
    {
        if (Shell.Current != null)
            return await Shell.Current.DisplayPromptAsync(Title, Label, OkButton, CancelButton, null, -1, null, DefaultValue);

        return null;
    }

    /// <summary>
    /// Opens a URL in the device browser.
    /// </summary>
    public static void OpenUrl(Uri Url)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await Browser.OpenAsync(Url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        });
    }

    /// <summary>
    /// No-op on MAUI (no hourglass cursor).
    /// </summary>
    public static void MouseHourglass() { }

    /// <summary>
    /// No-op on MAUI.
    /// </summary>
    public static void MouseDefault() { }
}
