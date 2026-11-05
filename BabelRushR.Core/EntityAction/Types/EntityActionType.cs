using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.EntityAction.Types;

[DataRegisterInitialize]
public class EntityActionType
{
    public static ILogger Logger => field ??= Logging.Logger(typeof(EntityActionType));

    public static IEntityActionType Fallback => PlaceHolderEntityActionType.Instance;

    public static IEnumerableRegistry<IEntityActionType> Registry { get; } =
        new RegistryBuilder<IEntityActionType>()
           .WithFallback(RegistryCreatUtils.CreateFallback(Fallback, Logger, "EntityActionType"))
           .AddRegistrant(ConstantEntityActionTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}