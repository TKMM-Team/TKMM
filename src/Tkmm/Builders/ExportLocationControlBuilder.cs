using System.Reflection;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using ConfigFactory.Core;
using ConfigFactory.Generics;
using FluentAvalonia.UI.Controls;
using Tkmm.Controls;
using Tkmm.Core;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

internal partial class ExportLocationControlBuilder : ControlBuilder<ExportLocationControlBuilder>
{
    public override object Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        return new Button {
            Content = "Edit",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Command = EditCommand,
            DataContext = context,
            [!Button.CommandParameterProperty] = new Binding(propertyInfo.Name)
        };
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(ExportLocations);
    }

    [RelayCommand]
    public static async Task Edit(ExportLocations source)
    {
        TaskDialog dialog = new() {
            DataContext = source,
            Buttons = [
                TaskDialogButton.CloseButton
            ],
            Title = "Edit Export Locations",
            Content = new ExportLocationCollectionEditor(),
            XamlRoot = App.XamlRoot
        };

        await dialog.ShowAsync();
        TKMM.Config.Save();
    }
}
