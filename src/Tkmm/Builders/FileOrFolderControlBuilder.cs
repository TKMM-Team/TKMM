using System.Reflection;
using ConfigFactory.Core;
using ConfigFactory.Generics;
using Tkmm.Core.Models;

namespace Tkmm.Builders;

public class FileOrFolderControlBuilder : ControlBuilder<FileOrFolderControlBuilder>
{
    public override object? Build(IConfigModule context, PropertyInfo propertyInfo)
    {
        return new FileOrFolderControl {
            DataContext = new FileOrFolderControlContext(context, propertyInfo),
        };
    }

    public override bool IsValid(Type type)
    {
        return type == typeof(FileOrFolder);
    }
}