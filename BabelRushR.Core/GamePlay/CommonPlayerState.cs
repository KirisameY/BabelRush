using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Numeric;

using KirisameY.Numeric;

namespace BabelRushR.Core.GamePlay;

public class CommonPlayerState(
    IEntity pcEntity,
    int maxAP = 6,
    double apRegeneration = 1.0)
    : ObservableObject, IPlayerState
{
    internal void Initialize(IGamePlay gamePlay) => GamePlay = gamePlay;
    void IPlayerState.Initialize(IGamePlay gamePlay) => Initialize(gamePlay);

    private IGamePlay GamePlay
    {
        get => field ?? throw new Exception($"{nameof(CommonPlayerState)} uninitialized");
        set;
    }

    public IEntity PCEntity => pcEntity;

    public ICardDeck CardDeck => field ??= new CommonCardDeck(GamePlay);

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

            GamePlay.EventBus.Publish(new PlayerAPChangedEvent(old, clamped));
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
        if (AP >= MaxAP.Value && APRegenerated >= 1.0) return;

        APRegenerated += APRegeneration.Value * delta;

        while (APRegenerated >= 1.0)
        {
            if (AP >= MaxAP.Value)
            {
                APRegenerated = 1.0;
                break;
            }

            APRegenerated -= 1.0;
            AP++; // 将经由属性设置器发布 APChangedEvent
        }
    }
}