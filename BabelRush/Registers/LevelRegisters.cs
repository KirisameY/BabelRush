using BabelRush.Scenery.Rooms;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static class LevelRegisters
{
    public static IRegister<RoomTemplate> RoomRegister { get; }

    // [DataRegister<RoomTemplateModel, RoomTemplate>("rooms")]
    // private static readonly CommonRegister<RoomTemplate> RoomRegister = new(_ => RoomTemplate.Default);
}