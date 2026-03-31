using LegalLabMaui.Models.Tokens.Reports;

namespace LegalLabMaui.Views;

[QueryProperty(nameof(ReportMarkdownParam), "ReportMarkdown")]
[QueryProperty(nameof(TitleParam), "Title")]
[QueryProperty(nameof(ReportParam), "Report")]
public partial class ReportPage : ContentPage
{
    private TokenReport? report;
    private string reportText = string.Empty;

    public ReportPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string ReportText
    {
        get => this.reportText;
        set
        {
            this.reportText = value;
            OnPropertyChanged();
        }
    }

    public string ReportMarkdownParam
    {
        set => this.ReportText = value ?? string.Empty;
    }

    public string TitleParam
    {
        set => Title = value ?? "Report";
    }

    public TokenReport? ReportParam
    {
        set
        {
            if (this.report is not null)
                this.report.ReportUpdated -= this.Report_ReportUpdated;

            this.report = value;

            if (this.report is not null)
                this.report.ReportUpdated += this.Report_ReportUpdated;
        }
    }

    private void Report_ReportUpdated(object? sender, ReportUpdatedEventArgs e)
    {
        _ = AppService.UpdateGui(() =>
        {
            this.ReportText = e.Markdown ?? string.Empty;
            return Task.CompletedTask;
        });
    }
}
