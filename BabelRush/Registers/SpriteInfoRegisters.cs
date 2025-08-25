using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Gui.DisplayInfos.Animation;
using BabelRush.Registering;
using BabelRush.Registering.Misc;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class SpriteInfoRegisters
{
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string ShaderIncludes = "shaders/includes";
        public const string ShaderInfos = "shaders";
        public const string ShaderInstances = "shader_instances";
        public const string Textures = "textures";
        public const string Sprites = "sprites";
        public const string AnimationSets = "animations";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, ShaderInclude> ShaderIncludes { get; } =
        CreateSimpleRegister.CommonRes<ShaderInclude, ShaderIncludeModel>(Paths.ShaderIncludes, new ShaderInclude());

    public static IRegister<RegKey, ShaderInfo> ShaderInfos { get; } =
        CreateSimpleRegister.Res<ShaderInfo, ShaderInfoModel>(Paths.ShaderInfos, ShaderInfo.Default);

    public static IRegister<RegKey, ShaderInstance> ShaderInstances { get; } =
        CreateSimpleRegister.Res<ShaderInstance, ShaderInstanceModel>(Paths.ShaderInstances, ShaderInstance.Fallback);

    public static IRegister<RegKey, Texture2D> Textures { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>(Paths.Textures, new PlaceholderTexture2D());

    public static IRegister<RegKey, SpriteInfo> Sprites { get; } =
        CreateSimpleRegister.Res<SpriteInfo, SpriteInfoModel>(Paths.Sprites, SpriteInfo.Fallback);

    public static IRegister<RegKey, AnimationSet> AnimationSets { get; } =
        new AnimationSetRegister(Paths.AnimationSets)
           .AddToRegisterHub(Paths.AnimationSets);
}