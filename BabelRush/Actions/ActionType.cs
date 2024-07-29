using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Registers;

using Godot;

namespace BabelRush.Actions;

public class ActionType(string id, TargetPattern targetPattern, IEnumerable<ActionItem> actionItems)
{
    public string Id { get; } = id;
    public string Name => ActionRegisters.ActionName.GetItem(Id);
    public string Desc => ActionRegisters.ActionDesc.GetItem(Id);
    public Texture2D Icon => ActionRegisters.ActionIcon.GetItem(Id);
    public TargetPattern TargetPattern { get; } = targetPattern;
    public IImmutableList<ActionItem> ActionItems { get; } = actionItems.ToImmutableList();

    public Action NewInstance() => new(this);

    public static ActionType Default { get; } = new("default", TargetPattern.None, []);
}