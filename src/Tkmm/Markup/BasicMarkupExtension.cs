// Copyright (c) Tkmm Team and Contributors. MIT.
// https://github.com/Ryubing/Tkmm/blob/master/LICENSE.txt
// 
// Modified by TKMM-Team

using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Tkmm.Markup;

internal abstract class BasicMarkupExtension<T> : MarkupExtension
{
    public abstract string Name { get; }
    public virtual Action<object, T?>? Setter => null;

    protected abstract T? Value { get; }

    protected virtual void ConfigureBindingExtension(CompiledBindingExtension _) { }

    private ClrPropertyInfo PropertyInfo =>
        new(Name,
            _ => Value,
            Setter as Action<object, object?>,
            typeof(T));

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        CompiledBindingExtension ext = new(
            new CompiledBindingPathBuilder()
                .Property(PropertyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()
        );
            
        ConfigureBindingExtension(ext);
        return ext.ProvideValue(serviceProvider);
    } 
}