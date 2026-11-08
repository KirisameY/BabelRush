using System.Runtime.CompilerServices;

using BabelRushR.Core.Entity.Health;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Numeric;

using KirisameY.EventBus.EventComponents;
using KirisameY.GenericUtils;
using KirisameY.Numeric;

namespace BabelRushR.Core.Entity.Damage;

public sealed class DamageManager(IGamePlay gamePlay)
{
    public void TryApplyTo(EntityDamage damage, IEntity target)
    {
        var request = gamePlay.EventBus.Publish(new EntityDamageRequestEvent(damage, target, new()));
        if (request.Canceled) return;

        var value = gamePlay.EventBus.Publish(
            new EntityDamageBeforeEvent(damage, target, INumeric.CreateReadonly(damage.Value, TypeA.Of<CommonNumericModifyOrder>()))
        ).Value.Value;
        var health = target.Health;
        var snapshot = health.Snapshot();

        var temp = value;
        health.Block  -= CalcPart(ref temp, health.Block);
        health.Shield -= CalcPart(ref temp, health.Shield);
        health.HP     -= CalcPart(ref temp, health.HP);
        gamePlay.EventBus.Publish(
            new EntityDamageAfterEvent(damage, target, value, new(snapshot, health.Snapshot()))
        );

        return;

        int CalcPart(ref int restDamage, int part)
        {
            if (restDamage <= 0) return 0;
            var t = Math.Min(restDamage, part);
            restDamage -= t;
            return t;
        }
    }
}

public static class DamageManagerAttachment
{
    private static readonly ConditionalWeakTable<IGamePlay, DamageManager> _cache = [];

    extension(IGamePlay gamePlay)
    {
        public DamageManager DamageManager => _cache.GetOrAdd(gamePlay, play => new(play));
    }
}