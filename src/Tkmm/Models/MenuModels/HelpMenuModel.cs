using Tkmm.Actions;
using Tkmm.Attributes;

// ReSharper disable UnusedMember.Global

namespace Tkmm.Models.MenuModels;

public sealed class HelpMenuModel
{
#if !SWITCH
    [TkMenu(TkLocale.Menu_Help, TkLocale.Menu_Help, InputGesture = "F1", Icon = TkIcons.CIRCLE_QUESTION)]
    public static Task Help()
    {
        return SystemActions.Instance.OpenDocumentationWebsite();
    }
#endif

    [TkMenu(TkLocale.Menu_HelpCheckForUpdates, TkLocale.Menu_Help, InputGesture = "Ctrl + U", Icon = TkIcons.CLOUD_ARROW_UP)]
    public static Task CheckForUpdates()
    {
        return SystemActions.Instance.CheckForUpdates(isAutoCheck: false);
    }

    [TkMenu(TkLocale.Menu_HelpAbout, TkLocale.Menu_Help, InputGesture = "F12", Icon = TkIcons.CIRCLE_INFO, IsSeparator = true)]
    public static Task About()
    {
        return SystemActions.Instance.ShowAboutDialog();
    }
}