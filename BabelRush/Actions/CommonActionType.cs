using System;

using Godot;

namespace BabelRush.Actions;

public class CommonActionType(string id, bool hasValue, Func<Action> instanceGetter) : ActionType
{
    public override string Id { get; } = id;
    public override string Name => Registers.ActionName.GetItem(Id);
    public override string Description => Registers.ActionDesc.GetItem(Id);
    public override Texture2D Icon => Registers.ActionIcon.GetItem(Id);
    public override bool HasValue { get; } = hasValue;

    public override Action NewInstance() => instanceGetter();
}