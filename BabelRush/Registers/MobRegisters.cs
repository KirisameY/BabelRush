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

    [DataRegister<MobTypeModel, MobType>("mobs")]
    private static readonly CommonRegister<MobType> MobsRegister =
        new(nameof(MobsRegister), _ => MobType.Default);
    
    // Mob animation data
    private static readonly MobAnimationSetRegister MobAnimationSetRegister = new(MobAnimationSet.Default);
    public static IRegister<MobAnimationSet> MobAnimationSets => MobAnimationSetRegister;
    
    static partial void _Register()
    {
        FileLoader.AddDefaultResRegistrant("textures/mobs", new(new MobAnimationDefaultRegistrant(MobAnimationSetRegister)));
        FileLoader.AddLocalResRegistrant("textures/mobs", new(new MobAnimationLocalizedRegistrant(MobAnimationSetRegister)));
    }
}