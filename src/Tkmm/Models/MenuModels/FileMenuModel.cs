using Tkmm.Actions;
using Tkmm.Attributes;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class FileMenuModel
{
    [TkMenu(TkLocale.Menu_FileInstallFile, TkLocale.Menu_File, InputGesture = "Ctrl + I", Icon = "fa-solid fa-file-import")]
    public static Task InstallFile()
    {
        return ImportActions.Instance.ImportFromFile();
    }

    [TkMenu(TkLocale.Menu_FileInstallFolder, TkLocale.Menu_File, InputGesture = "Ctrl + Shift + I", Icon = "fa-regular fa-folder-open")]
    public static Task InstallFolder()
    {
        return ImportActions.Instance.ImportFromFolder();
    }

    [TkMenu(TkLocale.Menu_FileInstallArg, TkLocale.Menu_File, InputGesture = "Ctrl + Alt + I", Icon = "fa-regular fa-keyboard")]
    public static Task ImportArgument()
    {
        return ImportActions.Instance.ImportFromArgument();
    }

    [TkMenu(TkLocale.Menu_FileClearTempFiles, TkLocale.Menu_File, InputGesture = "Ctrl + Shift + F6", Icon = "fa-solid fa-broom-wide", IsSeparator = true)]
    public static Task ClearTempFolder()
    {
        return SystemActions.Instance.CleanupTempFolder();
    }

    [TkMenu(TkLocale.Menu_FileExit, TkLocale.Menu_File, InputGesture = "Alt + F4", Icon = "fa-solid fa-right-from-bracket", IsSeparator = true)]
    public static Task Exit()
    {
        return SystemActions.Instance.SoftClose();
    }
}