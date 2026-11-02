using System.Text.Json;

using Tomlyn;

namespace BabelRushR.Core.Common;

public static class Serializing
{
    public static TomlSerializerOptions DefaultOptions { get; } = TomlSerializerOptions.Default with
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}