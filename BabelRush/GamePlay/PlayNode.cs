using System;

using Godot;

using KirisameLib.Core.Logging;

namespace BabelRush.GamePlay;

public partial class PlayNode : Node
{
    //Getter
    private PlayNode() { }

    private static PackedScene? _scene;
    private static PackedScene Scene => _scene ??= ResourceLoader.Load<PackedScene>("res://GamePlay/Play.tscn");

    public static PlayNode GetInstance(Action<double> process)
    {
        var result = Scene.Instantiate<PlayNode>();
        result.Name = "Play";
        result.Process = process;
        return result;
    }


    //Process
    private Action<double>? _process;
    public Action<double> Process
    {
        get
        {
            if (_process is not null) return _process;
            Logger.Log(LogLevel.Error, "Process", "Process is null! This should not happen!");
            throw new InvalidOperationException("Process is null! This should not happen!");
        }
        set => _process = value;
    }

    public override void _Process(double delta)
    {
        Process.Invoke(delta);
    }


    //Members
    private Camera? _camera;
    public Camera Camera => _camera ??= GetNode<Camera>("Camera");


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("PlayNode");
}