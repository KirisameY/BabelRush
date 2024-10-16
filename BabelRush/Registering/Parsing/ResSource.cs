using System.Collections.Generic;

using Godot;

namespace BabelRush.Registering.Parsing;

public readonly record struct ResSource(string Id, Dictionary<string, FileAccess> Files, IDictionary<string, object>? Data);