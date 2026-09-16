using BabelRushR.Core.Card;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.EventBus;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay;

public class CommonPlayerState(
    IEntity pcEntity,
    IEventBus eventBus,
    int maxAP = 3,
    double apRegeneration = 1.0)
    : ObservableObject, IPlayerState
{
    public IEventBus EventBus => eventBus;

    public IEntity PCEntity => pcEntity;

    public INotifiableList<ICard> HandPile { get; } = new NotifiableList<ICard>();
    public INotifiableList<ICard> DrawPile { get; } = new NotifiableList<ICard>();
    public INotifiableList<ICard> DiscardPile { get; } = new NotifiableList<ICard>();

    public int MaxAP => maxAP;

    public int AP
    {
        get;
        set
        {
            var clamped = Math.Clamp(value, 0, MaxAP);
            var old = field;
            if (!SetProperty(ref field, clamped)) return;

            // 属性访问器无法把 Order 交给调用方，这里自行接管分发。
            eventBus.OrderPost(new APChangedEvent(old, clamped)).Submit();
        }
    }

    public double APRegenerated
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double APRegeneration => apRegeneration;

    public void Update(double delta)
    {
        // 满费时锁在 1：此时花掉一点，下一帧立刻回满。这是刻意的设计，不是漏写重置。
        if (AP >= MaxAP)
        {
            APRegenerated = 1.0;
            return;
        }

        APRegenerated += APRegeneration * delta;

        while (APRegenerated >= 1.0)
        {
            APRegenerated -= 1.0;
            AP++; // 经由属性设置器发布 APChangedEvent

            if (AP < MaxAP) continue;

            APRegenerated = 1.0;
            break;
        }
    }
}
