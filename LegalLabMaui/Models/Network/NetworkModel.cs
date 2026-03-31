using LegalLabMaui.Models.Legal;
using LegalLabMaui.Models.Network.Sniffer;
using LegalLabMaui.Models.Tokens;
using LegalLabMaui.Models.Wallet;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Waher.Events;
using Waher.Networking.DNS;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Runtime.Inventory;
using Waher.Runtime.Settings;

namespace LegalLabMaui.Models.Network;

[Singleton]
public class NetworkModel : PersistedModel
{
    private readonly PersistedProperty<string> savedAccounts;
    private readonly PersistedProperty<string> selectedAccount;
    private readonly PersistedProperty<string> xmppServer;
    private readonly PersistedProperty<string> account;
    private readonly PersistedProperty<string> password;
    private readonly PersistedProperty<string> passwordMethod;
    private readonly PersistedProperty<string> apiKey;
    private readonly PersistedProperty<string> apiKeySecret;
    private readonly PersistedProperty<string> legalComponentJid;
    private readonly PersistedProperty<string> eDalerComponentJid;
    private readonly PersistedProperty<string> neuroFeaturesComponentJid;
    private readonly PersistedProperty<string> httpFileUploadComponentJid;
    private readonly PersistedProperty<long> httpFileUploadMaxSize;
    private readonly PersistedProperty<bool> createAccount;
    private readonly PersistedProperty<bool> trustServerCertificate;
    private readonly PersistedProperty<bool> allowInsecureAlgorithms;
    private readonly PersistedProperty<bool> storePasswordInsteadOfDigest;
    private readonly PersistedProperty<bool> connectOnStartup;
    private readonly Property<string> password2;
    private readonly Property<XmppState> state;
    private readonly Property<bool> connected;

    private readonly Command connect;
    private readonly Command disconnect;
    private readonly Command randomizePassword;
    private readonly ParametrizedCommand copySnifferItem;
    private readonly ParametrizedCommand removeSnifferItem;
    private readonly Command clearSniffer;
    private readonly Command saveCredentials;
    private readonly Command deleteCredentials;
    private readonly Command newAccount;

    private XmppClient? client;
    private LegalModel? legalModel;
    private WalletModel? walletModel;
    private TokensModel? tokensModel;
    private ObservableSniffer? sniffer;
    private bool loading = false;

