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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Fonts = "fonts/res";
        public const string FontInfos = "fonts";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, Font> Fonts { get; } =
        CreateSimpleRegister.Res<Font, FontModel>(Paths.Fonts, new FontVariation());
    public static IRegister<RegKey, FontInfo> FontInfos { get; } =
        CreateSimpleRegister.Res<FontInfo, FontInfoModel>(Paths.FontInfos, new FontInfo(RegKey.Default, 12));
}