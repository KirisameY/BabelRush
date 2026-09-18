using System.ComponentModel;

using BabelRushR.Core.Card;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Numeric;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;

namespace BabelRushR.Core.GamePlay;

public interface IPlayerState : INotifyPropertyChanged, IUpdatable
{
    IEntity PCEntity { get; }

    INotifiableList<ICard> HandPile { get; }
    INotifiableList<ICard> DrawPile { get; }
    INotifiableList<ICard> DiscardPile { get; }

    /// <summary>费用上限。可挂 Modifier 修改，数值变化时应重新钳制 <see cref="AP"/>。</summary>
    IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxAP { get; }

    /// <summary>当前费用，应被钳制在 [0, <see cref="MaxAP"/>] 内。</summary>
    int AP { get; set; }

    /// <summary>距离下一点费用的累积进度，范围 [0, 1]。</summary>
    double APRegenerated { get; }

    /// <summary>每秒回复的费用点数。</summary>
    IModifierEditableNumeric<double, CommonNumericModifyOrder> APRegeneration { get; }
}
