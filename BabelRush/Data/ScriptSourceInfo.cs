using NLua;

namespace BabelRush.Data;

public record struct ScriptSourceInfo(string Id, LuaFunction Script);