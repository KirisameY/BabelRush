using BabelRushR.Core.Card.Types;
using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.Entity.Types;

[DataRegisterInitialize]
public class EntityType
{
    public static ILogger Logger => field ??= Logging.Logger(typeof(CardType));

    public static IEntityType Fallback => PlaceHolderEntityType.Instance;

    public static IEnumerableRegistry<IEntityType> Registry { get; } =
        new RegistryBuilder<IEntityType>()
           .WithFallback(RegistryCreatUtils.CreateFallback(Fallback, Logger, "EntityType"))
           .AddRegistrant(RegistryCreatUtils.ListDataRegistrant<IEntityType>("/entities", "entity", c => c.ID))
           // .AddRegistrant(ConstantEntityTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}