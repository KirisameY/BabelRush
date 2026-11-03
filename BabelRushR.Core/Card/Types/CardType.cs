using BabelRushR.Core.Data;

using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

namespace BabelRushR.Core.Card.Types;

[DataRegisterInitialize]
public static class CardType
{
    public static ICardType Fallback => PlaceHolderCardType.Instance;

    public static IEnumerableRegistry<ICardType> Registry { get; } =
        new RegistryBuilder<ICardType>()
           .WithFallback(Fallback)
           .AddRegistrant(DataRegistrationManager.Attach(new ListedTomlDataRegistrant<ICardType>("/cards", "card", c => c.ID)))
           .AddRegistrant(ConstantCardTypes.Registrant)
           .WithRegisterDoneEventSource(DataRegistrationManager.DoneSource)
           .Build();
}