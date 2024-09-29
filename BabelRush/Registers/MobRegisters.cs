using BabelRush.Mobs;
using BabelRush.Mobs.Animation;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class MobRegisters
{
    public static IRegister<string> MobName { get; } =
        new I18nRegister<string>(nameof(MobName), id => id);
    public static IRegister<string> MobDesc { get; } =
        new I18nRegister<string>(nameof(MobDesc), _ => "");
    public static IRegister<MobAnimationSet> AnimationSets { get; } =
        new CommonRegister<MobAnimationSet>(nameof(AnimationSets), _ => MobAnimationSet.Default);
    public static IRegister<MobType> Mobs { get; } =
        new CommonRegister<MobType>(nameof(Mobs), _ => MobType.Default);
}