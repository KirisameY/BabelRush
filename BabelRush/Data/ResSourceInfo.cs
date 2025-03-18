using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using KirisameLib.Extensions;

namespace BabelRush.Data;

public readonly record struct ResSourceInfo(ImmutableArray<string> Dir, string Name)
{
    public string Path => Dir.Append(Name).Join('/');
    public Dictionary<string, byte[]> Files { get; } = new();
}