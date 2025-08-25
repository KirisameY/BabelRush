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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Modules = "_module";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, LuaTable?> Modules { get; } =
        new RegisterBuilder<RegKey, LuaTable?>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(ScriptRootLoader.WithReservedSourceTaker(Paths.Modules, new SourceTakerRegistrant<ScriptSourceInfo, ScriptModuleModel, LuaTable>()))
           .AddRegistrant(ManualRegistrant.Common<LuaTable?>(RootNames.Script, Paths.Modules))
           .WithFallback((LuaTable?)null)
           .Build()
           .AddToRegisterHubNullable(Paths.Modules);
}