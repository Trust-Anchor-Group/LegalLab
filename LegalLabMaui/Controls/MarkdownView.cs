using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LegalLabMaui.Models.Script;
using Microsoft.Maui.Controls.Shapes;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Operators.Matrices;
using MarkdownTextAlignment = Waher.Content.Markdown.Model.TextAlignment;

namespace LegalLabMaui.Controls;

public class MarkdownView : ContentView
{
    public static readonly BindableProperty MarkdownProperty = BindableProperty.Create(
        nameof(Markdown), typeof(string), typeof(MarkdownView), string.Empty,
        propertyChanged: OnMarkdownChanged);

    private int renderVersion;

    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    private static void OnMarkdownChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MarkdownView)bindable).RenderMarkdown();
    }

    private async void RenderMarkdown()
    {
        int version = ++this.renderVersion;
        string markdown = this.Markdown ?? string.Empty;

        if (string.IsNullOrWhiteSpace(markdown))
        {
            this.Content = new Label
            {
                Text = string.Empty,
                IsVisible = false
            };
            return;
        }

        try
        {
            MarkdownDocument document = await MarkdownDocument.CreateAsync(markdown);
            MarkdownTheme theme = MarkdownTheme.Create();
            MauiMarkdownRenderer renderer = new(theme);
            View view = await renderer.RenderAsync(document);

            if (version != this.renderVersion)
                return;

            await MainThread.InvokeOnMainThreadAsync(() => this.Content = view);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);

            if (version != this.renderVersion)
                return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                this.Content = new Label
                {
                    Text = ex.Message,
                    TextColor = Colors.Red,
                    LineBreakMode = LineBreakMode.WordWrap
                };
            });
        }
    }
}

internal sealed class MauiMarkdownRenderer : Renderer
{
    private readonly MarkdownTheme theme;
    private readonly Stack<IList<IView>> containers = new();
    private readonly Stack<LabelState> labelStates = new();

    private VerticalStackLayout? root;
    private Label? currentLabel;
    private FormattedString? currentFormatted;

    private MarkdownTextAlignment alignment = MarkdownTextAlignment.Left;
    private bool bold;
    private bool italic;
    private bool strikeThrough;
    private bool underline;
    private bool superscript;
    private bool subscript;
    private bool code;
    private string? hyperlink;

    public MauiMarkdownRenderer(MarkdownTheme theme)
    {
        this.theme = theme;
    }

    public async Task<View> RenderAsync(MarkdownDocument document)
    {
        await this.RenderDocument(document, false);
        return this.root ?? new VerticalStackLayout();
    }

    public override Task RenderDocument(MarkdownDocument document, bool inclusion)
    {
        this.alignment = MarkdownTextAlignment.Left;
        this.bold = false;
        this.italic = false;
        this.strikeThrough = false;
        this.underline = false;
        this.superscript = false;
        this.subscript = false;
        this.code = false;
        this.hyperlink = null;

        return base.RenderDocument(document, inclusion);
    }

    public override Task RenderDocumentHeader()
    {
        this.root = new VerticalStackLayout
        {
            Spacing = 0
        };

        this.containers.Push(this.root.Children);
        return Task.CompletedTask;
    }

    public override Task RenderDocumentFooter()
    {
        if (this.containers.Count > 0)
            this.containers.Pop();

        return Task.CompletedTask;
    }

