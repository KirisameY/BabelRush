using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

using FontInfoModel = BabelRush.Gui.DisplayInfos.FontInfoModel;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LocalInfoRegisters
{
    public static IRegister<RegKey, Font> Fonts { get; } =
        CreateSimpleRegister.Res<Font, FontModel>("fonts/res", new FontVariation());
    public static IRegister<RegKey, FontInfo> FontInfos { get; } =
        CreateSimpleRegister.Res<FontInfo, FontInfoModel>("fonts", new FontInfo(RegKey.Default, 12));
}