using BabelRush.Data.ExtendModels;
using BabelRush.Gui.Misc;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static class LocalInfoRegisters
{
    public static IRegister<Font> Fonts { get; }
    public static IRegister<FontInfo> FontInfos { get; }

    // [ResRegister<FontModel, Font>("fonts/res")]
    // private static readonly CommonRegister<Font> FontRegister =
    //     new(_ => new FontVariation());
    //
    // [ResRegister<FontInfoModel, FontInfo>("fonts")]
    // private static readonly CommonRegister<FontInfo> FontInfoRegister =
    //     new(_ => new FontInfo("", 12));
}