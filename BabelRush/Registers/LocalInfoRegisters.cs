using BabelRush.Data.ExtendModels;
using BabelRush.Gui.Misc;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class LocalInfoRegisters
{
    [ResRegister<FontModel, Font>("fonts/res")]
    private static readonly CommonRegister<Font> FontRegister =
        new(_ => new FontVariation());

    [ResRegister<FontInfoModel, FontInfo>("fonts")]
    private static readonly CommonRegister<FontInfo> FontInfoRegister =
        new(_ => new FontInfo("", 12));
}