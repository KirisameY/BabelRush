using System.Collections.Immutable;

using BabelRushR.Core.EntityAction.Types;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card.Types;

public record RecordCardType(RegKey Id, int Cost, ImmutableArray<(IEntityActionType Type, int Value)> Actions) : ICardType
{
    public ICard CreateCard() => new TypedCard(this);
}