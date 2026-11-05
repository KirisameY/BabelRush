using Tomlyn;
using Tomlyn.Serialization;

namespace BabelRushR.Core.Common.Serializing;

// ReSharper disable once TypeParameterCanBeVariant
public interface ICollectableConverter<TSelf> where TSelf : TomlConverter, ICollectableConverter<TSelf>
{
    public static abstract TSelf Create();
}

// ReSharper disable once TypeParameterCanBeVariant
public interface ICollectableTypeInfo<TSelf> where TSelf : TomlTypeInfo, ICollectableTypeInfo<TSelf>
{
    public static abstract TSelf Create(TomlSerializerOptions options);
}