using Godot;

namespace BabelRush.Action;

public interface IActionType
{
    Texture2D Icon { get; }
    bool HasValue { get; }

    IAction NewInstance();
}