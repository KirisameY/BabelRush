using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registering.Registers;
using BabelRush.Scenery.Rooms;
using BabelRush.Scenery.Stages.Template;

using Godot;

using KirisameLib.Data.Registers;

using StageTemplateModel = BabelRush.Scenery.Stages.Template.StageTemplateModel;

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