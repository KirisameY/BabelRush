using Godot;

namespace BabelRush;

public static class Project
{
    public const string Name = "BabelRush";

    public const string LogDirPath = "./logs";
    public const int MaxLogFileCount = 16;

    public static Vector2 ViewportSize { get; } = new((float)ProjectSettings.GetSetting("display/window/size/viewport_width"),
                                                      (float)ProjectSettings.GetSetting("display/window/size/viewport_height"));
}