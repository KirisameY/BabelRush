using BabelRushR.Core.Battle;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;

namespace BabelRushR.Core.EntityAction.Types;

public static class ConstantEntityActionTypes
{
    public static IRegistrant<IEntityActionType> Registrant => new InnerRegistrant();


    // Types
    // ReSharper disable UnusedMember.Global

    public static readonly IEntityActionType Attack = New("attack", (context, value) =>
    {
        var play = context.GamePlay;
        var side = play.Battle.GetSideOf(context.Actor);
        play.Battle.GetSideEntities(side.Opponent)
           ?.MinBy(e => Math.Abs(e.Position - context.Actor.Position))
           ?.Health.HP -= value; // todo: 轉 attack
    });

    // ReSharper restore UnusedMember.Global


    private static List<IEntityActionType> EntityActionTypes => field ??= [];

    private static IEntityActionType New(string id, Action<EntityActionContext> action)
    {
        var type = new DelegateEntityActionType(RegKey.From("core", id), action);
        EntityActionTypes.Add(type);
        return type;
    }

    private static IEntityActionType New(string id, Action<EntityActionContext, int> action)
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