    public NetworkModel() : base()
    {
        this.loading = true;
        try
        {
            this.Add(this.savedAccounts = new PersistedProperty<string>("Accounts", nameof(this.SavedAccounts), true, string.Empty, this));
            this.Add(this.selectedAccount = new PersistedProperty<string>("Accounts", nameof(this.SelectedAccount), true, string.Empty, this));
            this.Add(this.xmppServer = new PersistedProperty<string>("XMPP", nameof(this.XmppServer), false, string.Empty, this));
            this.Add(this.account = new PersistedProperty<string>("XMPP", nameof(this.Account), false, string.Empty, this));
            this.Add(this.password = new PersistedProperty<string>("XMPP", nameof(this.Password), false, string.Empty, this));
            this.Add(this.passwordMethod = new PersistedProperty<string>("XMPP", nameof(this.PasswordMethod), false, string.Empty, this));
            this.Add(this.apiKey = new PersistedProperty<string>("XMPP", nameof(this.ApiKey), false, string.Empty, this));
            this.Add(this.apiKeySecret = new PersistedProperty<string>("XMPP", nameof(this.ApiKeySecret), false, string.Empty, this));
            this.Add(this.createAccount = new PersistedProperty<bool>("XMPP", nameof(this.CreateAccount), false, false, this));
            this.Add(this.trustServerCertificate = new PersistedProperty<bool>("XMPP", nameof(this.TrustServerCertificate), false, false, this));
            this.Add(this.allowInsecureAlgorithms = new PersistedProperty<bool>("XMPP", nameof(this.AllowInsecureAlgorithms), false, false, this));
            this.Add(this.storePasswordInsteadOfDigest = new PersistedProperty<bool>("XMPP", nameof(this.StorePasswordInsteadOfDigest), false, false, this));
            this.Add(this.connectOnStartup = new PersistedProperty<bool>("XMPP", nameof(this.ConnectOnStartup), false, false, this));
            this.Add(this.legalComponentJid = new PersistedProperty<string>("XMPP", nameof(this.LegalComponentJid), true, string.Empty, this));
            this.Add(this.eDalerComponentJid = new PersistedProperty<string>("XMPP", nameof(this.EDalerComponentJid), true, string.Empty, this));
            this.Add(this.neuroFeaturesComponentJid = new PersistedProperty<string>("XMPP", nameof(this.NeuroFeaturesComponentJid), true, string.Empty, this));
            this.Add(this.httpFileUploadComponentJid = new PersistedProperty<string>("XMPP", nameof(this.HttpFileUploadComponentJid), true, string.Empty, this));
            this.Add(this.httpFileUploadMaxSize = new PersistedProperty<long>("XMPP", nameof(this.HttpFileUploadMaxSize), true, 0L, this));

            this.password2 = new Property<string>(nameof(this.Password2), string.Empty, this);
            this.state = new Property<XmppState>(nameof(this.State), XmppState.Offline, this);
            this.connected = new Property<bool>(nameof(this.Connected), false, this);

            this.connect = new Command(this.CanExecuteConnect, this.ExecuteConnect);
            this.disconnect = new Command(this.CanExecuteDisconnect, this.ExecuteDisconnect);
            this.randomizePassword = new Command(this.CanExecuteRandomizePassword, this.ExecuteRandomizePassword);
            this.copySnifferItem = new ParametrizedCommand(this.CanExecuteCopy, this.ExecuteCopy);
            this.removeSnifferItem = new ParametrizedCommand(this.CanExecuteRemove, this.ExecuteRemove);
            this.clearSniffer = new Command(this.CanExecuteClearAll, this.ExecuteClearAll);
            this.saveCredentials = new Command(this.CanExecuteSaveCredentials, this.ExecuteSaveCredentials);
            this.deleteCredentials = new Command(this.CanExecuteDeleteCredentials, this.ExecuteDeleteCredentials);
            this.newAccount = new Command(this.ExecuteNewAccount);
        }
        finally
        {
            this.loading = false;
        }
    }

    // ── Properties ──────────────────────────────────────────────────────────

    public string[] SavedAccounts
    {
        get
        {
            string s = this.savedAccounts.Value;
            return string.IsNullOrEmpty(s) ? [] : s.Split('|');
        }
        set
        {
            StringBuilder sb = new();
            bool first = true;
            foreach (string item in value)
            {
                if (first) first = false;
                else sb.Append('|');
                sb.Append(item);
            }
            this.savedAccounts.Value = sb.ToString();
        }
    }

    public string SelectedAccount
    {
        get => this.selectedAccount.Value;
        set
        {
            this.selectedAccount.Value = value;
            this.deleteCredentials.RaiseCanExecuteChanged();
            if (!this.loading && !string.IsNullOrEmpty(value))
                Task.Run(() => AppService.UpdateGui(async () => await this.LoadCredentials(value)));
        }
    }

