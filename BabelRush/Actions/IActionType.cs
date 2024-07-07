using Godot;

namespace BabelRush.Actions;

public interface IActionType
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    Texture2D Icon { get; }
    bool HasValue { get; }

    IAction NewInstance();
}