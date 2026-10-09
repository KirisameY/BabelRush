using System.Collections.Immutable;

using BabelRushR.Core.EntityAction;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card;

public record RecordCardType(RegKey Id, int Cost, ImmutableArray<(IEntityActionType Type, int Value)> Actions) : ICardType
{
    public ICard CreateCard() => new TypedCard(this);
}