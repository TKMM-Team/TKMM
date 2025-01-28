using Avalonia.Controls;

namespace Tkmm.VirtualKeyboard.Extensions;

public static class VirtualKeyboardExtensions
{
    public static void AddVirtualKeyboard(this Window shell)
    {
        shell.GotFocus += async (_, e) => {
            if (e.Source is TextBox textBox) {
                await VirtualKeyboard.OpenKeyboard(shell, textBox);
            }
        };
    }
}