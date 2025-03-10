using BabelRush.Data.ExtendModels;
using BabelRush.Gui.Misc;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LocalInfoRegisters
{
    public static IRegister<Font> Fonts { get; } =
        SimpleRegisterCreate.Res<Font, FontModel>("fonts/res", new FontVariation());
    public static IRegister<FontInfo> FontInfos { get; } =
        SimpleRegisterCreate.Res<FontInfo, FontInfoModel>("fonts", new FontInfo("", 12));
}