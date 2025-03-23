using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Scenery.Rooms;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LevelRegisters
{
    public static IRegister<RegKey, RoomTemplate> RoomRegister { get; } =
        CreateSimpleRegister.Data<RoomTemplate, RoomTemplateModel>("rooms", RoomTemplate.Default);
}