using System.Collections.Immutable;

using BabelRushR.Core.EntityAction.Types;

using KirisameY.Registration.Data;
using KirisameY.Registration.Registering;
using KirisameY.Relinq.Extensions;


namespace BabelRushR.Core.Card.Types;

public static class ConstantCardTypes
{
    public static IRegistrant<ICardType> Registrant => new InnerRegistrant();


    // Types
    // ReSharper disable UnusedMember.Global

    public static readonly ICardType Strike = New("strike", 1, [
        // todo: 目标选择还没接（EntityActionContext 目前只带 Actor），先只保留数值
        (new ValuedDelegateEntityActionType((_, _) => { }), 6)
    ]);

    public static readonly ICardType Defense = New("defense", 1, []);

    // ReSharper restore UnusedMember.Global


    private static List<ICardType> CardTypes => field ??= [];

    private static ICardType New(string id, int cost, ImmutableArray<(IEntityActionType Type, int Value)> actions)
    {
        var type = new RecordCardType(RegKey.From("babelrush", id), cost, actions);
        CardTypes.Add(type);
        return type;
    }

    private class InnerRegistrant : IRegistrant<ICardType>
    {
        public void AcceptTarget(IRegisterTarget<ICardType> target) => CardTypes.ForEach(type => target.AddItem(type.Id, type));
    }
}