    private async Task LoadCredentials(string Account)
    {
        int i = Account.IndexOf('@');
        if (i < 0) return;

        string Prefix = "Credentials." + Account + ".";
        bool wasConnected = this.Connected;

        this.Account = Account[..i];
        this.XmppServer = Account[(i + 1)..];
        this.Password = await RuntimeSettings.GetAsync(Prefix + "Password", string.Empty);
        this.PasswordMethod = await RuntimeSettings.GetAsync(Prefix + "PasswordMethod", string.Empty);
        this.ApiKey = await RuntimeSettings.GetAsync(Prefix + "ApiKey", string.Empty);
        this.ApiKeySecret = await RuntimeSettings.GetAsync(Prefix + "ApiKeySecret", string.Empty);
        this.LegalComponentJid = await RuntimeSettings.GetAsync(Prefix + "LegalComponentJid", string.Empty);
        this.EDalerComponentJid = await RuntimeSettings.GetAsync(Prefix + "EDalerComponentJid", string.Empty);
        this.NeuroFeaturesComponentJid = await RuntimeSettings.GetAsync(Prefix + "NeuroFeaturesComponentJid", string.Empty);
        this.HttpFileUploadComponentJid = await RuntimeSettings.GetAsync(Prefix + "HttpFileUploadComponentJid", string.Empty);
        this.HttpFileUploadMaxSize = await RuntimeSettings.GetAsync(Prefix + "HttpFileUploadMaxSize", 0L);
        this.TrustServerCertificate = await RuntimeSettings.GetAsync(Prefix + "TrustServerCertificate", false);
        this.AllowInsecureAlgorithms = await RuntimeSettings.GetAsync(Prefix + "AllowInsecureAlgorithms", false);
        this.StorePasswordInsteadOfDigest = await RuntimeSettings.GetAsync(Prefix + "StorePasswordInsteadOfDigest", false);
        this.ConnectOnStartup = await RuntimeSettings.GetAsync(Prefix + "ConnectOnStartup", false);
        this.CreateAccount = false;
        this.Password2 = string.Empty;

        await this.ExecuteDisconnect();
        await this.ExecuteClearAll();

        if (wasConnected) await this.ExecuteConnect();
    }

    public string XmppServer { get => this.xmppServer.Value; set => this.xmppServer.Value = value; }
    public string Account { get => this.account.Value; set => this.account.Value = value; }

    public string Password
    {
        get => this.password.Value;
        set
        {
            if (this.password.Value != value)
            {
                this.password.Value = value;
                this.PasswordMethod = string.Empty;
            }
        }
    }

    public string PasswordMethod { get => this.passwordMethod.Value; set => this.passwordMethod.Value = value; }
    public string ApiKey { get => this.apiKey.Value; set => this.apiKey.Value = value; }
    public string ApiKeySecret { get => this.apiKeySecret.Value; set => this.apiKeySecret.Value = value; }

    public bool CreateAccount
    {
        get => this.createAccount.Value;
        set
        {
            this.createAccount.Value = value;
            this.connect.RaiseCanExecuteChanged();
            this.randomizePassword.RaiseCanExecuteChanged();
        }
    }

    public bool TrustServerCertificate { get => this.trustServerCertificate.Value; set => this.trustServerCertificate.Value = value; }
    public bool AllowInsecureAlgorithms { get => this.allowInsecureAlgorithms.Value; set => this.allowInsecureAlgorithms.Value = value; }
    public bool StorePasswordInsteadOfDigest { get => this.storePasswordInsteadOfDigest.Value; set => this.storePasswordInsteadOfDigest.Value = value; }
    public bool ConnectOnStartup { get => this.connectOnStartup.Value; set => this.connectOnStartup.Value = value; }
    public string LegalComponentJid { get => this.legalComponentJid.Value; set => this.legalComponentJid.Value = value; }
    public string EDalerComponentJid { get => this.eDalerComponentJid.Value; set => this.eDalerComponentJid.Value = value; }
    public string NeuroFeaturesComponentJid { get => this.neuroFeaturesComponentJid.Value; set => this.neuroFeaturesComponentJid.Value = value; }
    public string HttpFileUploadComponentJid { get => this.httpFileUploadComponentJid.Value; set => this.httpFileUploadComponentJid.Value = value; }
    public long HttpFileUploadMaxSize { get => this.httpFileUploadMaxSize.Value; set => this.httpFileUploadMaxSize.Value = value; }

    public string Password2
    {
        get => this.password2.Value;
        set { this.password2.Value = value; this.connect.RaiseCanExecuteChanged(); }
    }

    public XmppState State
    {
        get => this.state.Value;
        set
        {
            this.state.Value = value;
            this.connect.RaiseCanExecuteChanged();
            this.disconnect.RaiseCanExecuteChanged();
        }
    }

    public bool Connected
    {
        get => this.connected.Value;
        set
        {
            this.connected.Value = value;
            this.connect.RaiseCanExecuteChanged();
            this.disconnect.RaiseCanExecuteChanged();
            this.randomizePassword.RaiseCanExecuteChanged();
            this.saveCredentials.RaiseCanExecuteChanged();
        }
    }

