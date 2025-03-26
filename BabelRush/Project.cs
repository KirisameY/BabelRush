using Godot;

using KirisameLib.Logging;

namespace BabelRush;

public static class Project
{
    public const string Name = "BabelRush";
    public const string NameSpace = "babelrush";

    public static class Logging
    {
        public const string LogDirPath = "./logs";
        public const int MaxLogFileCount = 16;
    #if DEBUG
        public const LogLevel MinLogLevel = LogLevel.Debug;
    #else
        public const LogLevel MinLogLevel = LogLevel.Info;
    #endif
    }


    public static Vector2 ViewportSize { get; } = new((float)ProjectSettings.GetSetting("display/window/size/viewport_width"),
                                                      (float)ProjectSettings.GetSetting("display/window/size/viewport_height"));
}