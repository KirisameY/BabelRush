using KirisameY.Registration.Data;

using Tomlyn.Serialization;

namespace BabelRushR.Core.Common.Serializing;

public class RegKeyConverter(string? defaultNameSpace = null) : TomlConverter<RegKey>
{
    public override RegKey Read(TomlReader reader) => reader.GetString().Split(':') switch
    {
        [var ns, var k]                           => RegKey.From(ns, k),
        [var k] when defaultNameSpace is not null => RegKey.From(defaultNameSpace, k),
        var somethingElse                         => throw reader.CreateException($"invalid RegKey string: {somethingElse}")
    };

    public override void Write(TomlWriter writer, RegKey value)
    {
        writer.WriteStringValue(value.NameSpace == defaultNameSpace ? value.NameSpace : value.FullName);
    }

    public static RegKeyConverter Default { get; } = new();
}