using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;
using BabelRush.Registering;

using Godot;

using JetBrains.Annotations;

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

    [RegistrationMap] [UsedImplicitly]
    private static DataRegTool[] DataRegTools { get; } =
    [
        DataRegTool.Get<MobType, MobTypeData>("mobs", MobsRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static ResRegTool[] ResRegTools { get; } = [];

    [RegistrationMap] [UsedImplicitly]
    private static LangRegTool[] LangRegTools { get; } =
    [
        LangRegTool.Get<NameDesc, IDictionary<string, object>>("mobs", NameDesc.FromEntry, MobNameDescRegister),
    ];

    #endregion
}