using System.Collections.Generic;

namespace BabelRush.Data;

public readonly record struct ResSource(string Id, Dictionary<string, byte[]> Files, IDictionary<string, object>? Data);