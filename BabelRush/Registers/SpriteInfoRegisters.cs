using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class SpriteInfoRegisters
{
    public static IRegister<RegKey, ShaderInclude> ShaderIncludes { get; } =
        CreateSimpleRegister.CommonRes<ShaderInclude, ShaderIncludeModel>("shaders/includes", new ShaderInclude());

    public static IRegister<RegKey, ShaderInfo> ShaderInfos { get; } =
        CreateSimpleRegister.Res<ShaderInfo, ShaderInfoModel>("shaders", ShaderInfo.Default);

    public static IRegister<RegKey, ShaderInstance> ShaderInstances { get; } =
        CreateSimpleRegister.Res<ShaderInstance, ShaderInstanceModel>("shader_instances", ShaderInstance.Fallback);

    public static IRegister<RegKey, Texture2D> Textures { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>("textures", new PlaceholderTexture2D());

    public static IRegister<RegKey, SpriteInfo> Sprites { get; } =
        CreateSimpleRegister.Res<SpriteInfo, SpriteInfoModel>("sprites", SpriteInfo.Fallback);
}