using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card.Types;

public class PlaceHolderCardType : ICardType
{
    private PlaceHolderCardType() { }
    public static PlaceHolderCardType Instance => field ??= new();

    public RegKey ID => field ??= RegKey.From("core", "404");

    public ICard CreateCard() => new PlaceHolderCard();
}