    public override async Task RenderFootnotes()
    {
        if (this.Document is null)
            return;

        bool hasFootnotes = false;

        foreach (string key in this.Document.FootnoteOrder)
        {
            if (!this.Document.TryGetFootnoteNumber(key, out _) ||
                !this.Document.TryGetFootnote(key, out Footnote? footnote) ||
                footnote is null ||
                !footnote.Referenced)
            {
                continue;
            }

            hasFootnotes = true;
            break;
        }

        if (!hasFootnotes)
            return;

        this.AddView(new BoxView
        {
            HeightRequest = 1,
            Color = this.theme.TableBorderColor,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = this.theme.ParagraphMargins
        });

        foreach (string key in this.Document.FootnoteOrder)
        {
            if (!this.Document.TryGetFootnoteNumber(key, out int nr) ||
                !this.Document.TryGetFootnote(key, out Footnote? footnote) ||
                footnote is null ||
                !footnote.Referenced)
            {
                continue;
            }

            Grid grid = new()
            {
                ColumnSpacing = 10,
                Margin = new Thickness(
                    this.theme.ParagraphMargins.Left,
                    this.theme.ParagraphMarginTop,
                    this.theme.ParagraphMargins.Right,
                    this.theme.ParagraphMarginBottom)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Label numberLabel = this.CreateBaseLabel();
            numberLabel.Text = nr.ToString();
            numberLabel.FontSize = this.theme.BaseFontSize * this.theme.SuperscriptScale;
            numberLabel.TranslationY = -this.theme.SuperscriptOffset;
            Grid.SetColumn(numberLabel, 0);
            grid.Children.Add(numberLabel);

            VerticalStackLayout content = new() { Spacing = 0 };
            Grid.SetColumn(content, 1);
            grid.Children.Add(content);

            this.AddView(grid);
            this.containers.Push(content.Children);
            await footnote.Render(this);
            this.containers.Pop();
        }
    }

    public override Task Render(Abbreviation element) => this.RenderChildren(element);

    public override Task Render(AutomaticLinkMail element)
    {
        string? bak = this.hyperlink;
        this.hyperlink = "mailto:" + element.EMail;
        this.RenderSpan(element.EMail);
        this.hyperlink = bak;
        return Task.CompletedTask;
    }

    public override Task Render(AutomaticLinkUrl element)
    {
        string? bak = this.hyperlink;
        this.hyperlink = element.URL;
        this.RenderSpan(element.URL);
        this.hyperlink = bak;
        return Task.CompletedTask;
    }

    public override async Task Render(Delete element)
    {
        bool bak = this.strikeThrough;
        this.strikeThrough = true;
        await this.RenderChildren(element);
        this.strikeThrough = bak;
    }

    public override Task Render(DetailsReference element)
    {
        if (this.Document?.Detail is not null)
            return this.RenderDocument(this.Document.Detail, true);

        return this.Render((MetaReference)element);
    }

    public override Task Render(EmojiReference element)
    {
        this.RenderSpan(element.Emoji.Unicode);
        return Task.CompletedTask;
    }

    public override async Task Render(Emphasize element)
    {
        bool bak = this.italic;
        this.italic = true;
        await this.RenderChildren(element);
        this.italic = bak;
    }

    public override async Task Render(FootnoteReference element)
    {
        if (this.Document is null)
            return;

        Footnote? footnote = null;

        if (this.Document.TryGetFootnote(element.Key, out footnote) &&
            element.AutoExpand &&
            footnote is not null)
        {
            await this.Render(footnote);
            return;
        }

        if (this.Document.TryGetFootnoteNumber(element.Key, out int nr))
        {
            bool bak = this.superscript;
            this.superscript = true;
            this.RenderSpan(nr.ToString());
            this.superscript = bak;

            if (footnote is not null)
                footnote.Referenced = true;
        }
    }

    public override Task Render(HashTag element)
    {
        this.RenderSpan(element.Tag);
        return Task.CompletedTask;
    }

    public override Task Render(HtmlEntity element)
    {
        string value = WebUtility.HtmlDecode("&" + element.Entity + ";");
        if (!string.IsNullOrEmpty(value) && value != "&" + element.Entity + ";")
            this.RenderSpan(value);

        return Task.CompletedTask;
    }

    public override Task Render(HtmlEntityUnicode element)
    {
        this.RenderSpan(new string((char)element.Code, 1));
        return Task.CompletedTask;
    }

    public override Task Render(InlineCode element)
    {
        bool bak = this.code;
        this.code = true;
        this.RenderSpan(element.Code);
        this.code = bak;
        return Task.CompletedTask;
    }

    public override Task Render(InlineHTML element)
    {
        this.RenderSpan(element.HTML);
        return Task.CompletedTask;
    }

    public override async Task Render(InlineScript element)
    {
        object? result = await element.EvaluateExpression();
        await this.RenderObject(result, element.AloneInParagraph, element.Variables);
    }

    public async Task RenderObject(object? result, bool aloneInParagraph, Variables variables)
    {
        if (result is null)
            return;

        if (result is XmlDocument xml)
            result = await MarkdownDocument.TransformXml(xml, variables);
        else if (result is IToMatrix toMatrix)
            result = toMatrix.ToMatrix();

        switch (result)
        {
            case Graph graph:
                this.AddView(this.CreateGraphImage(graph));
                break;

            case SKImage image:
                this.AddView(this.CreateSkiaImage(image));
                break;

            case Exception ex:
                this.AddView(this.CreateParagraphLabel(Log.UnnestException(ex).Message, this.theme.ErrorColor));
                break;

            default:
                if (this.currentLabel is not null)
                    this.RenderSpan(result.ToString() ?? string.Empty);
                else
                    this.AddView(this.CreateParagraphLabel(result.ToString() ?? string.Empty));
                break;
        }
    }

    public override Task Render(InlineText element)
    {
        this.RenderSpan(element.Value);
        return Task.CompletedTask;
    }

    public override async Task Render(Insert element)
    {
        bool bak = this.underline;
        this.underline = true;
        await this.RenderChildren(element);
        this.underline = bak;
    }

    public override Task Render(LineBreak element)
    {
        this.RenderSpan(Environment.NewLine);
        return Task.CompletedTask;
    }

    public override async Task Render(Link element)
    {
        string? bak = this.hyperlink;
        this.hyperlink = element.Url;
        await this.RenderChildren(element);
        this.hyperlink = bak;
    }

    public override async Task Render(LinkReference element)
    {
        Multimedia? multimedia = element.Document.GetReference(element.Label);
        string? bak = this.hyperlink;

        if (multimedia is not null && multimedia.Items.Length > 0)
            this.hyperlink = multimedia.Items[0].Url;

        await this.RenderChildren(element);
        this.hyperlink = bak;
    }

    public override Task Render(MetaReference element)
    {
        StringBuilder sb = new();
        bool firstOnRow = true;

        if (element.TryGetMetaData(out KeyValuePair<string, bool>[]? values) && values is not null)
        {
            foreach (KeyValuePair<string, bool> pair in values)
            {
                if (firstOnRow)
                    firstOnRow = false;
                else
                    sb.Append(' ');

                sb.Append(pair.Key);

                if (pair.Value)
                {
                    sb.AppendLine();
                    firstOnRow = true;
                }
            }
        }

        this.RenderSpan(sb.ToString());
        return Task.CompletedTask;
    }

    public override Task Render(Multimedia element)
    {
        return this.RenderMultimedia(element.Items, element.Children, element.AloneInParagraph, element.Document);
    }

    public override Task Render(MultimediaReference element)
    {
        Multimedia? multimedia = element.Document.GetReference(element.Label);
        return multimedia is null ? this.RenderChildren(element) : this.RenderMultimedia(multimedia.Items, element.Children, element.AloneInParagraph, element.Document);
    }

    public override async Task Render(StrikeThrough element)
    {
        bool bak = this.strikeThrough;
        this.strikeThrough = true;
        await this.RenderChildren(element);
        this.strikeThrough = bak;
    }

    public override async Task Render(Strong element)
    {
        bool bak = this.bold;
        this.bold = true;
        await this.RenderChildren(element);
        this.bold = bak;
    }

    public override async Task Render(SubScript element)
    {
        bool bak = this.subscript;
        this.subscript = true;
        await this.RenderChildren(element);
        this.subscript = bak;
    }

    public override async Task Render(SuperScript element)
    {
        bool bak = this.superscript;
        this.superscript = true;
        await this.RenderChildren(element);
        this.superscript = bak;
    }

    public override async Task Render(Underline element)
    {
        bool bak = this.underline;
        this.underline = true;
        await this.RenderChildren(element);
        this.underline = bak;
    }

    public override async Task Render(BlockQuote element)
    {
        Grid grid = this.BeginQuoteBlock(this.theme.QuoteBorderColor);
        await this.RenderChildren(element);
        this.EndContainer();
    }

    public override async Task Render(BulletList element)
    {
        Grid grid = new()
        {
            Margin = this.theme.ParagraphMargins,
            RowSpacing = 0,
            ColumnSpacing = 0
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < element.Children.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        this.AddView(grid);

        ChunkNode<MarkdownElement>? loop = element.Children.FirstChunk;
        int row = 0;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                MarkdownElement child = loop[i];
                bool paragraphBullet = !child.InlineSpanElement || child.OutsideParagraph;
                this.GetMargins(child, out int topMargin, out int bottomMargin);

                Label bullet = this.CreateBaseLabel();
                bullet.Text = "•";
                bullet.Margin = new Thickness(0, topMargin, this.theme.ListContentMargin, bottomMargin);
                Grid.SetColumn(bullet, 0);
                Grid.SetRow(bullet, row);
                grid.Children.Add(bullet);

                VerticalStackLayout content = new() { Spacing = 0 };
                Grid.SetColumn(content, 1);
                Grid.SetRow(content, row);
                grid.Children.Add(content);

                this.containers.Push(content.Children);
                if (paragraphBullet)
                    await child.Render(this);
                else
                    await this.RenderLabelElement(child, true, Thickness.Zero, null, null, false);
                this.containers.Pop();

                row++;
            }

            loop = loop.Next;
        }
    }

