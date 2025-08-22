using BabelRush.Data;
using BabelRush.Level.Rooms;
using BabelRush.Level.Stages.Template;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using Godot;

using KirisameLib.Data.Registers;

using RoomTemplateModel = BabelRush.Level.Rooms.RoomTemplateModel;
using StageTemplateModel = BabelRush.Level.Stages.Template.StageTemplateModel;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LevelRegisters
{
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Rooms = "rooms";
        // ReSharper disable once InconsistentNaming
        public const string Sub_RoomIcons = "room_icons";
        public const string RoomIcon = $"{SpriteInfoRegisters.Paths.Textures}/{Sub_RoomIcons}";
        public const string Stages = "stages";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, Texture2D> RoomIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Textures, Paths.Sub_RoomIcons);

    public static IRegister<RegKey, RoomTemplate> Rooms { get; } =
        CreateSimpleRegister.Data<RoomTemplate, RoomTemplateModel>(Paths.Rooms, RoomTemplate.Default);


    public static IRegister<RegKey, StageTemplate> Stages { get; } =
        CreateSimpleRegister.Data<StageTemplate, StageTemplateModel>(Paths.Stages, StageTemplate.Default);
}