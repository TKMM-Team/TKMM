using MenuFactory.Abstractions;
using Tkmm.Models.MenuModels;

namespace Tkmm.Extensions;

public static class MenuExtension
{
    public static void ConfigureMenu(this IMenuFactory factory)
    {
        factory.AddMenuGroup<FileMenuModel>();
        factory.AddMenuGroup<ModMenuModel>();
        factory.AddMenuGroup<ToolsMenuModel>();
#if DEBUG
        factory.AddMenuGroup<DebugMenuModel>();
#endif
        factory.AddMenuGroup<HelpMenuModel>();
    }
}