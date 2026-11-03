using System.Collections.Immutable;
using System.Text.Json;

using Tomlyn;

namespace BabelRushR.Core.Common.Serializing;

public static class SerializingOptions
{
    public static TomlSerializerOptions Default { get; } = TomlSerializerOptions.Default with
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        IndentSize = 4,
        Converters = [RegKeyConverter.Default],
    };

    private static readonly Dictionary<string, TomlSerializerOptions> _withDefaultNsCache = [];

    public static TomlSerializerOptions WithDefaultNamespace(string ns)
    {
        if (!_withDefaultNsCache.TryGetValue(ns, out var options))
        {
            _withDefaultNsCache[ns] = options = Default with
            {
                Converters = Default.Converters.ToImmutableList().Replace(RegKeyConverter.Default, new RegKeyConverter(ns))
            };
        }
        return options;
    }
}