using BabelRushR.Core.Card.Types;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

namespace BabelRushR.Core.EntityAction.Types;

[DataRegisterInitialize]
public class EntityActionType
{
    public static IEntityActionType Fallback => PlaceHolderEntityActionType.Instance;

    public static IEnumerableRegistry<IEntityActionType> Registry { get; } =
        new RegistryBuilder<IEntityActionType>()
           .WithFallback(Fallback)
           .AddRegistrant(ConstantEntityActionTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}