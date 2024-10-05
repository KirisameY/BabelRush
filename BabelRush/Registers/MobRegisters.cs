using System.Collections.Frozen;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Misc;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;
using BabelRush.Registering;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MobRegisters
{
    #region Private Registers

    private static readonly LocalizedRegister<NameDesc> MobNameDescRegister =
        new(nameof(MobNameDescRegister), "en", id => (id, ""));

    private static readonly CommonRegister<MobType> MobsRegister =
        new(nameof(MobsRegister), _ => MobType.Default);

    #endregion


    #region Public Registers

    public static IRegister<NameDesc> MobNameDesc => MobNameDescRegister;
    public static IRegister<MobType> Mobs => MobsRegister;

    #endregion


    //todo: make this available
    public static IRegister<MobAnimationSet> MobAnimationSets { get; } =
        new CommonRegister<MobAnimationSet>(nameof(MobAnimationSets), _ => MobAnimationSet.Default);
    public static IRegister<Texture2D> MobAnimationTextures { get; } =
        new CommonRegister<Texture2D>(nameof(MobAnimationTextures), _ => new PlaceholderTexture2D());


    #region Map

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.ParseAndRegisterDelegate> RegisterMap { get; } =
        new Dictionary<string, Registration.ParseAndRegisterDelegate>
        {
            ["data/mobs"] = Registration.GetRegFunc<IDictionary<string, object>, MobTypeData>
                (MobTypeData.FromEntry,
                 data => MobsRegister.RegisterItem(data.Id, data.ToMobType()),
                 "data/???" //todo: set this
                ),
        }.ToFrozenDictionary();

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate> LocalizedRegisterMap { get; } =
        new Dictionary<string, Registration.LocalizedParseAndRegisterDelegate>
        {
            ["lang/mobs"] = Registration.GetLocalizedRegFunc<KeyValuePair<string, object>, NameDesc>
                (DataUtils.ParseNameDescAndId,
                 (local, id, nd) => MobNameDescRegister.RegisterLocalizedItem(local, id, nd)
                ),
        }.ToFrozenDictionary();

    #endregion
}