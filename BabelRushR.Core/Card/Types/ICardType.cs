using System.Collections.Immutable;

using BabelRushR.Core.EntityAction.Types;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card.Types;

public interface ICardType
{
    RegKey Id { get; }
    int Cost { get; }
    ImmutableArray<(IEntityActionType Type, int Value)> Actions { get; }

    ICard CreateCard();
}