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
    public static IRegister<RegKey, ShaderInclude> ShaderIncludes =>
        CreateSimpleRegister.CommonRes<ShaderInclude, ShaderIncludeModel>("shaders/includes", new ShaderInclude());

    public static IRegister<RegKey, ShaderInfo> ShaderInfos =>
        CreateSimpleRegister.Res<ShaderInfo, ShaderInfoModel>("shaders", ShaderInfo.Default);

    public static IRegister<RegKey, ShaderInstance> ShaderInstances =>
        CreateSimpleRegister.Res<ShaderInstance, ShaderInstanceModel>("shader_instances", ShaderInstance.Default);

    public static IRegister<RegKey, Texture2D> Textures { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>("textures", new PlaceholderTexture2D());

    public static IRegister<RegKey, SpriteInfo> SpriteInfos =>
        CreateSimpleRegister.Res<SpriteInfo, SpriteInfoModel>("sprites", SpriteInfo.Default);
}