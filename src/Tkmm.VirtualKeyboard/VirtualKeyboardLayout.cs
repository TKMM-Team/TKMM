using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;

namespace Tkmm.VirtualKeyboard;

public partial class VirtualKeyboardLayout : TemplatedControl
{
    private readonly HashSet<VirtualKey> _registered = [];
    
    public static readonly StyledProperty<bool> AcceptsReturnProperty = AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(AcceptsReturn));
    public static readonly StyledProperty<bool> IsShiftEnabledProperty = AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(IsShiftEnabled));

    public bool IsShiftEnabled {
        get => GetValue(IsShiftEnabledProperty);
        set => SetValue(IsShiftEnabledProperty, value);
    }
    
    public bool AcceptsReturn {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    protected VirtualKeyboardLayout()
    {
        PropertyChanged += (_, e) => {
            if (e.Property.Name != nameof(IsShiftEnabled)) {
                return;
            }
            
            UpdateKeyContents(IsShiftEnabled);
        };
    }

    public void RegisterKey(VirtualKey key)
    {
        _registered.Add(key);
    }

    private void UpdateKeyContents(bool isEnabled)
    {
        foreach (VirtualKey key in _registered) {
            key.Content = isEnabled ? key.ShiftKey : key.Key;
        }
    }
}