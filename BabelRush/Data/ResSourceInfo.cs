using System.Collections.Generic;
using System.Collections.Immutable;

namespace BabelRush.Data;

public record struct ResSourceInfo(ImmutableArray<string> Path, string Id)
{
    public Dictionary<string, byte[]> Files { get; } = new();
}