using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     一局游戏的根 VM。视图只需要认识这一个对象。
/// </summary>
public sealed class GamePlayViewModel(IGamePlay gamePlay) : ViewModelBase<GamePlayViewModel>(gamePlay, Reactions)
{
    /// <summary>
    ///     <see cref="IGamePlay"/> 属性名 → 该变化引发的更新与需要一并通知的属性名。
    /// </summary>
    private static readonly PropertyChangeReactor<GamePlayViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<GamePlayViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(IGamePlay.Scene)]     = ([UpdateScene], [nameof(Scene)]),
            [nameof(IGamePlay.Time)]      = ([], [nameof(Time)]),
            [nameof(IGamePlay.DeltaTime)] = ([], [nameof(DeltaTime)]),
        }.ToFrozenDictionary()
    );

    private readonly IGamePlay _gamePlay = gamePlay;


    #region Properties

    public SceneViewModel Scene
    {
        get => field ??= Track(new SceneViewModel(_gamePlay.Scene));
        private set
        {
            if (field == value) return;
            // ReSharper disable AssignNullToNotNullAttribute
            (var old, field) = (field, Track(value));
            // ReSharper restore AssignNullToNotNullAttribute
            if (old is null) return;
            Untrack(old);
            old.Dispose();
        }
    }

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
        instance.Scene = new SceneViewModel(instance._gamePlay.Scene);
    }

    #endregion
}