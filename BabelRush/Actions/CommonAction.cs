using System;
using System.Collections.Generic;
using System.Security.Principal;

using BabelRush.Mobs;

using Godot;

namespace BabelRush.Actions;

public class CommonValueAction(IActionType type, int value, CommonValueAction.ActFunc act) : IAction
{
    public delegate void ActFunc(IMob self, IReadOnlySet<IMob> targets, int value);

    public IActionType Type { get; } = type;
    public int Value { get; } = value;

    public void Act(IMob self, IReadOnlySet<IMob> targets)
    {
        act(self, targets, Value);
    }
}

public class CommonSimpleAction : IAction
{
    private static Dictionary<string, CommonSimpleAction> Actions { get; } = [];

    private CommonSimpleAction(IActionType type, ActFunc act)
    {
        Type = type;
        _act = act;
    }

    public static CommonSimpleAction GetInstance(IActionType type, ActFunc act)
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


    public delegate void ActFunc(IMob self, IReadOnlySet<IMob> targets);

    public IActionType Type { get; }
    public int Value => 0;

    private readonly ActFunc _act;

    public void Act(IMob self, IReadOnlySet<IMob> targets)
    {
        _act(self, targets);
    }
}