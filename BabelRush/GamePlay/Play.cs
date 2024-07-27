using System;

using Godot;

using KirisameLib.Logging;

namespace BabelRush.GamePlay;

public partial class Play : Node2D
{
    //Singleton
    private Play(PlayState state)//Todo
    {
        _state = state;
    }

    private static Play? _instance;
    public static Play Instance
    {
        get
        {
            if (_instance is not null) return _instance;
            Logger.Log(LogLevel.Error, "GettingInstance", "GamePlay did not initialized");
            throw new GamePlayNotInitializedException();
        }
        private set => _instance = value;
    }


    //Member
    private readonly PlayState _state;
    public static PlayState State => Instance._state;


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}