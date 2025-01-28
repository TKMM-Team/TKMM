using FluentAvalonia.UI.Controls;

namespace Tkmm.Dialogs;

public enum MessageDialogButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public static class MessageDialog
{
    public static async Task<ContentDialogResult> Show(object? content, string? title = null, MessageDialogButtons buttons = MessageDialogButtons.Ok)
    {
        ContentDialog dialog = new() {
            PrimaryButtonText = buttons switch {
                MessageDialogButtons.Ok or MessageDialogButtons.OkCancel => "Ok",
                MessageDialogButtons.YesNo or MessageDialogButtons.YesNoCancel => "Yes",
                _ => string.Empty
            },
            SecondaryButtonText = buttons switch {
                MessageDialogButtons.YesNoCancel => "No",
                _ => string.Empty
            },
            IsSecondaryButtonEnabled = buttons switch {
                MessageDialogButtons.YesNoCancel => true,
                _ => false
            },
            CloseButtonText = buttons switch {
                MessageDialogButtons.YesNo => "No",
                MessageDialogButtons.OkCancel or MessageDialogButtons.YesNoCancel => "Cancel",
                _ => string.Empty
            },
            Title = title,
            Content = content
        };
        
        return await dialog.ShowAsync();
    }
}