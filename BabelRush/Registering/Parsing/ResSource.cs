using System.Collections.Generic;

namespace BabelRush.Registering.Parsing;

public readonly record struct ResSource(string Id, Godot.FileAccess? File, IDictionary<string, object>? Data)
{
    public static implicit operator ResSource((string Id, Godot.FileAccess? File, IDictionary<string, object>? Data) tuple) =>
        new(tuple.Id, tuple.File, tuple.Data);

    public static implicit operator (string Id, Godot.FileAccess? File, IDictionary<string, object>? Data)(ResSource source) =>
        (source.Id, source.File, source.Data);
}