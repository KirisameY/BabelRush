using System.ComponentModel;

using BabelRushR.Core.Card;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay;

public interface IPlayerState : INotifyPropertyChanged, IUpdatable
{
    IEntity PCEntity { get; }

    INotifiableList<ICard> HandPile { get; }
    INotifiableList<ICard> DrawPile { get; }
    INotifiableList<ICard> DiscardPile { get; }

    int MaxAP { get; }
    int AP { get; set; }

    /// <summary>距离下一点费用的累积进度，范围 [0, 1]。</summary>
    double APRegenerated { get; }

    /// <summary>每秒回复的费用点数。</summary>
    double APRegeneration { get; }
}
