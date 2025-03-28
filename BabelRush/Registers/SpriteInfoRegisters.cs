using System;

using BabelRush.Data;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class SpriteInfoRegisters
{
    public static IRegister<RegKey, ShaderInclude> ShaderIncludes => throw new NotImplementedException();

    public static IRegister<RegKey, ShaderInfo> ShaderInfos => throw new NotImplementedException();

    public static IRegister<RegKey, ShaderInstance> ShaderInstances => throw new NotImplementedException();

    public static IRegister<RegKey, SpriteInfo> SpriteInfos => throw new NotImplementedException();
}