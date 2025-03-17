using BabelRush.Registering;
using BabelRush.Scenery.Rooms;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class LevelRegisters
{
    public static IRegister<RoomTemplate> RoomRegister { get; } =
        CreateSimpleRegister.Data<RoomTemplate, RoomTemplateModel>("rooms", RoomTemplate.Default);
}