    public override async Task Render(CenterAligned element)
    {
        MarkdownTextAlignment bak = this.alignment;
        this.alignment = MarkdownTextAlignment.Center;
        await this.RenderChildren(element);
        this.alignment = bak;
    }

    public override Task Render(CodeBlock element)
    {
        Editor editor = new()
        {
            Text = string.Join(Environment.NewLine, this.GetCodeRows(element)),
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.TextChanges,
            FontFamily = "Courier New",
            FontSize = this.theme.CodeFontSize,
            TextColor = this.theme.TextColor,
            BackgroundColor = this.theme.CodeBackgroundColor,
            Margin = this.theme.ParagraphMargins,
            MinimumHeightRequest = 0
        };

        this.AddView(editor);
        return Task.CompletedTask;
    }

    public override Task Render(CommentBlock element) => Task.CompletedTask;

    public override async Task Render(DefinitionDescriptions element)
    {
        MarkdownElement? last = element.Children.HasLastItem ? element.Children.LastItem : null;
        ChunkNode<MarkdownElement>? loop = element.Children.FirstChunk;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                MarkdownElement child = loop[i];
                Thickness margin = new(this.theme.DefinitionMargin, 0, 0, child == last ? this.theme.DefinitionSeparator : 0);

                if (child.InlineSpanElement && !child.OutsideParagraph)
                    await this.RenderLabelElement(child, true, margin, null, null, false);
                else
                {
                    VerticalStackLayout stack = new() { Spacing = 0, Margin = margin };
                    this.AddView(stack);
                    this.containers.Push(stack.Children);
                    await child.Render(this);
                    this.containers.Pop();
                }
            }

