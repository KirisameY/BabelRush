using System.Collections.Immutable;

using NLua;

namespace BabelRush.Data;

public record struct ScriptSourceInfo(ImmutableArray<string> Path, LuaFunction Script);