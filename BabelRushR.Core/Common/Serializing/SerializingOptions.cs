using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
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

    /// <summary>收集程序集内的多态、转换器和类型信息，构造最终的 <see cref="Default"/>。</summary>
    internal static void CollectOptions()
    {
        if (Interlocked.Exchange(ref _initialized, true)) throw new InvalidOperationException("Already initialized");

        var polymorphismMappings = CollectPolymorphismMappings();
        var converters = CollectConverters();
        var typeInfos = CollectTypeInfos();

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


    #region Collect

    private static FrozenDictionary<Type, IReadOnlyList<TomlDerivedType>> CollectPolymorphismMappings()
    {
        var mappings = new Dictionary<Type, List<TomlDerivedType>>();

        foreach (var derivedType in AttributedTypeScanner.FindTypesWithAttribute<TomlBaseTypeAttribute>())
        {
            AttributedTypeScanner.ValidateConcreteClass(derivedType, "TOML derived type");

            foreach (var attribute in derivedType.GetCustomAttributes<TomlBaseTypeAttribute>(inherit: false))
            {
                var baseType = attribute.BaseType;
                ValidatePolymorphicBase(baseType, derivedType);

                if (string.IsNullOrWhiteSpace(attribute.Discriminator))
                {
                    throw new InvalidOperationException(
                        $"TOML derived type '{derivedType.FullName}' has an empty discriminator for base '{baseType.FullName}'.");
                }

                AddDerivedTypeMapping(mappings, baseType, derivedType, attribute.Discriminator);
            }
        }

        var result = new Dictionary<Type, IReadOnlyList<TomlDerivedType>>(mappings.Count);
        foreach (var (baseType, derivedTypes) in mappings)
        {
            result[baseType] = [..derivedTypes];
        }
        return result.ToFrozenDictionary();
    }

    private static void ValidatePolymorphicBase(Type baseType, Type derivedType)
    {
        // Tomlyn 只支持 class / interface 作为多态基类型。
        if (!baseType.IsClass && !baseType.IsInterface)
        {
            throw new InvalidOperationException(
                $"TOML polymorphic base '{baseType.FullName}' of '{derivedType.FullName}' must be a class or an interface.");
        }

        if (baseType.ContainsGenericParameters)
        {
            throw new InvalidOperationException(
                $"TOML polymorphic base '{baseType.FullName}' of '{derivedType.FullName}' must not be an open generic type.");
        }

        if (baseType == derivedType)
        {
            throw new InvalidOperationException(
                $"TOML type '{derivedType.FullName}' cannot be registered as a derived type of itself.");
        }

        if (!baseType.IsAssignableFrom(derivedType))
        {
            throw new InvalidOperationException(
                $"TOML type '{derivedType.FullName}' is not assignable to its declared base '{baseType.FullName}'.");
        }
    }

    private static void AddDerivedTypeMapping(
        Dictionary<Type, List<TomlDerivedType>> mappings, Type baseType, Type derivedType, string discriminator)
    {
        if (!mappings.TryGetValue(baseType, out var derivedTypes))
        {
            mappings[baseType] = derivedTypes = [];
        }

        if (derivedTypes.Any(t => string.Equals(t.Discriminator, discriminator, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Multiple TOML derived types are registered with discriminator '{discriminator}' for base '{baseType.FullName}'.");
        }

        derivedTypes.Add(new TomlDerivedType(derivedType, discriminator));
    }

    private static List<TomlConverter> CollectConverters()
    {
        // 标特性用默认构造；实现接口用 Create()。
        var converterTypes = OrderTypes(
            AttributedTypeScanner.FindTypesWithAttribute<TomlConverterForAttribute>()
                               .Concat(AttributedTypeScanner.FindTypesImplementingGeneric(typeof(ICollectableConverter<>)))
                               .Distinct()
        );

        var converters = new List<TomlConverter>();
        foreach (var converterType in converterTypes)
        {
            AttributedTypeScanner.ValidateRegistrationType(converterType, typeof(TomlConverter), "TOML converter");
            converters.Add(ImplementsSelfFactory(converterType, typeof(ICollectableConverter<>))
                ? CreateCollectableConverter(converterType)
                : CreateDefaultConverter(converterType));
        }
        return converters;
    }

    private static TomlConverter CreateCollectableConverter(Type converterType)
    {
        try
        {
            var method = typeof(SerializingOptions)
                        .GetMethod(nameof(CreateCollectableConverterCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(converterType);
            return method.Invoke(null, null) as TomlConverter
                ?? throw new InvalidOperationException(
                       $"TOML converter '{converterType.FullName}' returned null from ICollectableConverter<{converterType.Name}>.Create().");
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create TOML converter '{converterType.FullName}'.", ex.InnerException ?? ex);
        }
    }

    private static TSelf CreateCollectableConverterCore<TSelf>()
        where TSelf : TomlConverter, ICollectableConverter<TSelf> => TSelf.Create();

    private static TomlConverter CreateDefaultConverter(Type converterType)
    {
        if (converterType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new InvalidOperationException(
                $"TOML converter '{converterType.FullName}' must declare a public parameterless constructor "
              + $"or implement ICollectableConverter<{converterType.Name}>.");
        }

        try
        {
            return (TomlConverter)Activator.CreateInstance(converterType)!;
        }
        catch (Exception ex) when (ex is TargetInvocationException or MemberAccessException)
        {
            throw new InvalidOperationException(
                $"Failed to create TOML converter '{converterType.FullName}'.", ex.InnerException ?? ex);
        }
    }

    private static FrozenDictionary<Type, Func<TomlSerializerOptions, TomlTypeInfo>> CollectTypeInfos()
    {
        var factories = new Dictionary<Type, Func<TomlSerializerOptions, TomlTypeInfo>>();

        foreach (var typeInfoType in AttributedTypeScanner.FindTypesWithAttribute<TomlTypeInfoAttribute>())
        {
            AttributedTypeScanner.ValidateRegistrationType(typeInfoType, typeof(TomlTypeInfo), "TOML type info");

            if (!ImplementsSelfFactory(typeInfoType, typeof(ICollectableTypeInfo<>)))
            {
                throw new InvalidOperationException(
                    $"TOML type info '{typeInfoType.FullName}' must implement ICollectableTypeInfo<{typeInfoType.Name}>.");
            }

            foreach (var attribute in typeInfoType.GetCustomAttributes<TomlTypeInfoAttribute>(inherit: false))
            {
                var targetType = attribute.Type;
                if (targetType.ContainsGenericParameters)
                {
                    throw new InvalidOperationException(
                        $"TOML type info '{typeInfoType.FullName}' targets an open generic type '{targetType.FullName}'.");
                }

                if (factories.ContainsKey(targetType))
                {
                    throw new InvalidOperationException(
                        $"Multiple TOML type infos are registered for type '{targetType.FullName}'.");
                }

                factories[targetType] = CreateTypeInfoFactory(typeInfoType);
            }
        }

        return factories.ToFrozenDictionary();
    }

    private static Func<TomlSerializerOptions, TomlTypeInfo> CreateTypeInfoFactory(Type typeInfoType)
    {
        try
        {
            var method = typeof(SerializingOptions)
                        .GetMethod(nameof(CreateTypeInfoFactoryCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(typeInfoType);
            return method.Invoke(null, null) as Func<TomlSerializerOptions, TomlTypeInfo>
                ?? throw new InvalidOperationException($"Failed to create TOML type info factory for '{typeInfoType.FullName}'.");
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create TOML type info factory for '{typeInfoType.FullName}'.", ex.InnerException ?? ex);
        }
    }

    private static Func<TomlSerializerOptions, TomlTypeInfo> CreateTypeInfoFactoryCore<TSelf>()
        where TSelf : TomlTypeInfo, ICollectableTypeInfo<TSelf>
        => options => TSelf.Create(options);

    private static bool ImplementsSelfFactory(Type type, Type genericInterfaceDefinition)
    {
        return type.GetInterfaces().Any(implemented =>
        {
            if (!implemented.IsGenericType) return false;
            if (implemented.GetGenericTypeDefinition() != genericInterfaceDefinition) return false;
            var arguments = implemented.GetGenericArguments();
            return arguments.Length == 1 && arguments[0] == type;
        });
    }

    private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types) => types
       .OrderBy(type => type.Assembly.GetName().Name ?? string.Empty, StringComparer.Ordinal)
       .ThenBy(type => type.FullName ?? type.Name, StringComparer.Ordinal);

    #endregion


    private readonly struct TypeInfoResolver(FrozenDictionary<Type, Func<TomlSerializerOptions, TomlTypeInfo>> typeInfos) : ITomlTypeInfoResolver
    {
        public TomlTypeInfo? GetTypeInfo(Type type, TomlSerializerOptions options) => typeInfos.GetValueOrDefault(type)?.Invoke(options);
    }
}