            loop = loop.Next;
        }
    }

    public override Task Render(DefinitionList element) => this.RenderChildren(element);

    public override async Task Render(DefinitionTerms element)
    {
        int topMargin = this.theme.ParagraphMarginTop;
        ChunkNode<MarkdownElement>? loop = element.Children.FirstChunk;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                Thickness margin = new(this.theme.DefinitionMargin, topMargin, this.theme.ParagraphMargins.Right, 0);
                await this.RenderLabelElement(loop[i], true, margin, null, null, true);
                topMargin = 0;
            }

            loop = loop.Next;
        }
    }

    public override async Task Render(DeleteBlocks element)
    {
        this.BeginQuoteBlock(this.theme.DeletedQuoteBorderColor);
        await this.RenderChildren(element);
        this.EndContainer();
    }

    public override Task Render(Footnote element) => this.RenderChildren(element);

    public override Task Render(Header element)
    {
        int index = Math.Clamp(element.Level - 1, 0, this.theme.HeaderFontSizes.Length - 1);
        return this.RenderLabelElement(
            element,
            false,
            this.theme.ParagraphMargins,
            this.theme.HeaderFontSizes[index],
            this.theme.HeaderColors[index],
            true);
    }

    public override Task Render(HorizontalRule element)
    {
        this.AddView(new BoxView
        {
            HeightRequest = 1,
            Color = this.theme.TableBorderColor,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = this.theme.ParagraphMargins
        });

        return Task.CompletedTask;
    }

    public override Task Render(HtmlBlock element)
    {
        return this.RenderChildren(element);
    }

    public override async Task Render(InsertBlocks element)
    {
        this.BeginQuoteBlock(this.theme.InsertedQuoteBorderColor);
        await this.RenderChildren(element);
        this.EndContainer();
    }

    public override Task Render(InvisibleBreak element) => Task.CompletedTask;

    public override async Task Render(LeftAligned element)
    {
        MarkdownTextAlignment bak = this.alignment;
        this.alignment = MarkdownTextAlignment.Left;
        await this.RenderChildren(element);
        this.alignment = bak;
    }

    public override async Task Render(MarginAligned element)
    {
        MarkdownTextAlignment bak = this.alignment;
        this.alignment = MarkdownTextAlignment.Left;
        await this.RenderChildren(element);
        this.alignment = bak;
    }

    public override async Task Render(NestedBlock element)
    {
        if (element.HasOneChild)
        {
            await element.FirstChild.Render(this);
            return;
        }

        if (this.IsInlineOnly(element.Children))
            await this.RenderLabelElement(element, false, Thickness.Zero, null, null, false);
        else
            await this.RenderChildren(element);
    }

    public override Task Render(NumberedItem element) => this.RenderChild(element);

    public override async Task Render(NumberedList element)
    {
        Grid grid = new()
        {
            Margin = this.theme.ParagraphMargins,
            RowSpacing = 0,
            ColumnSpacing = 0
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < element.Children.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        this.AddView(grid);

        ChunkNode<MarkdownElement>? loop = element.Children.FirstChunk;
        int row = 0;
        int expected = 0;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                MarkdownElement child = loop[i];
                expected++;
                int number = child is NumberedItem item ? item.Number : expected;

                bool paragraphBullet = !child.InlineSpanElement || child.OutsideParagraph;
                this.GetMargins(child, out int topMargin, out int bottomMargin);

                Label bullet = this.CreateBaseLabel();
                bullet.Text = number + ".";
                bullet.Margin = new Thickness(0, topMargin, this.theme.ListContentMargin, bottomMargin);
                Grid.SetColumn(bullet, 0);
                Grid.SetRow(bullet, row);
                grid.Children.Add(bullet);

                VerticalStackLayout content = new() { Spacing = 0 };
                Grid.SetColumn(content, 1);
                Grid.SetRow(content, row);
                grid.Children.Add(content);

                this.containers.Push(content.Children);
                if (paragraphBullet)
                    await child.Render(this);
                else
                    await this.RenderLabelElement(child, true, Thickness.Zero, null, null, false);
                this.containers.Pop();

                row++;
            }

            loop = loop.Next;
        }
    }

    public override Task Render(Paragraph element)
    {
        return this.RenderLabelElement(element, false, this.theme.ParagraphMargins, null, null, false);
    }

    public override async Task Render(RightAligned element)
    {
        MarkdownTextAlignment bak = this.alignment;
        this.alignment = MarkdownTextAlignment.Right;
        await this.RenderChildren(element);
        this.alignment = bak;
    }

    public override Task Render(Sections element) => this.RenderChildren(element);

    public override Task Render(SectionSeparator element)
    {
        this.AddView(new BoxView
        {
            HeightRequest = 1,
            Color = this.theme.TableBorderColor,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = this.theme.ParagraphMargins
        });

        return Task.CompletedTask;
    }

    public override async Task Render(Table element)
    {
        Grid grid = new()
        {
            Margin = this.theme.ParagraphMargins,
            RowSpacing = 0,
            ColumnSpacing = 0
        };

        for (int column = 0; column < element.Columns; column++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        int totalRows = element.Headers.Length + element.Rows.Length;
        for (int row = 0; row < totalRows; row++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        this.AddView(grid);

        int rowNr = 0;
        for (int row = 0; row < element.Headers.Length; row++, rowNr++)
            await this.RenderTableRow(grid, element.Headers[row], element.HeaderCellAlignments[row], rowNr, true, element);

        for (int row = 0; row < element.Rows.Length; row++, rowNr++)
            await this.RenderTableRow(grid, element.Rows[row], element.RowCellAlignments[row], rowNr, false, element);
    }

    public override Task Render(TaskItem element) => this.RenderChild(element);

    public override async Task Render(TaskList element)
    {
        Grid grid = new()
        {
            Margin = this.theme.ParagraphMargins,
            RowSpacing = 0,
            ColumnSpacing = 0
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < element.Children.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        this.AddView(grid);

        ChunkNode<MarkdownElement>? loop = element.Children.FirstChunk;
        int row = 0;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                MarkdownElement child = loop[i];
                bool paragraphBullet = !child.InlineSpanElement || child.OutsideParagraph;
                this.GetMargins(child, out int topMargin, out int bottomMargin);

                Label bullet = this.CreateBaseLabel();
                bullet.Text = child is TaskItem item && item.IsChecked ? "✓" : string.Empty;
                bullet.Margin = new Thickness(0, topMargin, this.theme.ListContentMargin, bottomMargin);
                Grid.SetColumn(bullet, 0);
                Grid.SetRow(bullet, row);
                grid.Children.Add(bullet);

                VerticalStackLayout content = new() { Spacing = 0 };
                Grid.SetColumn(content, 1);
                Grid.SetRow(content, row);
                grid.Children.Add(content);

                this.containers.Push(content.Children);
                if (paragraphBullet)
                    await child.Render(this);
                else
                    await this.RenderLabelElement(child, true, Thickness.Zero, null, null, false);
                this.containers.Pop();

                row++;
            }

            loop = loop.Next;
        }
    }

    public override Task Render(UnnumberedItem element) => this.RenderChild(element);

    private async Task RenderMultimedia(MultimediaItem[] items, ChunkedList<MarkdownElement> children, bool aloneInParagraph, MarkdownDocument document)
    {
        if (items.Length == 0)
        {
            await this.Render(children);
            return;
        }

        MultimediaItem item = items[0];
        View? view = this.CreateMultimediaView(item);

        if (view is not null)
        {
            this.AddView(view);
            return;
        }

        if (!string.IsNullOrEmpty(item.Url))
        {
            string? bak = this.hyperlink;
            this.hyperlink = item.Url;
            if (this.currentLabel is not null)
                this.RenderSpan(item.Title ?? item.Url);
            else
                this.AddView(this.CreateParagraphLabel(item.Title ?? item.Url, this.theme.LinkColor));
            this.hyperlink = bak;
            return;
        }

        await this.Render(children);
    }

    private View? CreateMultimediaView(MultimediaItem item)
    {
        if (string.IsNullOrEmpty(item.Url))
            return null;

        ImageSource? source = this.CreateImageSource(item.Url);
        if (source is null)
            return null;

        Image image = new()
        {
            Source = source,
            Aspect = Aspect.AspectFit,
            Margin = this.theme.ParagraphMargins,
            HorizontalOptions = LayoutOptions.Start,
            MaximumHeightRequest = item.Height.GetValueOrDefault() > 0 ? item.Height.Value : 420
        };

        if (item.Width.GetValueOrDefault() > 0)
            image.MaximumWidthRequest = item.Width.Value;

        if (Uri.TryCreate(item.Url, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            TapGestureRecognizer tap = new();
            tap.Tapped += (_, _) => AppService.OpenUrl(uri);
            image.GestureRecognizers.Add(tap);
        }

        return image;
    }

    private Image CreateGraphImage(Graph graph)
    {
        PixelInformation pixels = graph.CreatePixels();
        byte[] data = pixels.EncodeAsPng();

        return new Image
        {
            Source = ImageSource.FromStream(() => new MemoryStream(data)),
            Aspect = Aspect.AspectFit,
            Margin = this.theme.ParagraphMargins,
            MaximumHeightRequest = pixels.Height,
            MaximumWidthRequest = pixels.Width
        };
    }

    private Image CreateSkiaImage(SKImage image)
    {
        using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        byte[] data = encoded.ToArray();

        return new Image
        {
            Source = ImageSource.FromStream(() => new MemoryStream(data)),
            Aspect = Aspect.AspectFit,
            Margin = this.theme.ParagraphMargins,
            MaximumHeightRequest = image.Height,
            MaximumWidthRequest = image.Width
        };
    }

    private ImageSource? CreateImageSource(string url)
    {
        try
        {
            const string prefix = "data:image/";

            if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                int commaPos = url.IndexOf(',');
                if (commaPos > 0)
                {
                    string meta = url[..commaPos];
                    string payload = url[(commaPos + 1)..];

                    if (meta.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] data = Convert.FromBase64String(payload);
                        return ImageSource.FromStream(() => new MemoryStream(data));
                    }
                }
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                return ImageSource.FromUri(uri);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }

        return null;
    }

    private async Task RenderLabelElement(MarkdownElement element, bool includeElement, Thickness margin, double? fontSize, Color? textColor, bool forceBold)
    {
        Label label = this.CreateBaseLabel();
        label.Margin = margin;

        if (fontSize.HasValue)
            label.FontSize = fontSize.Value;

        if (textColor is not null)
            label.TextColor = textColor;

        this.AddView(label);

        LabelState state = new(this.currentLabel, this.currentFormatted, this.bold);
        this.labelStates.Push(state);

        this.currentLabel = label;
        this.currentFormatted = new FormattedString();
        this.currentLabel.FormattedText = this.currentFormatted;

        if (forceBold)
            this.bold = true;

        try
        {
            if (includeElement)
                await element.Render(this);
            else
                await this.RenderChildren(element);
        }
        finally
        {
            LabelState previous = this.labelStates.Pop();
            this.currentLabel = previous.Label;
            this.currentFormatted = previous.Formatted;
            this.bold = previous.Bold;
        }
    }

    private Grid BeginQuoteBlock(Color borderColor)
    {
        Grid grid = new()
        {
            ColumnSpacing = 0,
            Margin = this.theme.ParagraphMargins
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        BoxView accent = new()
        {
            WidthRequest = this.theme.QuoteBorderWidth,
            Color = borderColor,
            VerticalOptions = LayoutOptions.Fill
        };
        Grid.SetColumn(accent, 0);
        grid.Children.Add(accent);

        VerticalStackLayout content = new()
        {
            Spacing = 0,
            Padding = new Thickness(this.theme.QuotePadding, 0, 0, 0)
        };
        Grid.SetColumn(content, 1);
        grid.Children.Add(content);

        this.AddView(grid);
        this.containers.Push(content.Children);
        return grid;
    }

    private void EndContainer()
    {
        if (this.containers.Count > 0)
            this.containers.Pop();
    }

    private Label CreateBaseLabel()
    {
        return new Label
        {
            LineBreakMode = LineBreakMode.WordWrap,
            TextColor = this.theme.TextColor,
            FontSize = this.theme.BaseFontSize,
            HorizontalTextAlignment = this.ToMauiAlignment(this.alignment)
        };
    }

    private Label CreateParagraphLabel(string text, Color? textColor = null)
    {
        return new Label
        {
            Text = text,
            LineBreakMode = LineBreakMode.WordWrap,
            TextColor = textColor ?? this.theme.TextColor,
            FontSize = this.theme.BaseFontSize,
            Margin = this.theme.ParagraphMargins,
            HorizontalTextAlignment = this.ToMauiAlignment(this.alignment)
        };
    }

    private void RenderSpan(string text)
    {
        if (this.currentFormatted is null)
            return;

        if (this.superscript)
            text = TextRenderer.ToSuperscript(text);
        else if (this.subscript)
            text = TextRenderer.ToSubscript(text);

        Span span = new()
        {
            Text = text,
            FontFamily = this.code ? "Courier New" : null,
            TextColor = this.hyperlink is null ? this.theme.TextColor : this.theme.LinkColor
        };

        FontAttributes fontAttributes = FontAttributes.None;
        if (this.bold)
            fontAttributes |= FontAttributes.Bold;
        if (this.italic)
            fontAttributes |= FontAttributes.Italic;
        if (fontAttributes != FontAttributes.None)
            span.FontAttributes = fontAttributes;

        TextDecorations decorations = TextDecorations.None;
        if (this.strikeThrough)
            decorations |= TextDecorations.Strikethrough;
        if (this.underline || this.hyperlink is not null)
            decorations |= TextDecorations.Underline;
        if (decorations != TextDecorations.None)
            span.TextDecorations = decorations;

        if (this.hyperlink is not null && Uri.TryCreate(this.hyperlink, UriKind.Absolute, out Uri? uri))
        {
            TapGestureRecognizer tap = new();
            tap.Tapped += (_, _) => AppService.OpenUrl(uri);
            span.GestureRecognizers.Add(tap);
        }

        this.currentFormatted.Spans.Add(span);
    }

    private async Task RenderTableRow(Grid grid, MarkdownElement[] currentRow, MarkdownTextAlignment?[] cellAlignments, int rowNr, bool isHeader, Table element)
    {
        int nrColumns = element.Columns;
        MarkdownTextAlignment bak = this.alignment;

        for (int column = 0; column < nrColumns; column++)
        {
            MarkdownElement? cell = currentRow[column];
            if (cell is null)
                continue;

            this.alignment = cellAlignments[column] ?? element.ColumnAlignments[column];
            int columnSpan = column + 1;
            while (columnSpan < nrColumns && currentRow[columnSpan] is null)
                columnSpan++;
            columnSpan -= column;

            Border border = new()
            {
                Stroke = new SolidColorBrush(this.theme.TableBorderColor),
                StrokeThickness = 1,
                BackgroundColor = rowNr % 2 == 0 ? this.theme.TableRowOddBackground : this.theme.TableRowEvenBackground,
                Padding = this.theme.TableCellPadding,
                StrokeShape = new RoundRectangle { CornerRadius = 0 }
            };

            Grid.SetColumn(border, column);
            Grid.SetRow(border, rowNr);
            if (columnSpan > 1)
                Grid.SetColumnSpan(border, columnSpan);
            grid.Children.Add(border);

            if (cell.InlineSpanElement && !cell.OutsideParagraph)
            {
                VerticalStackLayout host = new() { Spacing = 0 };
                border.Content = host;
                this.containers.Push(host.Children);
                await this.RenderLabelElement(cell, true, Thickness.Zero, null, null, isHeader);
                this.containers.Pop();
            }
            else
            {
                VerticalStackLayout host = new() { Spacing = 0 };
                border.Content = host;
                this.containers.Push(host.Children);
                await cell.Render(this);
                this.containers.Pop();
            }
        }

        this.alignment = bak;
    }

    private bool IsInlineOnly(ChunkedList<MarkdownElement> children)
    {
        ChunkNode<MarkdownElement>? loop = children.FirstChunk;

        while (loop is not null)
        {
            for (int i = loop.Start; i < loop.Pos; i++)
            {
                MarkdownElement child = loop[i];
                if (!child.InlineSpanElement || child.OutsideParagraph)
                    return false;
            }

            loop = loop.Next;
        }

        return true;
    }

    private IEnumerable<string> GetCodeRows(CodeBlock element)
    {
        for (int i = element.Start; i <= element.End; i++)
            yield return element.Rows[i];
    }

    private void AddView(View view)
    {
        if (this.containers.Count == 0)
            return;

        this.containers.Peek().Add(view);
    }

    private void GetMargins(MarkdownElement element, out int topMargin, out int bottomMargin)
    {
        if (element.InlineSpanElement && !element.OutsideParagraph)
        {
            topMargin = 0;
            bottomMargin = 0;
        }
        else if (element is NestedBlock nestedBlock)
        {
            ChunkNode<MarkdownElement>? loop = nestedBlock.Children.FirstChunk;
            bool first = true;
            topMargin = 0;
            bottomMargin = 0;

            while (loop is not null)
            {
                for (int i = loop.Start; i < loop.Pos; i++)
                {
                    MarkdownElement child = loop[i];
                    if (first)
                    {
                        first = false;
                        this.GetMargins(child, out topMargin, out bottomMargin);
                    }
                    else
                    {
                        this.GetMargins(child, out _, out bottomMargin);
                    }
                }

                loop = loop.Next;
            }
        }
        else if (element is MarkdownElementSingleChild singleChild)
        {
            this.GetMargins(singleChild.Child, out topMargin, out bottomMargin);
        }
        else
        {
            topMargin = this.theme.ParagraphMarginTop;
            bottomMargin = this.theme.ParagraphMarginBottom;
        }
    }

    private Microsoft.Maui.TextAlignment ToMauiAlignment(MarkdownTextAlignment alignment)
    {
        return alignment switch
        {
            MarkdownTextAlignment.Center => Microsoft.Maui.TextAlignment.Center,
            MarkdownTextAlignment.Right => Microsoft.Maui.TextAlignment.End,
            _ => Microsoft.Maui.TextAlignment.Start
        };
    }

    private readonly record struct LabelState(Label? Label, FormattedString? Formatted, bool Bold);
}

internal sealed class MarkdownTheme
{
    public double BaseFontSize { get; init; }
    public double CodeFontSize { get; init; }
    public Thickness ParagraphMargins { get; init; }
    public int ParagraphMarginTop { get; init; }
    public int ParagraphMarginBottom { get; init; }
    public int DefinitionMargin { get; init; }
    public int DefinitionSeparator { get; init; }
    public int ListContentMargin { get; init; }
    public int QuotePadding { get; init; }
    public double QuoteBorderWidth { get; init; }
    public double SuperscriptScale { get; init; }
    public double SuperscriptOffset { get; init; }
    public Thickness TableCellPadding { get; init; }
    public double[] HeaderFontSizes { get; init; } = [];
    public Color[] HeaderColors { get; init; } = [];
    public Color TextColor { get; init; }
    public Color LinkColor { get; init; }
    public Color QuoteBorderColor { get; init; }
    public Color InsertedQuoteBorderColor { get; init; }
    public Color DeletedQuoteBorderColor { get; init; }
    public Color TableBorderColor { get; init; }
    public Color TableRowOddBackground { get; init; }
    public Color TableRowEvenBackground { get; init; }
    public Color CodeBackgroundColor { get; init; }
    public Color ErrorColor { get; init; }

    public static MarkdownTheme Create()
    {
        bool dark = Application.Current?.RequestedTheme == AppTheme.Dark;

        return new MarkdownTheme
        {
            BaseFontSize = 14,
            CodeFontSize = 13,
            ParagraphMargins = new Thickness(0, 0, 0, 12),
            ParagraphMarginTop = 0,
            ParagraphMarginBottom = 12,
            DefinitionMargin = 18,
            DefinitionSeparator = 10,
            ListContentMargin = 10,
            QuotePadding = 12,
            QuoteBorderWidth = 4,
            SuperscriptScale = 0.75,
            SuperscriptOffset = 6,
            TableCellPadding = new Thickness(10, 8),
            HeaderFontSizes = [28, 24, 20, 18, 16, 15],
            HeaderColors =
            [
                ResolveColor(dark ? "PrimaryDark" : "Primary", dark ? Colors.White : Color.FromArgb("#1F5D78")),
                ResolveColor(dark ? "PrimaryDark" : "Primary", dark ? Colors.White : Color.FromArgb("#1F5D78")),
                ResolveColor(dark ? "PrimaryDark" : "Primary", dark ? Colors.White : Color.FromArgb("#1F5D78")),
                ResolveColor("TextPrimary", dark ? Colors.White : Color.FromArgb("#17212B")),
                ResolveColor("TextPrimary", dark ? Colors.White : Color.FromArgb("#17212B")),
                ResolveColor("TextPrimary", dark ? Colors.White : Color.FromArgb("#17212B"))
            ],
            TextColor = ResolveColor("TextPrimary", dark ? Colors.White : Color.FromArgb("#17212B")),
            LinkColor = ResolveColor(dark ? "PrimaryDark" : "Primary", dark ? Color.FromArgb("#D7E9F0") : Color.FromArgb("#1F5D78")),
            QuoteBorderColor = ResolveColor("Info", Color.FromArgb("#245D78")),
            InsertedQuoteBorderColor = ResolveColor("Success", Color.FromArgb("#2E6E49")),
            DeletedQuoteBorderColor = ResolveColor("Danger", Color.FromArgb("#A14141")),
            TableBorderColor = ResolveColor("BorderSubtle", Color.FromArgb("#D1DBE1")),
            TableRowOddBackground = ResolveColor(dark ? "Gray900" : "Surface", dark ? Color.FromArgb("#212121") : Colors.White),
            TableRowEvenBackground = ResolveColor(dark ? "Gray800" : "SurfaceMuted", dark ? Color.FromArgb("#262626") : Color.FromArgb("#EDF2F4")),
            CodeBackgroundColor = ResolveColor(dark ? "Gray950" : "MonoSurface", dark ? Color.FromArgb("#141414") : Color.FromArgb("#F6F8F9")),
            ErrorColor = ResolveColor("Danger", Color.FromArgb("#A14141"))
        };
    }

    private static Color ResolveColor(string resourceKey, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out object? value) == true)
        {
            if (value is Color color)
                return color;

            if (value is SolidColorBrush brush)
                return brush.Color;
        }

        return fallback;
    }
}