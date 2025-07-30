using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;
using BabelRush.Scripting;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

using NLua;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MiscRegisters
{
    private const string ModulePath = "_module";
    public static IRegister<RegKey, LuaTable?> Modules { get; } =
        new RegisterBuilder<RegKey, LuaTable?>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(ScriptRootLoader.WithReservedSourceTaker(ModulePath, new SourceTakerRegistrant<ScriptSourceInfo, ScriptModuleModel, LuaTable>()))
           .AddRegistrant(ManualRegistrant.Common<LuaTable?>(RootNames.Script, ModulePath))
           .WithFallback((LuaTable?)null)
           .Build();
}