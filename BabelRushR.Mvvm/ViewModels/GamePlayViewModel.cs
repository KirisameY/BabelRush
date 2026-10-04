using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;

using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     一局游戏的根 VM。视图只需要认识这一个对象。
/// </summary>
public sealed class GamePlayViewModel : ViewModelBase
{
    /// <summary>
    ///     <see cref="IGamePlay"/> 属性名 → 本 VM 中受其影响、需要一并通知的属性名。
    /// </summary>
    private static readonly PropertyChangeForwarder Notifications = new(new Dictionary<string, ImmutableArray<string>>
    {
        [nameof(IGamePlay.Scene)]     = [nameof(Scene)],
        [nameof(IGamePlay.Time)]      = [nameof(Time)],
        [nameof(IGamePlay.DeltaTime)] = [nameof(DeltaTime)],
    }.ToFrozenDictionary());

    private static readonly PropertyChangeReactor<GamePlayViewModel> Updates = new(new Dictionary<string, ImmutableArray<Action<GamePlayViewModel>>>
    {
        [nameof(IGamePlay.Scene)] = [UpdateScene],
    }.ToFrozenDictionary());

    private readonly IGamePlay _gamePlay;

    public GamePlayViewModel(IGamePlay gamePlay)
    {
        _gamePlay = gamePlay;

        Scene = new SceneViewModel(gamePlay.Scene);
        Track(Scene);
        Track(gamePlay.Subscribe(OnGamePlayPropertyChanged));
    }

    #region Properties

    public SceneViewModel Scene { get; private set; }

    /// <summary>
    ///     开局以来的累计时间（秒）。
    /// </summary>
    public double Time => _gamePlay.Time;

    /// <summary>
    ///     上一帧实际推进的时间（秒）。
    /// </summary>
    public double DeltaTime => _gamePlay.DeltaTime;

    /// <summary>
    ///     由视图每帧调用，推进逻辑层。
    /// </summary>
    public void Update(double delta) => _gamePlay.Update(delta);

    #endregion

    #region Reacts

    private static void UpdateScene(GamePlayViewModel instance)
    {
        if (ReferenceEquals(instance.Scene.SourceScene, instance._gamePlay.Scene)) return;

        var (oldScene, newScene) = (instance.Scene, new SceneViewModel(instance._gamePlay.Scene));
        oldScene.Dispose();
        instance.Untrack(oldScene);
        instance.Scene = newScene;
        instance.Track(newScene);
    }

    #endregion

    private void OnGamePlayPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Updates.React(e.PropertyName, this);
        Notifications.Forward(e.PropertyName, OnPropertyChanged);
    }
}