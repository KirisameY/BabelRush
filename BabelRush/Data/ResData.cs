using System.Collections.Generic;

namespace BabelRush.Data;

public record ResData(string Id, Godot.FileAccess? File, IDictionary<string, object>? Data);