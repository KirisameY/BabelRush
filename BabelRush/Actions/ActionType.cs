using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Actions;

public class ActionType(string id, string iconId, TargetPattern targetPattern, IEnumerable<ActionStep> actionItems)
{
    public string Id { get; } = id;
    public NameDesc NameDesc => ActionRegisters.ActionNameDesc.GetItem(Id);
    public Texture2D Icon => ActionRegisters.ActionIcon.GetItem(iconId);
    public TargetPattern TargetPattern { get; } = targetPattern;

    [field: AllowNull, MaybeNull]
    public ImmutableList<ActionStep> ActionItems => field ??= actionItems.ToImmutableList();

    public ActionInstance NewInstance(int value) => new(this, value);

    public static ActionType Default { get; } = new("default", "default", new TargetPattern.None(), []);
}