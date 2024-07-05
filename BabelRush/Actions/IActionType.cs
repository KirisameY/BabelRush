using Godot;

namespace BabelRush.Actions;

public interface IActionType
{
    Texture2D Icon { get; }
    bool HasValue { get; }

    IAction NewInstance();
}