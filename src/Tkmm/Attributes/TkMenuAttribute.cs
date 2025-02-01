using MenuFactory.Abstractions.Attributes;

namespace Tkmm.Attributes;

public class TkMenuAttribute(TkLocale name, TkLocale path) : MenuAttribute(Enum.GetName(name)!, Enum.GetName(path)!);