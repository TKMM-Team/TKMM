using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Microsoft.Extensions.Logging;
using Tkmm.Actions;
using TkSharp.Core;

namespace Tkmm.Behaviors;

public class InstallModDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        return true;
    }

    public override async void Drop(object? sender, DragEventArgs e, object? sourceContext, object? targetContext)
    {
        try {
            if (e.Data.GetFiles() is IEnumerable<IStorageItem> paths) {
                foreach (IStorageItem item in paths) {
                    switch (item) {
                        case IStorageFile file:
                            await using (Stream input = await file.OpenReadAsync()) {
                                await ModActions.Instance.Install(file.Name, input);
                            }
                            break;
                        case IStorageFolder folder when folder.TryGetLocalPath() is string folderPath:
                            await ModActions.Instance.Install(folderPath);
                            break;
                    }
                }
            }
            else if (e.Data.GetText() is string arg) {
                await ModActions.Instance.Install(arg);
            }
        }
        catch (Exception ex) {
            TkLog.Instance.LogError(ex, "Failed to install drag/drop target: {TargetData}.", e.Data);
        }
    }
}