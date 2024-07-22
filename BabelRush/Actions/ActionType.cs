using Godot;

namespace BabelRush.Actions;

public abstract class ActionType
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Icon { get; }
    public abstract bool HasValue { get; }
    public abstract Action NewInstance();

    public static ActionType Default { get; } =
        new CommonActionType("default", false, () => CommonSimpleAction.GetInstance("default", (_, _) => { }));
}