using BabelRushR.Core.Registration;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;
using KirisameY.Registration.Registry;

namespace BabelRushR.Core.Card.Types;

[RegisterInitialize]
public static class CardType
{
    public static ICardType Fallback => PlaceHolderCardType.Instance;

    public static IEnumerableRegistry<ICardType> Registry { get; } =
        new RegistryBuilder<ICardType>()
           .WithFallback(Fallback)
           .AddRegistrant(ConstantCardTypes.Registrant)
           .WithRegisterDoneEventSource(RegistrationManager.DoneSource)
           .Build();
}