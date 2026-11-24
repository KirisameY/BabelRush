using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.Level;

[DataRegisterInitialize]
public static class RoomDefinition
{
    public static ILogger Logger => field ??= Logging.Logger(typeof(RoomDefinition));

    public static IRoomDefinition Fallback => PlaceHolderRoomDefinition.Instance;

    public static IEnumerableRegistry<IRoomDefinition> Registry { get; } =
        new RegistryBuilder<IRoomDefinition>()
           .WithFallback(RegistryCreateUtils.CreateFallback(Fallback, Logger, "RoomDefinition"))
           .AddRegistrant(RegistryCreateUtils.ListDataRegistrant<IRoomDefinition>("/rooms", "room", r => r.ID))
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}