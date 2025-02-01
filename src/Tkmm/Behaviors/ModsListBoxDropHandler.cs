using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Tkmm.Core;
using TkSharp.Core.Models;

namespace Tkmm.Behaviors;

public class ModsListBoxDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is not ListBox { ItemsSource: IList list } listBox || listBox.GetVisualAt(e.GetPosition(listBox)) is not Control control) {
            return false;
        }

        Type? listBoxType = GetListBoxType(listBox);
        if (control.DataContext?.GetType() != listBoxType) {
            goto DefaultCheck;
        }

        int currentIndex = list.IndexOf(sourceContext);
        int targetIndex = list.IndexOf(control.DataContext);

        if (currentIndex == -1 || targetIndex == -1 || currentIndex == targetIndex) {
            goto DefaultCheck;
        }

        (list[currentIndex], list[targetIndex]) = (list[targetIndex], list[currentIndex]);
        listBox.SelectedIndex = targetIndex;

    DefaultCheck:
        return listBoxType == typeof(TkMod) || listBoxType == typeof(TkProfileMod);

    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is not ListBox { ItemsSource: IList list } listBox) {
            return false;
        }

        Type? targetType = GetListBoxType(listBox);

        if (sourceContext is TkMod mod && targetType == typeof(TkProfileMod) && mod.GetProfileMod() is TkProfileMod newProfileMod && !list.Contains(newProfileMod)) {
            list.Add(newProfileMod);
            return true;
        }

        if (sourceContext is not TkProfileMod profileMod || targetType != typeof(TkMod)) {
            return false;
        }

        TKMM.ModManager.GetCurrentProfile().Mods.Remove(profileMod);
        return true;

    }

    private static Type? GetListBoxType(ListBox sender)
    {
        IEnumerable? src = sender.ItemsSource;

        if (src is null) {
            return null;
        }

        Type srcType = src.GetType();
        return src.GetType().IsGenericType ? srcType.GetGenericArguments().FirstOrDefault() : null;
    }
}
