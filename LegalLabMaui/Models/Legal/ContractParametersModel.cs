using LegalLabMaui.Extensions;
using LegalLabMaui.Models.Design;
using LegalLabMaui.Models.Legal.Items;
using LegalLabMaui.Models.Legal.Items.Parameters;
using LegalParamInfo = LegalLabMaui.Models.Legal.Items.ParameterInfo;
using LegalLabMaui.Models.Standards;
using System.Text;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Runtime.Geo;
using Waher.Script;

namespace LegalLabMaui.Models.Legal;

public class ContractParametersModel : Model
{
    private static readonly Contract emptyContract = new();

    private readonly Property<bool> parametersOk;
    private readonly Property<ParameterInfo[]> parameters;
    private readonly Property<string> language;
    private readonly Property<Iso__639_1.Record[]> languages;

    protected readonly Dictionary<CaseInsensitiveString, ParameterInfo> parametersByName = [];
    private readonly DesignModel? designModel;

    private Contract contract;
    private Parameter[]? contractParameters;

    public ContractParametersModel(Parameter[] Parameters, Contract Contract, string Language, DesignModel? DesignModel)
    {
        this.parametersOk = new Property<bool>(nameof(this.ParametersOk), false, this);
        this.parameters = new Property<ParameterInfo[]>(nameof(this.Parameters), [], this);
        this.language = new Property<string>(nameof(this.Language), Language, this);
        this.languages = new Property<Iso__639_1.Record[]>(nameof(this.Languages), [], this);
        this.contract = Contract ?? emptyContract;
        this.designModel = DesignModel;

        this.SetParameters(Parameters);
    }

    public virtual Task SetContract(Contract Contract)
    {
        this.contract = Contract;
        this.SetParameters(Contract.Parameters);
        return Task.CompletedTask;
    }

    protected void SetParameters(Parameter[] ContractParameters)
    {
        foreach (ParameterInfo parameter in this.Parameters)
            parameter.PropertyChanged -= this.ParameterInfo_PropertyChanged;

        this.contractParameters = ContractParameters;

        string PrevLanguage = this.Language;
        bool Found = false;

        this.Languages = this.contract.GetLanguages().ToIso639_1();

        if (!string.IsNullOrWhiteSpace(PrevLanguage))
        {
            foreach (Iso__639_1.Record Rec in this.Languages)
            {
                if (Rec.Code == PrevLanguage)
                {
                    Found = true;
                    break;
                }
            }
        }

        this.Language = Found ? PrevLanguage : this.contract.DefaultLanguage;

        if (string.IsNullOrEmpty(this.Language) && (this.Languages?.Length ?? 0) == 0)
        {
            this.Languages = DesignModel.English;
            this.Language = "en";
        }

        List<ParameterInfo> Parameters = [];

        if (ContractParameters is not null)
        {
            foreach (Parameter Parameter in ContractParameters)
            {
                if (this.parametersByName.TryGetValue(Parameter.Name, out ParameterInfo? ParameterInfo))
                {
                    Parameters.Add(ParameterInfo);
                    ParameterInfo.ContractUpdated(this.Contract);
                }
            }
        }

        this.Parameters = [.. Parameters];
    }

    public Contract Contract => this.contract;

    public string Language
    {
        get => this.language.Value;
        set => this.SetLanguage(value).Wait();
    }

    public Iso__639_1.Record[] Languages
    {
        get => this.languages.Value;
        set => this.languages.Value = value;
    }

    public Iso__639_1.Record? SelectedLanguage
    {
        get => Array.Find(this.Languages, record => record.Code == this.Language);
        set => this.Language = value?.Code ?? string.Empty;
    }

    protected virtual async Task<bool> SetLanguage(string Language)
    {
        try
        {
            this.language.Value = Language;

            if (!string.IsNullOrEmpty(Language))
            {
                foreach (ParameterInfo PI in this.Parameters)
                {
                    HumanReadableText? Text = PI.Parameter.Descriptions.Find(Language, "en");

                    if (Text is null)
                        PI.DescriptionAsMarkdown = string.Empty;
                    else
                        PI.DescriptionAsMarkdown = (await Text.GenerateMarkdown(this.contract, MarkdownType.ForEditing) ?? string.Empty).Trim();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            AppService.ErrorBox(ex.Message);
            return false;
        }
    }

    public bool ParametersOk
    {
        get => this.parametersOk.Value;
        set
        {
            this.parametersOk.Value = value;
            this.ParametersOkChanged();
        }
    }

    protected virtual void ParametersOkChanged() { }

    /// <summary>
    /// Populates the parametersByName dictionary from contract parameters.
    /// Called by subclasses or the view to initialize parameter infos.
    /// </summary>
    public virtual Task PopulateParameters(Dictionary<CaseInsensitiveString, object>? PresetValues)
    {
        List<ParameterInfo> ParameterList = [];
        this.parametersByName.Clear();

        if (this.contractParameters is null)
            return Task.CompletedTask;

        foreach (Parameter Parameter in this.contractParameters)
        {
            ParameterInfo? pi = null;

            if (Parameter is BooleanParameter BP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is bool b)
                    BP.Value = b;

                pi = new BooleanParameterInfo(this.contract, BP, this.designModel, this.parameters);
            }
            else if (Parameter is NumericalParameter NP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is decimal d)
                    NP.Value = d;

                pi = new NumericalParameterInfo(this.contract, NP, this.designModel, this.parameters);
            }
            else if (Parameter is StringParameter SP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is string s)
                    SP.Value = s;

                pi = new StringParameterInfo(this.contract, SP, this.designModel, this.parameters);
            }
            else if (Parameter is DateParameter DP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is DateTime dt)
                    DP.Value = dt;

                pi = new DateParameterInfo(this.contract, DP, this.designModel, this.parameters);
            }
            else if (Parameter is DateTimeParameter DTP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is DateTime dt)
                    DTP.Value = dt;

