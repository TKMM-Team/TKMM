using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tkmm.Core.Models;

/// <summary>
/// Wrapper around a string to allow custom UI control building.
/// </summary>
[JsonConverter(typeof(FileOrFolderJsonConverter))]
public readonly struct FileOrFolder(string? path) : IEquatable<FileOrFolder>
{
    public readonly string? Path = path;
    
    public static implicit operator string?(FileOrFolder fileOrFolder) => fileOrFolder.Path;
    
    public static implicit operator FileOrFolder(string? path) => new(path);

    public bool Equals(FileOrFolder other)
    {
        return Path == other.Path;
    }

    public override bool Equals(object? obj)
    {
        return obj is FileOrFolder other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Path != null ? Path.GetHashCode() : 0;
    }

    public static bool operator ==(FileOrFolder left, FileOrFolder right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FileOrFolder left, FileOrFolder right)
    {
        return !(left == right);
    }
}

internal sealed class FileOrFolderJsonConverter : JsonConverter<FileOrFolder>
{
    public override FileOrFolder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, FileOrFolder value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Path);
    }
}