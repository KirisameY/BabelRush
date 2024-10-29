using System.Collections.Generic;

namespace BabelRush.Data;

public record struct ResSourceInfo(string Id)
{
    public Dictionary<string, byte[]> Files { get; } = new();
}