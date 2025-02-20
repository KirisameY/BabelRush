using BabelRush.Registering;
using BabelRush.Scenery.Rooms;

using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class LevelRegisters
{
    [DataRegister<RoomTemplateModel, RoomTemplate>("rooms")]
    private static readonly CommonRegister<RoomTemplate> RoomRegister = new(_ => RoomTemplate.Default);
}