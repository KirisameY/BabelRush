using System.Collections.Immutable;

using BabelRushR.Core.EntityAction.Types;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card.Types;

public record RecordCardType(RegKey ID, int Cost, ImmutableArray<(IEntityActionType Type, int Value)> Actions) : ICardType
{
    public ICard CreateCard() => new RecordTypedCard(this);
}

public record DelayedRecordCardType(RegKey ID, int Cost, ImmutableList<DelayedRecordCardType.CardActionEntry>? Actions) : ICardType
{
    public ImmutableList<DelayedRecordCardType.CardActionEntry> Actions { get; } = Actions ??= [];

    private RecordCardType FinalType => field ??= new(ID, Cost, [..Actions.Select(entry => (EntityActionType.Registry[entry.Type], 0))]);
    public ICard CreateCard() => new RecordTypedCard(FinalType);

    public readonly record struct CardActionEntry(RegKey Type, int Value);
}