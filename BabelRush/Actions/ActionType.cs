using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Data;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Actions;

public class ActionType(string id, TargetPattern targetPattern, IEnumerable<ActionStep> actionItems)
{
    public string Id { get; } = id;
    public NameDesc NameDesc => ActionRegisters.ActionNameDesc.GetItem(Id);
    public Texture2D Icon => ActionRegisters.ActionIcon.GetItem(Id);
    public TargetPattern TargetPattern { get; } = targetPattern;
    public IImmutableList<ActionStep> ActionItems { get; } = actionItems.ToImmutableList();

    public Action NewInstance() => new(this);

    public static ActionType Default { get; } = new("default", new TargetPattern.None(), []);
}