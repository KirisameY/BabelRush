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
    public static IRegister<RegKey, Texture2D> RoomIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Textures, "room_icons");

    public static IRegister<RegKey, RoomTemplate> Rooms { get; } =
        CreateSimpleRegister.Data<RoomTemplate, RoomTemplateModel>("rooms", RoomTemplate.Default);


    public static IRegister<RegKey, StageTemplate> Stages { get; } =
        CreateSimpleRegister.Data<StageTemplate, StageTemplateModel>("stages", StageTemplate.Default);
}