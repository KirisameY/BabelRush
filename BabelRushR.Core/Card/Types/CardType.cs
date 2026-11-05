using BabelRushR.Core.Common;
using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

using Microsoft.Extensions.Logging;

namespace BabelRushR.Core.Card.Types;

[DataRegisterInitialize]
public static class CardType
{
    public static ILogger Logger => field ??= Logging.Logger(typeof(CardType));

    public static ICardType Fallback => PlaceHolderCardType.Instance;

    public static IEnumerableRegistry<ICardType> Registry { get; } =
        new RegistryBuilder<ICardType>()
           .WithFallback(RegistryCreatUtils.CreateFallback(Fallback, Logger, "CardType"))
           .AddRegistrant(RegistryCreatUtils.ListDataRegistrant<ICardType>("/cards","card", c=>c.ID))
           .AddRegistrant(ConstantCardTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}