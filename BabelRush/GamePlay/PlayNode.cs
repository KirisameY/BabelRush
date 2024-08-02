using System;

using Godot;

using KirisameLib.Logging;

namespace BabelRush.GamePlay;

public partial class PlayNode : Node
{
    public static PlayNode GetInstance(Action<double> process)
    {
        // Note: Temp!
        var result = new PlayNode();
        result.Process = process;
        return result;
    }


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


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("PlayNode");
}