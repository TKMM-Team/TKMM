using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Tkmm.VirtualKeyboard.Layouts;

namespace Tkmm.VirtualKeyboard;

public partial class VirtualKeyboard : TemplatedControl
{
    private const string INPUT_TEXT_BOX_NAME = "PART_VirtualKeyboardInput";

    private static readonly DialogHost _host = new();
    private static TextBox? _last;

    public static async ValueTask OpenKeyboard(Window root, TextBox src)
    {
        if (src.Name == INPUT_TEXT_BOX_NAME || _last == src) {
            return;
        }

        _last = src;
        string? original = src.Text;

        VirtualKeyboard keyboard = new() {
            Source = src,
            KeyboardLayout = new KeyboardLayoutUS {
                AcceptsReturn = src.AcceptsReturn
            }
        };

        _host.Content = keyboard;

        var overlay = OverlayLayer.GetOverlayLayer(root);
        if (overlay is null) {
            throw new InvalidOperationException("Unable to find OverlayLayer from the provided Window");
        }

        overlay.Children.Add(_host);

        keyboard.DataContext = src;
        keyboard[!VirtualTextProperty] = new Binding(nameof(src.Text), BindingMode.TwoWay);
        keyboard[!TextPositionProperty] = new Binding(nameof(src.CaretIndex), BindingMode.TwoWay);

        if (!await keyboard.WaitForExit()) {
            src.Text = original;
        }

        overlay.Children.Remove(_host);
        _last = null;
    }

    private readonly TaskCompletionSource<bool> _exitCompletionSource = new();

    public static readonly StyledProperty<VirtualKeyboardLayout> KeyboardLayoutProperty = AvaloniaProperty.Register<VirtualKeyboard, VirtualKeyboardLayout>(nameof(KeyboardLayout));

    public VirtualKeyboardLayout KeyboardLayout {
        get => GetValue(KeyboardLayoutProperty);
        set => SetValue(KeyboardLayoutProperty, value);
    }

    public static readonly StyledProperty<string?> VirtualTextProperty = AvaloniaProperty.Register<VirtualKeyboard, string?>(nameof(VirtualText));

    public string? VirtualText {
        get => GetValue(VirtualTextProperty);
        set => SetValue(VirtualTextProperty, value);
    }

    public static readonly StyledProperty<int> TextPositionProperty = AvaloniaProperty.Register<VirtualKeyboard, int>(nameof(TextPosition));

    public int TextPosition {
        get => GetValue(TextPositionProperty);
        set => SetValue(TextPositionProperty, value);
    }

    public required TextBox Source { get; init; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var virtualTextBox = e.NameScope.Get<TextBox>(INPUT_TEXT_BOX_NAME);
        virtualTextBox.Focus();
    }

    [RelayCommand]
    public void Exit(bool result)
    {
        if (result && !KeyboardLayout.AcceptsReturn) {
            SimulateKeyPress(Key.Enter);
        }

        _exitCompletionSource.TrySetResult(result);
    }

    public async ValueTask<bool> WaitForExit()
    {
        return await _exitCompletionSource.Task;
    }
    
    internal void SimulateKeyPress(Key key)
    {
        var keyEventArgs = new KeyEventArgs {
            RoutedEvent = KeyDownEvent,
            Key = key,
            Source = Source,
            KeyModifiers = KeyModifiers.None
        };

        Source.RaiseEvent(keyEventArgs);
    }
}