using System.Collections.Immutable;

using BabelRushR.Core.EntityAction.Types;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card.Types;

public class PlaceHolderCardType : ICardType
{
    private PlaceHolderCardType() { }
    public static PlaceHolderCardType Instance => field ??= new();

    public RegKey Id => RegKey.Default;
    public int Cost => 0;
    public ImmutableArray<(IEntityActionType Type, int Value)> Actions { get; } = [];

    public ICard CreateCard() => new TypedCard(this);
}