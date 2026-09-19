using BabelRushR.Core.Card;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Numeric;

using KirisameY.EventBus;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;

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

    public IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxAP =>
        field ??= INumeric.CreateReadonly<int, CommonNumericModifyOrder>(maxAP)
                          .WithUpdateHandler((_, _) => AP = AP)         // 上限下调时把当前 AP 一起削下来
                          .WithUpdateHandler(PropertyChangedHandler()); // 再通知

    public int AP
    {
        get;
        set
        {
            var clamped = Math.Clamp(value, 0, MaxAP.Value);
            var old = field;
            if (!SetProperty(ref field, clamped)) return;

            eventBus.Publish(new APChangedEvent(old, clamped));
        }
    }

    public double APRegenerated
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public IModifierEditableNumeric<double, CommonNumericModifyOrder> APRegeneration =>
        field ??= INumeric.CreateReadonly<double, CommonNumericModifyOrder>(apRegeneration)
                          .WithUpdateHandler(PropertyChangedHandler());

    public void Update(double delta)
    {
        // 满费锁1
        if (AP >= MaxAP.Value)
        {
            APRegenerated = 1.0;
            return;
        }

        APRegenerated += APRegeneration.Value * delta;

        while (APRegenerated >= 1.0)
        {
            APRegenerated -= 1.0;
            AP++; // 将经由属性设置器发布 APChangedEvent

            if (AP < MaxAP.Value) continue;

            APRegenerated = 1.0;
            break;
        }
    }
}