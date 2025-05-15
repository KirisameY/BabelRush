using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registering.Registers;
using BabelRush.Scenery.Rooms;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LevelRegisters
{
    public static IRegister<RegKey, Texture2D> RoomIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Textures, "room_icons");

    public static IRegister<RegKey, RoomTemplate> Rooms { get; } =
        CreateSimpleRegister.Data<RoomTemplate, RoomTemplateModel>("rooms", RoomTemplate.Default);
}