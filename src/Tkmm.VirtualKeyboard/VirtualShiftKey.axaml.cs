using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Tkmm.VirtualKeyboard;

public class VirtualShiftKey : ToggleButton
{
    public static readonly StyledProperty<object?> OnContentProperty = AvaloniaProperty.Register<VirtualShiftKey, object?>(nameof(OnContent));
    public static readonly StyledProperty<object?> OffContentProperty = AvaloniaProperty.Register<VirtualShiftKey, object?>(nameof(OffContent));

    public object? OnContent {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }
    
    public object? OffContent {
        get => GetValue(OffContentProperty);
        set {
            SetValue(OffContentProperty, value);
            Content ??= value;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SetContent();
    }

    protected override void OnIsCheckedChanged(RoutedEventArgs e)
    {
        base.OnIsCheckedChanged(e);
        SetContent();
    }

    private void SetContent()
    {
        Content = IsChecked switch {
            true => OnContent,
            _ => OffContent
        };
    }
}