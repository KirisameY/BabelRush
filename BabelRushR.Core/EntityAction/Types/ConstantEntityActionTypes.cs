using BabelRushR.Core.Battle;
using BabelRushR.Core.Entity.Damage;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

namespace BabelRushR.Core.EntityAction.Types;

public static class ConstantEntityActionTypes
{
    public static IRegistrant<IEntityActionType> Registrant => new InnerRegistrant();


    // Types
    // ReSharper disable UnusedMember.Global

    public static readonly IEntityActionType Attack = New("attack", (action, context, value) =>
    {
        var play = context.GamePlay;
        var side = play.Battle.GetSideOf(context.Actor);
        var target = play.Battle.GetSideEntities(side.Opponent)
                        ?.MinBy(e => Math.Abs(e.Position - context.Actor.Position));
        if (target == null) return;
        DamageSource damageSource = context switch
        {
            CardEntityActionContext c => new CardEntityActionDamageSource(context.Actor, action, c.Card),
            _                         => new EntityActionDamageSource(context.Actor, action)
        };
        var damage = new EntityDamage(damageSource, value);
        play.DamageManager.TryApplyTo(damage, target);
    });

    // ReSharper restore UnusedMember.Global


    private static List<IEntityActionType> EntityActionTypes => field ??= [];

    private static IEntityActionType New(string id, Action<DelegateEntityAction, EntityActionContext> action)
    {
        var type = new DelegateEntityActionType(RegKey.From("core", id), action);
        EntityActionTypes.Add(type);
        return type;
    }

    private static IEntityActionType New(string id, Action<ValuedDelegateEntityAction, EntityActionContext, int> action)
    {
        var type = new ValuedDelegateEntityActionType(RegKey.From("core", id), action);
        EntityActionTypes.Add(type);
        return type;
    }

    private class InnerRegistrant : IRegistrant<IEntityActionType>
    {
        public void AcceptTarget(IRegisterTarget<IEntityActionType> target) => EntityActionTypes.ForEach(type => target.AddItem(type.ID, type));
    }
}