    public LegalModel? Legal => this.legalModel;
    public WalletModel? Wallet => this.walletModel;
    public TokensModel? Tokens => this.tokensModel;
    public ObservableSniffer? Sniffer => this.sniffer;
    public ObservableCollection<SniffItem> SnifferItems => this.sniffer?.Items ?? [];

    // ── Commands ─────────────────────────────────────────────────────────────

    public ICommand Connect => this.connect;
    public ICommand Disconnect => this.disconnect;
    public ICommand RandomizePassword => this.randomizePassword;
    public ICommand CopySnifferItem => this.copySnifferItem;
    public ICommand RemoveSnifferItem => this.removeSnifferItem;
    public ICommand ClearSniffer => this.clearSniffer;
    public ICommand SaveCredentials => this.saveCredentials;
    public ICommand DeleteCredentials => this.deleteCredentials;
    public ICommand NewAccount => this.newAccount;

    // ── Start / Stop ─────────────────────────────────────────────────────────

    public override async Task Start()
    {
        this.sniffer = new ObservableSniffer(1000);
        this.sniffer.SelectionChanged += this.Sniffer_SelectionChanged;
        this.RaisePropertyChanged(nameof(this.SnifferItems));

        if (this.ConnectOnStartup)
            await this.ExecuteConnect();

        await base.Start();
    }

    public override async Task Stop()
    {
        if (this.legalModel is not null) { await this.legalModel.Stop(); this.legalModel.Dispose(); this.legalModel = null; }
        if (this.walletModel is not null) { await this.walletModel.Stop(); this.walletModel.Dispose(); this.walletModel = null; }
        if (this.tokensModel is not null) { await this.tokensModel.Stop(); this.tokensModel.Dispose(); this.tokensModel = null; }
        if (this.client is not null) { await this.client.DisposeAsync(); this.client = null; }
        await base.Stop();
    }

    // ── Connection ───────────────────────────────────────────────────────────

    private bool CanExecuteConnect() => this.client is null && (!this.CreateAccount || this.Password == this.Password2);
    private bool CanExecuteDisconnect() => this.client is not null;
    private bool CanExecuteRandomizePassword() => this.client is null && this.CreateAccount;

    public async Task ExecuteConnect()
    {
        try
        {
            AppService.MouseHourglass();
            string host;
            int port;

            try
            {
                SRV record = await DnsResolver.LookupServiceEndpoint(this.XmppServer, "xmpp-client", "tcp");
                host = record.TargetHost;
                port = record.Port;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                host = this.XmppServer;
                port = 5222;
            }

            if (this.legalModel is not null) { await this.legalModel.Stop(); this.legalModel.Dispose(); this.legalModel = null; }
            if (this.walletModel is not null) { await this.walletModel.Stop(); this.walletModel.Dispose(); this.walletModel = null; }

            if (string.IsNullOrEmpty(this.PasswordMethod))
                this.client = new XmppClient(host, port, this.Account, this.Password, "en", typeof(App).Assembly, this.sniffer);
            else
                this.client = new XmppClient(host, port, this.Account, this.Password, this.PasswordMethod, "en", typeof(App).Assembly, this.sniffer);

            this.client.DefaultRetryTimeout = 15000;
            this.client.DefaultMaxRetryTimeout = 15000;

            if (this.CreateAccount) this.client.AllowRegistration(this.ApiKey, this.ApiKeySecret);

            this.client.TrustServer = this.TrustServerCertificate;
            this.client.AllowEncryption = true;

            if (!this.AllowInsecureAlgorithms)
            {
                this.client.AllowCramMD5 = false;
                this.client.AllowDigestMD5 = false;
                this.client.AllowPlain = false;
                this.client.AllowScramSHA1 = false;
            }

            this.client.OnStateChanged += this.Client_OnStateChanged;
            this.client.OnConnectionError += this.Client_OnConnectionError;
            this.client.OnChatMessage += this.Client_OnChatMessage;

            await this.client.Connect(this.XmppServer);
        }
        catch (Exception ex)
        {
            AppService.MouseDefault();
            AppService.ErrorBox("Unable to connect to the XMPP network. Error reported: " + ex.Message);
        }
    }

