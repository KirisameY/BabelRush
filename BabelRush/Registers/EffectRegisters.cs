using BabelRush.Data;
using BabelRush.Effects;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class EffectRegisters
{
    // public static IRegister<RegKey, EffectBehaviorTemplate> EffectBehaviors { get; } =
    //     new RegisterBuilder<RegKey, EffectBehaviorTemplate>()
    //        .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
    //        .AddRegistrant(HookSourceLoader.OnScript(PATH, new SourceTakerRegistrant<ScriptSourceInfo, MODEL, EffectBehaviorTemplate>()))
    //        .AddRegistrant(HookSourceLoader.OnScript(PATH, new SourceTakerRegistrant<ScriptSourceInfo, MODEL, EffectBehaviorTemplate>()))
    //        .AddRegistrant(ManualRegistrant.Common<EffectBehaviorTemplate>(RootNames.Script, PATH))
    //        .WithFallback(EffectBehaviorTemplate.Default)
    //        .Build();

    public static IRegister<RegKey, EffectScript> EffectScripts { get; } =
        CreateSimpleRegister.Script<EffectScript, EffectScriptModel>("effects", EffectScript.Default);

    public static IRegister<RegKey, NameDesc> EffectNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("effects", "en", id => (id, ""));

    public static IRegister<RegKey, SpriteInfo> EffectIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Sprites, "effects");

    public static IRegister<RegKey, EffectType> Effects { get; } =
        CreateSimpleRegister.Data<EffectType, EffectTypeModel>("effects", EffectType.Default);
}