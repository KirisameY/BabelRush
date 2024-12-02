using System;
using System.Diagnostics.CodeAnalysis;

using Godot;

using KirisameLib.Logging;

namespace BabelRush.GamePlay;

public partial class PlayNode : Node
{
    //Getter
    private PlayNode() { }

    [field: AllowNull, MaybeNull]
    private static PackedScene Scene => field ??= ResourceLoader.Load<PackedScene>("res://GamePlay/Play.tscn");

    public static PlayNode GetInstance(Action<double> process)
    {
        var result = Scene.Instantiate<PlayNode>();
        result.Name = "Play";
        result.Process = process;
        return result;
    }


    //Process
    [field: AllowNull, MaybeNull]
    public Action<double> Process
    {
        get
        {
            if (field is not null) return field;
            Logger.Log(LogLevel.Error, "Process", "Process is null! This should not happen!");
            throw new InvalidOperationException("Process is null! This should not happen!");
        }
        set;
    }

    public override void _Process(double delta)
    {
        Process.Invoke(delta);
    }


    //Members
    private Camera? _camera;
    public Camera Camera => _camera ??= GetNode<Camera>("Camera");


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger("PlayNode");
}