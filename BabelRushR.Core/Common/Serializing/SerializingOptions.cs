using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;

using Tomlyn;
using Tomlyn.Serialization;

namespace BabelRushR.Core.Common.Serializing;

public static class SerializingOptions
{
    public static TomlSerializerOptions DefaultBeforeInitialize { get; } = TomlSerializerOptions.Default with
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        IndentSize = 4,
        Converters = [RegKeyConverter.Default],
    };


    private static bool _initialized = false;

    internal static void CollectOptions()
    {
        if (Interlocked.Exchange(ref _initialized, true)) throw new InvalidOperationException("Already initialized");

        var polymorphismMappings = FrozenDictionary<Type, IReadOnlyList<TomlDerivedType>>.Empty;
        var converters = new List<TomlConverter>();
        var typeInfos = FrozenDictionary<Type, Func<TomlSerializerOptions, TomlTypeInfo>>.Empty;

        Default = TomlSerializerOptions.Default with
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            IndentSize = 4,
            PolymorphismOptions = new()
            {
                DerivedTypeMappings           = polymorphismMappings,
                TypeDiscriminatorPropertyName = "$type",
                UnknownDerivedTypeHandling    = TomlUnknownDerivedTypeHandling.Fail
            },
            Converters = [RegKeyConverter.Default, ..converters],
            TypeInfoResolver = new TypeInfoResolver(typeInfos),
        };
    }

    public static TomlSerializerOptions Default
    {
        get => field ?? throw new InvalidOperationException(
            $"Serializing options is not initialized yet, consider use {nameof(DefaultBeforeInitialize)} instead."
        );
        private set;
    }

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

    private readonly struct TypeInfoResolver(FrozenDictionary<Type, Func<TomlSerializerOptions, TomlTypeInfo>> typeInfos) : ITomlTypeInfoResolver
    {
        public TomlTypeInfo? GetTypeInfo(Type type, TomlSerializerOptions options) => typeInfos.GetValueOrDefault(type)?.Invoke(options);
    }
}