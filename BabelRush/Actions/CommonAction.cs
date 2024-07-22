using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public class CommonValueAction(ActionType type, int value, CommonValueAction.ActFunc act) : Action
{
    public delegate void ActFunc(Mob self, IReadOnlySet<Mob> targets, int value);

    public override ActionType Type { get; } = type;
    public override int Value { get; } = value;

    public override void Act(Mob self, IReadOnlySet<Mob> targets)
    {
        act(self, targets, Value);
    }
}

public class CommonSimpleAction : Action
{
    private static Dictionary<string, CommonSimpleAction> Actions { get; } = [];

    private CommonSimpleAction(ActionType type, ActFunc act)
    {
        Type = type;
        _act = act;
    }

    public static CommonSimpleAction GetInstance(ActionType type, ActFunc act)
    {
        if (Actions.TryGetValue(type.Id, out var res)) return res;
        res = new(type, act);
        Actions.Add(type.Id, res);
        return res;
    }

    public static CommonSimpleAction GetInstance(string id, ActFunc act)
    {
        if (Actions.TryGetValue(id, out var res)) return res;
        res = new(Registers.Actions.GetItem(id), act);
        Actions.Add(id, res);
        return res;
    }


    public delegate void ActFunc(Mob self, IReadOnlySet<Mob> targets);

    public override ActionType Type { get; }
    public override int Value => 0;

    private readonly ActFunc _act;

    public override void Act(Mob self, IReadOnlySet<Mob> targets)
    {
        _act(self, targets);
    }
}