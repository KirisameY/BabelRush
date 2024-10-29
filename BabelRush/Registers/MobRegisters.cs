using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;
using BabelRush.Registering;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class MobRegisters
{
    [LangRegister<NameDescModel, NameDesc>("mobs")]
    private static readonly LocalizedRegister<NameDesc> MobNameDescRegister =
        new(nameof(MobNameDescRegister), "en", id => (id, ""));

    [ResRegister<MobAnimationSetModel, MobAnimationSet>("textures/mobs/data")]
    private static readonly CommonRegister<MobAnimationSet> MobAnimationSetsRegister =
        new(nameof(MobAnimationSetsRegister), _ => MobAnimationSet.Default);

    [DataRegister<MobTypeModel, MobType>("mobs")]
    private static readonly CommonRegister<MobType> MobsRegister =
        new(nameof(MobsRegister), _ => MobType.Default);


    // public static IRegister<NameDesc> MobNameDesc => MobNameDescRegister;
    // public static IRegister<MobType> Mobs => MobsRegister;

    // public static IRegister<MobAnimationSet> MobAnimationSets { get; } =
    //     new CommonRegister<MobAnimationSet>(nameof(MobAnimationSets), _ => MobAnimationSet.Default);
    // public static IRegister<Texture2D> MobAnimationTextures { get; } =
    //     new CommonRegister<Texture2D>(nameof(MobAnimationTextures), _ => new PlaceholderTexture2D());
}