using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using ZLogger;

namespace BabelRushR.Core.EntityAction.Types;

[DataRegisterInitialize]
public class EntityActionType
{
    public static IEntityActionType Fallback => PlaceHolderEntityActionType.Instance;

    public static IEnumerableRegistry<IEntityActionType> Registry { get; } =
        new RegistryBuilder<IEntityActionType>()
           .WithFallback(key =>
            {
                Logging.Logger(typeof(EntityActionType)).ZLogWarning($"EntityActionType {key} not found in registry, fallback value used.");
                return Fallback;
            })
           .AddRegistrant(ConstantEntityActionTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}