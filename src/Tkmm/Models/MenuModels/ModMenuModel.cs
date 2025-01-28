using Tkmm.Actions;
using Tkmm.Attributes;
using Tkmm.Builders;
using Tkmm.Core;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public class ModMenuModel
{
    [TkMenu(TkLocale.Menu_ModExportFile, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + E", Icon = "fa-solid fa-file-export")]
    public static Task Export()
    {
        return ModActions.Instance.ExportMod();
    }

    [TkMenu(TkLocale.Menu_ModConfigureOptions, TkLocale.Menu_Mod, InputGesture = "F4", Icon = "fa-sliders")]
    public static Task ConfigureOptions()
    {
        return ModActions.Instance.ConfigureModOptions();
    }

    [TkMenu(TkLocale.Menu_ModRemoveFromProfile, TkLocale.Menu_Mod, InputGesture = "Ctrl + Delete", Icon = "fa-regular fa-trash", IsSeparator = true)]
    public static Task RemoveFromProfile()
    {
        return ModActions.Instance.RemoveModFromProfile();
    }

    [TkMenu(TkLocale.Menu_ModUninstall, TkLocale.Menu_Mod, InputGesture = "Ctrl + Shift + Delete", Icon = "fa-solid fa-trash")]
    public static Task Uninstall()
    {
        return ModActions.Instance.UninstallMod();
    }

    [TkMenu(TkLocale.Menu_ModEditExportLocations, TkLocale.Menu_Mod, InputGesture = "Ctrl + L", Icon = "fa-regular fa-pen-to-square", IsSeparator = true)]
    public static async Task EditExportLocations()
    {
        // TODO: Abstract this to somewhere else
        await ExportLocationControlBuilder.Edit(TKMM.Config.ExportLocations);
    }

    [TkMenu(TkLocale.Menu_ModMerge, TkLocale.Menu_Mod, InputGesture = "Ctrl + M", Icon = "fa-solid fa-list-check")]
    public static Task MergeMods()
    {
        return MergeActions.Instance.Merge();
    }
}