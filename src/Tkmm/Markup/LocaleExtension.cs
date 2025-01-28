// Copyright (c) Tkmm Team and Contributors. MIT.
// https://github.com/Ryubing/Tkmm/blob/master/LICENSE.txt
// 
// Modified by TKMM-Team

using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Tkmm.Markup;

internal class LocaleExtension(TkLocale key) : BasicMarkupExtension<string>
{
    public override string Name => "Translation";
    protected override string Value => Locale[key];

    protected override void ConfigureBindingExtension(CompiledBindingExtension bindingExtension) 
        => bindingExtension.Source = Locale;
}