    private Task Client_OnChatMessage(object Sender, Waher.Networking.XMPP.Events.MessageEventArgs e)
    {
        AppService.UpdateGui(async () =>
            await AppService.MessageBox(e.Body, "Message from " + e.From));
        return Task.CompletedTask;
    }

    private async Task Client_OnConnectionError(object Sender, Exception Exception)
    {
        if (this.CreateAccount || !this.ConnectOnStartup)
        {
            if (this.client is not null) { await this.client.DisposeAsync(); this.client = null; }
        }
        this.connect.RaiseCanExecuteChanged();
        AppService.ErrorBox(Exception.Message);
    }

    private async Task Client_OnStateChanged(object Sender, XmppState NewState)
    {
        try
        {
            this.State = NewState;

            switch (NewState)
            {
                case XmppState.Connected:
                    this.Connected = true;
                    AppService.MouseDefault();
                    this.client!.OnConnectionError -= this.Client_OnConnectionError;
                    this.ConnectOnStartup = true;
                    this.CreateAccount = false;
                    this.ApiKey = string.Empty;
                    this.ApiKeySecret = string.Empty;

                    if (string.IsNullOrEmpty(this.PasswordMethod) && !this.StorePasswordInsteadOfDigest)
                    {
                        this.Password = this.client.PasswordHash;
                        this.PasswordMethod = this.client.PasswordHashMethod;
                    }

                    await this.Save();

                    await this.EnsureSubModels();
                    break;

                case XmppState.Error:
                case XmppState.Offline:
                    this.Connected = false;
                    break;
            }

            await this.OnStateChanged.Raise(this, NewState);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }

    private async Task EnsureSubModels()
    {
        if (string.IsNullOrEmpty(this.LegalComponentJid) ||
            string.IsNullOrEmpty(this.EDalerComponentJid) ||
            string.IsNullOrEmpty(this.NeuroFeaturesComponentJid) ||
            string.IsNullOrEmpty(this.HttpFileUploadComponentJid))
        {
            ServiceItemsDiscoveryEventArgs e = await this.client!.ServiceItemsDiscoveryAsync(string.Empty);
            if (e.Ok)
            {
                foreach (Item component in e.Items)
                {
                    ServiceDiscoveryEventArgs e2 = await this.client.ServiceDiscoveryAsync(component.JID);

                    if (e2.HasAnyFeature(ContractsClient.NamespacesLegalIdentities) &&
                        e2.HasAnyFeature(ContractsClient.NamespacesSmartContracts))
                        this.LegalComponentJid = component.JID;

                    if (e2.HasFeature(EDaler.EDalerClient.NamespaceEDaler))
                        this.EDalerComponentJid = component.JID;

                    if (e2.HasFeature(NeuroFeatures.NeuroFeaturesClient.NamespaceNeuroFeatures))
                        this.NeuroFeaturesComponentJid = component.JID;

                    if (e2.HasFeature(HttpFileUploadClient.Namespace))
                    {
                        this.HttpFileUploadComponentJid = component.JID;
                        this.HttpFileUploadMaxSize = HttpFileUploadClient.FindMaxFileSize(this.client, e2) ?? 0;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(this.LegalComponentJid))
        {
            if (this.legalModel is null || this.legalModel.Contracts.ComponentAddress != this.LegalComponentJid)
            {
                this.legalModel?.Dispose();
                this.legalModel = null;
                this.legalModel = new LegalModel(this.client!, this.LegalComponentJid,
                    this.HttpFileUploadComponentJid,
                    this.HttpFileUploadMaxSize <= 0 ? null : this.HttpFileUploadMaxSize);
                await this.legalModel.Load();
                await this.legalModel.Start();
            }

            if (!string.IsNullOrEmpty(this.EDalerComponentJid) &&
                (this.walletModel is null || this.walletModel.EDaler.ComponentAddress != this.EDalerComponentJid))
            {
                this.walletModel?.Dispose();
                this.walletModel = null;
                this.walletModel = new WalletModel(this.client!, this.legalModel!.Contracts, this.EDalerComponentJid, this);
                await this.walletModel.Load();
                await this.walletModel.Start();
            }

            if (!string.IsNullOrEmpty(this.NeuroFeaturesComponentJid) &&
                (this.tokensModel is null || this.tokensModel.NeuroFeaturesClient.ComponentAddress != this.NeuroFeaturesComponentJid))
            {
                this.tokensModel?.Dispose();
                this.tokensModel = null;
                this.tokensModel = new TokensModel(this.client!, this.legalModel!.Contracts, this.NeuroFeaturesComponentJid);
                await this.tokensModel.Load();
                await this.tokensModel.Start();
            }
        }
    }

    public event Waher.Events.EventHandlerAsync<XmppState>? OnStateChanged;

    public async Task ExecuteDisconnect()
    {
        this.LegalComponentJid = string.Empty;
        this.EDalerComponentJid = string.Empty;
        this.NeuroFeaturesComponentJid = string.Empty;

        this.legalModel?.Dispose(); this.legalModel = null;
        this.walletModel?.Dispose(); this.walletModel = null;
        this.tokensModel?.Dispose(); this.tokensModel = null;

        if (this.client is not null) { await this.client.DisposeAsync(); this.client = null; }

        this.State = XmppState.Offline;
        this.Connected = false;
        this.ConnectOnStartup = false;
    }

    // ── Sniffer commands ─────────────────────────────────────────────────────

    private bool CanExecuteCopy(object? Item) => GetSelectedSniffItem(Item) is not null;
    private bool CanExecuteRemove(object? Item) => GetSelectedSniffItem(Item) is not null;
    private bool CanExecuteClearAll() => true;

    private SniffItem? GetSelectedSniffItem(object? item) =>
        item as SniffItem ?? this.sniffer?.Items.FirstOrDefault(x => x.IsSelected);

    private void ExecuteCopy(object? Item)
    {
        if (GetSelectedSniffItem(Item) is SniffItem sniff)
        {
            string text = $"Date:\t{sniff.Timestamp.Date.ToShortDateString()}\n" +
                          $"Time:\t{sniff.Timestamp.ToLongTimeString()}\n" +
                          $"Type:\t{sniff.Type}\n\n{sniff.Message}";
            MainThread.BeginInvokeOnMainThread(async () =>
                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(text));
        }
    }

    private void ExecuteRemove(object? Item)
    {
        if (GetSelectedSniffItem(Item) is SniffItem sniff)
            AppService.UpdateGui(() => { this.sniffer?.Items.Remove(sniff); return Task.CompletedTask; });
    }

    private Task ExecuteClearAll() { this.sniffer?.Clear(); return Task.CompletedTask; }

    private void Sniffer_SelectionChanged(object? sender, EventArgs e)
    {
        this.copySnifferItem.RaiseCanExecuteChanged();
        this.removeSnifferItem.RaiseCanExecuteChanged();
    }

    // ── Save/Delete credentials ───────────────────────────────────────────────

    private bool CanExecuteSaveCredentials() => this.Connected;

    private async Task ExecuteSaveCredentials()
    {
        SortedDictionary<string, bool> sorted = [];
        foreach (string acct in this.SavedAccounts) sorted[acct] = true;

        string jid = this.Account + "@" + this.XmppServer;
        sorted[jid] = true;

        string prefix = "Credentials." + jid + ".";
        await RuntimeSettings.SetAsync(prefix + "Password", this.Password);
        await RuntimeSettings.SetAsync(prefix + "PasswordMethod", this.PasswordMethod);
        await RuntimeSettings.SetAsync(prefix + "ApiKey", this.ApiKey);
        await RuntimeSettings.SetAsync(prefix + "ApiKeySecret", this.ApiKeySecret);
        await RuntimeSettings.SetAsync(prefix + "LegalComponentJid", this.LegalComponentJid);
        await RuntimeSettings.SetAsync(prefix + "EDalerComponentJid", this.EDalerComponentJid);
        await RuntimeSettings.SetAsync(prefix + "NeuroFeaturesComponentJid", this.NeuroFeaturesComponentJid);
        await RuntimeSettings.SetAsync(prefix + "HttpFileUploadComponentJid", this.HttpFileUploadComponentJid);
        await RuntimeSettings.SetAsync(prefix + "HttpFileUploadMaxSize", this.HttpFileUploadMaxSize);
        await RuntimeSettings.SetAsync(prefix + "TrustServerCertificate", this.TrustServerCertificate);
        await RuntimeSettings.SetAsync(prefix + "AllowInsecureAlgorithms", this.AllowInsecureAlgorithms);
        await RuntimeSettings.SetAsync(prefix + "StorePasswordInsteadOfDigest", this.StorePasswordInsteadOfDigest);
        await RuntimeSettings.SetAsync(prefix + "ConnectOnStartup", this.ConnectOnStartup);

        string[] accounts = new string[sorted.Count];
        sorted.Keys.CopyTo(accounts, 0);

        this.loading = true;
        try { this.SavedAccounts = accounts; this.SelectedAccount = jid; }
        finally { this.loading = false; }

        await AppService.MessageBox("Credentials saved.", "Information");
    }

    private bool CanExecuteDeleteCredentials() => !string.IsNullOrEmpty(this.SelectedAccount);

    private async Task ExecuteDeleteCredentials()
    {
        SortedDictionary<string, bool> sorted = [];
        foreach (string acct in this.SavedAccounts)
            if (acct != this.SelectedAccount) sorted[acct] = true;

        string jid = this.Account + "@" + this.XmppServer;
        string prefix = "Credentials." + jid + ".";
        await RuntimeSettings.DeleteAsync(prefix + "Password");
        await RuntimeSettings.DeleteAsync(prefix + "PasswordMethod");
        await RuntimeSettings.DeleteAsync(prefix + "ApiKey");
        await RuntimeSettings.DeleteAsync(prefix + "ApiKeySecret");
        await RuntimeSettings.DeleteAsync(prefix + "LegalComponentJid");
        await RuntimeSettings.DeleteAsync(prefix + "EDalerComponentJid");
        await RuntimeSettings.DeleteAsync(prefix + "NeuroFeaturesComponentJid");
        await RuntimeSettings.DeleteAsync(prefix + "HttpFileUploadComponentJid");
        await RuntimeSettings.DeleteAsync(prefix + "TrustServerCertificate");
        await RuntimeSettings.DeleteAsync(prefix + "AllowInsecureAlgorithms");
        await RuntimeSettings.DeleteAsync(prefix + "StorePasswordInsteadOfDigest");
        await RuntimeSettings.DeleteAsync(prefix + "ConnectOnStartup");

        string[] accounts = new string[sorted.Count];
        sorted.Keys.CopyTo(accounts, 0);

        this.loading = true;
        try { this.SelectedAccount = string.Empty; this.SavedAccounts = accounts; }
        finally { this.loading = false; }

        await AppService.MessageBox("Credentials deleted.", "Information");
    }

    private async Task ExecuteNewAccount()
    {
        await this.ExecuteDisconnect();
        this.SelectedAccount = string.Empty;
        this.XmppServer = string.Empty;
        this.Account = string.Empty;
        this.Password = string.Empty;
        this.PasswordMethod = string.Empty;
        this.ApiKey = string.Empty;
        this.ApiKeySecret = string.Empty;
        this.LegalComponentJid = string.Empty;
        this.EDalerComponentJid = string.Empty;
        this.NeuroFeaturesComponentJid = string.Empty;
        this.TrustServerCertificate = false;
        this.AllowInsecureAlgorithms = false;
        this.StorePasswordInsteadOfDigest = false;
        this.ConnectOnStartup = false;
        this.CreateAccount = false;
        this.Password2 = string.Empty;
        await this.ExecuteClearAll();
    }

    private Task ExecuteRandomizePassword()
    {
        byte[] bin = new byte[28];
        Random.Shared.NextBytes(bin);
        this.Password = Convert.ToBase64String(bin);
        this.Password2 = this.Password;
        return Task.CompletedTask;
    }
}
