namespace BabelRushR.Core.Common.Serializing;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class TomlBaseTypeAttribute(string discriminator) : Attribute
{
    public string Discriminator => discriminator;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TomlConverterForAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TomlTypeInfoAttribute(Type type) : Attribute
{
    public Type Type => type;
}

public class TomlTypeInfoAttribute<T>() : TomlTypeInfoAttribute(typeof(T));