                pi = new DateTimeParameterInfo(this.contract, DTP, this.designModel, this.parameters);
            }
            else if (Parameter is TimeParameter TP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is TimeSpan ts)
                    TP.Value = ts;

                pi = new TimeParameterInfo(this.contract, TP, this.designModel, this.parameters);
            }
            else if (Parameter is DurationParameter DrP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is Waher.Content.Duration dur)
                    DrP.Value = dur;

                pi = new DurationParameterInfo(this.contract, DrP, this.designModel, this.parameters);
            }
            else if (Parameter is CalcParameter CP)
                pi = new CalcParameterInfo(this.contract, CP, this.designModel, this.parameters);
            else if (Parameter is RoleParameter RP)
                pi = new RoleParameterInfo(this.contract, RP, this.designModel, this.parameters);
            else if (Parameter is ContractReferenceParameter CRP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is string s)
                    CRP.Value = s;

                pi = new ContractReferenceParameterInfo(this.contract, CRP, this.designModel, this.parameters);
            }
            else if (Parameter is GeoParameter GP)
            {
                if ((PresetValues?.TryGetValue(Parameter.Name, out object? pv) ?? false) && pv is GeoPosition pos)
                    GP.Value = pos;

                pi = new GeoParameterInfo(this.contract, GP, this.designModel, this.parameters);
            }

            if (pi is not null)
            {
                this.parametersByName[Parameter.Name] = pi;
                pi.PropertyChanged += this.ParameterInfo_PropertyChanged;
                ParameterList.Add(pi);
            }
        }

        this.Parameters = [.. ParameterList];
        return this.ValidateParameters();
    }

    private async void ParameterInfo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ParameterInfo parameter)
            return;

        if (e.PropertyName == nameof(ParameterInfo.Value))
        {
            await this.ValidateParameters();
            await this.RaiseParametersChanged();
        }
        else if ((e.PropertyName == nameof(ParameterInfo.ErrorText) || e.PropertyName == nameof(ParameterInfo.ErrorReason)) &&
                 !string.IsNullOrEmpty(parameter.ErrorText))
        {
            this.ParametersOk = false;
        }
    }

    protected async Task RaiseParametersChanged()
    {
        try
        {
            await this.ParametersChanged();
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }

    protected virtual Task ParametersChanged() => Task.CompletedTask;

    public virtual async Task<Variables?> ValidateParameters()
    {
        Variables Variables = [];
        bool Ok = true;

        Variables["Duration"] = this.contract.Duration;

        DateTime? FirstSignature = this.Contract.FirstSignatureAt;
        if (FirstSignature.HasValue)
        {
            Variables["Now"] = FirstSignature.Value.ToLocalTime();
            Variables["NowUtc"] = FirstSignature.Value.ToUniversalTime();
        }

        foreach (ParameterInfo P in this.parametersByName.Values)
            P.Parameter.Populate(Variables);

        foreach (ParameterInfo P in this.parametersByName.Values)
        {
            if (await P.ValidateParameter(Variables))
            {
                Log.Informational("Parameter " + P.Name + " is OK.");
            }
            else
            {
                StringBuilder Msg = new();
                Msg.Append("Parameter ").Append(P.Name).Append(" contains errors.");

                if (P.ErrorReason.HasValue)
                    Msg.Append(" Reason: ").Append(P.ErrorReason.Value.ToString());

                if (!string.IsNullOrEmpty(P.ErrorText))
                    Msg.Append(", Text: ").Append(P.ErrorText);

                Log.Error(Msg.ToString());

                if ((P is not CalcParameterInfo && P is not RoleParameterInfo) ||
                    this.contract.State == ContractState.Approved)
                {
                    Ok = false;
                }
            }
        }

        this.ParametersOk = Ok;
        return Ok ? Variables : null;
    }

    public ParameterInfo[] Parameters
    {
        get => this.parameters.Value;
        set => this.parameters.Value = value;
    }
}
