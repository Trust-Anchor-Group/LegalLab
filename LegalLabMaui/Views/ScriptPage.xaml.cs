using LegalLabMaui.Models.Script;
using System.Collections.Specialized;
using System.Linq;

#if WINDOWS
using Windows.System;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
#endif

namespace LegalLabMaui.Views;

public partial class ScriptPage : ContentPage
{
    private ScriptModel? attachedModel;
    private bool executingFromEditor;

#if WINDOWS
    private TextBox? nativeInputEdit;
    private KeyEventHandler? nativeKeyDownHandler;
#endif

    public ScriptPage()
    {
        InitializeComponent();

#if WINDOWS
        InputEdit.HandlerChanged += InputEdit_HandlerChanged;
        InputEdit.HandlerChanging += InputEdit_HandlerChanging;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.ScriptModel;

        if (BindingContext is ScriptModel model && !ReferenceEquals(this.attachedModel, model))
        {
            if (this.attachedModel is not null)
                this.attachedModel.History.CollectionChanged -= History_CollectionChanged;

            this.attachedModel = model;
            this.attachedModel.History.CollectionChanged += History_CollectionChanged;
        }
    }

    protected override void OnDisappearing()
    {
        if (this.attachedModel is not null)
        {
            this.attachedModel.History.CollectionChanged -= History_CollectionChanged;
            this.attachedModel = null;
        }

#if WINDOWS
        DetachNativeInputEdit();
#endif

        base.OnDisappearing();
    }

    private async void History_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {/*
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (this.attachedModel?.History.FirstOrDefault() is ScriptHistoryItem first)
                HistoryCollectionView.ScrollTo(first, position: ScrollToPosition.Start, animate: true);
        });
        */
    }

    private void HistoryCollectionView_SelectionChanged(
        object? sender,
        Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ScriptHistoryItem item)
            return;

        string script = item.Script ?? string.Empty;

        if (BindingContext is ScriptModel model)
            model.Input = script;
        else
            InputEdit.Text = script;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            InputEdit.CursorPosition = script.Length;
            InputEdit.SelectionLength = 0;
            HistoryCollectionView.SelectedItem = null;
            InputEdit.Focus();
        });
    }

#if WINDOWS
    private void InputEdit_HandlerChanged(object? sender, EventArgs e)
    {
        if (InputEdit.Handler?.PlatformView is not TextBox textBox)
            return;

        if (ReferenceEquals(this.nativeInputEdit, textBox))
            return;

        DetachNativeInputEdit();

        this.nativeInputEdit = textBox;
        this.nativeKeyDownHandler = NativeInputEdit_KeyDown;

        this.nativeInputEdit.AddHandler(
            UIElement.KeyDownEvent,
            this.nativeKeyDownHandler,
            true);
    }

    private void InputEdit_HandlerChanging(object? sender, HandlerChangingEventArgs e)
    {
        DetachNativeInputEdit();
    }

    private void DetachNativeInputEdit()
    {
        if (this.nativeInputEdit is not null && this.nativeKeyDownHandler is not null)
        {
            this.nativeInputEdit.RemoveHandler(UIElement.KeyDownEvent, this.nativeKeyDownHandler);
        }

        this.nativeInputEdit = null;
        this.nativeKeyDownHandler = null;
    }

    private void NativeInputEdit_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        bool shiftDown =
            (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) &
             Windows.UI.Core.CoreVirtualKeyStates.Down)
            == Windows.UI.Core.CoreVirtualKeyStates.Down;

        if (shiftDown)
            return; // allow Shift+Enter to insert a newline

        if (BindingContext is ScriptModel model && model.Run.CanExecute(null))
        {
            e.Handled = true;   // prevent the newline
            ExecuteEditorScript(model);
        }
    }

    private void ExecuteEditorScript(ScriptModel model)
    {
        if (this.executingFromEditor)
            return;

        string script = this.nativeInputEdit?.Text ?? InputEdit.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(script))
            return;

        this.executingFromEditor = true;
        try
        {
            InputEdit.Text = string.Empty;
            model.Execute(script);
        }
        finally
        {
            this.executingFromEditor = false;
        }
    }
#endif
}