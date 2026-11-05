namespace BabelRushR.Core.Common.Serializing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class TomlBaseTypeAttribute(Type baseType, string discriminator) : Attribute
{
    public Type BaseType => baseType;
    public string Discriminator => discriminator;
}

public class TomlBaseTypeAttribute<TBase>(string discriminator) : TomlBaseTypeAttribute(typeof(TBase), discriminator);

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TomlConverterForAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TomlTypeInfoAttribute(Type type) : Attribute
{
    public Type Type => type;
}

public class TomlTypeInfoAttribute<T>() : TomlTypeInfoAttribute(typeof(T));
