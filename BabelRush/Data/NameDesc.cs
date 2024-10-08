using System.Collections.Generic;

namespace BabelRush.Data;

public readonly struct NameDesc(string name, string desc)
{
    public string Name { get; } = name;
    public string Desc { get; } = desc;

    public static implicit operator (string Name, string Desc)(NameDesc nameDesc) => (nameDesc.Name, nameDesc.Desc);
    public static implicit operator NameDesc((string Name, string Desc) tuple) => new(tuple.Name, tuple.Desc);
}