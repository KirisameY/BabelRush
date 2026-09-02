using System.ComponentModel;

using BabelRushR.Core.Card;
using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay;

public interface IPlayerState : INotifyPropertyChanged
{
    IEntity PCEntity { get; }

    INotifiableList<ICard> HandPile { get; }
    INotifiableList<ICard> DrawPile { get; }
    INotifiableList<ICard> DiscardPile { get; }

    int MaxAP { get; }
    int AP { get; set; }
    double APRegenerated { get; }
    double APRegeneration { get; }
}