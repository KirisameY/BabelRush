using System;
using System.Collections.Generic;

using BabelRush.Mobs;

using Godot;

namespace BabelRush.Actions;

public class ActionType(string id, bool hasValue, Func<IAction> instanceGetter) : IActionType
{
    public string Id { get; } = id;
    public string Name => Registers.ActionName.GetItem(Id);
    public string Description => Registers.ActionDesc.GetItem(Id);
    public Texture2D Icon => Registers.ActionIcon.GetItem(Id);
    public bool HasValue { get; } = hasValue;

    public IAction NewInstance() => instanceGetter();


    public static ActionType Default { get; } =
        new("default", false, () => CommonSimpleAction.GetInstance(Default, (_, _) => { }));
}