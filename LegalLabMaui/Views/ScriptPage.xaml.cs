using LegalLabMaui.Models.Script;

#if WINDOWS
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace LegalLabMaui.Views;

public partial class ScriptPage : ContentPage
{
    #if WINDOWS
    private UIElement? nativeInputView;
    #endif

    public ScriptPage()
    {
        InitializeComponent();

        #if WINDOWS
        InputEdit.HandlerChanged += InputEdit_HandlerChanged;
        #endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext ??= AppService.ScriptModel;

        #if WINDOWS
        AttachNativeInputHandler();
        #endif
    }

    protected override void OnDisappearing()
    {
        #if WINDOWS
        DetachNativeInputHandler();
        #endif

        base.OnDisappearing();
    }

    #if WINDOWS
    private void InputEdit_HandlerChanged(object? sender, EventArgs e)
    {
        AttachNativeInputHandler();
    }

    private void AttachNativeInputHandler()
    {
        if (InputEdit.Handler?.PlatformView is not UIElement nativeView)
            return;

        if (ReferenceEquals(this.nativeInputView, nativeView))
            return;

        DetachNativeInputHandler();

        this.nativeInputView = nativeView;
        this.nativeInputView.KeyDown += NativeInputView_KeyDown;
    }

    private void DetachNativeInputHandler()
    {
        if (this.nativeInputView is null)
            return;

        this.nativeInputView.KeyDown -= NativeInputView_KeyDown;
        this.nativeInputView = null;
    }

    private void NativeInputView_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        CoreVirtualKeyStates shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        CoreVirtualKeyStates controlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);

        if (shiftState.HasFlag(CoreVirtualKeyStates.Down) || controlState.HasFlag(CoreVirtualKeyStates.Down))
            return;

        if (BindingContext is ScriptModel model && model.Run.CanExecute(null))
        {
            e.Handled = true;
            model.Run.Execute(null);
        }
    }
    #endif
}
